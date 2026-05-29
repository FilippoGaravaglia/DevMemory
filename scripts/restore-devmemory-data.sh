#!/usr/bin/env bash
set -euo pipefail

TOOL_COMMAND="${DEVMEMORY_TOOL_COMMAND:-devmemory}"
BACKUP_DIR="${DEVMEMORY_BACKUP_DIR:-$HOME/.devmemory/backups}"
TIMESTAMP="$(date +"%Y%m%d_%H%M%S")"

BACKUP_FILE=""
ASSUME_YES="false"
DRY_RUN="false"

print_usage() {
  cat <<EOF
Restore DevMemory local data from a backup file.

Usage:
  ./scripts/restore-devmemory-data.sh <backup-file> [--yes] [--dry-run]

Options:
  --yes      Skip interactive confirmation.
  --dry-run  Validate restore inputs without modifying the storage file.

Examples:
  ./scripts/restore-devmemory-data.sh ~/.devmemory/backups/devmemory_20260529_185639.json --dry-run
  ./scripts/restore-devmemory-data.sh ~/.devmemory/backups/devmemory_20260529_185639.json
  ./scripts/restore-devmemory-data.sh ~/.devmemory/backups/devmemory_20260529_185639.json --yes
EOF
}

print_step() {
  echo
  echo "$1"
  printf '%s\n' "$1" | sed 's/./-/g'
}

parse_args() {
  if [ "$#" -eq 0 ]; then
    print_usage
    exit 1
  fi

  while [ "$#" -gt 0 ]; do
    case "$1" in
      --yes)
        ASSUME_YES="true"
        shift
        ;;

      --dry-run)
        DRY_RUN="true"
        shift
        ;;

      --help|-h)
        print_usage
        exit 0
        ;;

      --*)
        echo "Unknown option: $1"
        echo
        print_usage
        exit 1
        ;;

      *)
        if [ -n "$BACKUP_FILE" ]; then
          echo "Only one backup file can be provided."
          echo
          print_usage
          exit 1
        fi

        BACKUP_FILE="$1"
        shift
        ;;
    esac
  done

  if [ -z "$BACKUP_FILE" ]; then
    echo "Backup file is required."
    echo
    print_usage
    exit 1
  fi
}

verify_checksum_if_available() {
  local backup_file="$1"
  local checksum_file="$backup_file.sha256"

  if [ ! -f "$checksum_file" ]; then
    echo "Checksum file not found. Skipping checksum validation."
    echo "Expected checksum path:"
    echo "  $checksum_file"
    return
  fi

  if command -v sha256sum >/dev/null 2>&1; then
    (cd "$(dirname "$backup_file")" && sha256sum -c "$(basename "$checksum_file")")
    return
  fi

  if command -v shasum >/dev/null 2>&1; then
    local expected_hash
    local actual_hash

    expected_hash="$(awk '{ print $1 }' "$checksum_file")"
    actual_hash="$(shasum -a 256 "$backup_file" | awk '{ print $1 }')"

    if [ "$expected_hash" != "$actual_hash" ]; then
      echo "Checksum validation failed."
      echo "Expected: $expected_hash"
      echo "Actual:   $actual_hash"
      exit 1
    fi

    echo "$backup_file: OK"
    return
  fi

  echo "SHA-256 checksum tool not found. Skipping checksum validation."
}

confirm_restore() {
  local storage_path="$1"
  local backup_file="$2"

  if [ "$ASSUME_YES" = "true" ]; then
    return
  fi

  echo
  echo "You are about to replace the current DevMemory storage file."
  echo
  echo "Current storage:"
  echo "  $storage_path"
  echo
  echo "Backup source:"
  echo "  $backup_file"
  echo
  printf "Type 'restore' to continue: "

  local confirmation
  read -r confirmation

  if [ "$confirmation" != "restore" ]; then
    echo "Restore cancelled."
    exit 1
  fi
}

create_current_backup() {
  local storage_path="$1"

  if [ ! -f "$storage_path" ]; then
    echo "Current storage file does not exist. No pre-restore backup created."
    return
  fi

  mkdir -p "$BACKUP_DIR"

  local pre_restore_backup="$BACKUP_DIR/devmemory_pre_restore_$TIMESTAMP.json"

  cp "$storage_path" "$pre_restore_backup"

  echo "Current storage backup created:"
  echo "  $pre_restore_backup"
}

parse_args "$@"

echo "Restoring DevMemory local data..."
echo
echo "Tool command: $TOOL_COMMAND"
echo "Backup file:  $BACKUP_FILE"
echo "Backup dir:   $BACKUP_DIR"
echo "Dry-run:      $DRY_RUN"
echo

if ! command -v "$TOOL_COMMAND" >/dev/null 2>&1; then
  echo "DevMemory CLI tool was not found in PATH."
  echo
  echo "Install it first with:"
  echo "  ./scripts/install-local-tool.sh"
  exit 1
fi

print_step "Step 1/5 - Resolve storage path"
STORAGE_PATH="$("$TOOL_COMMAND" storage | tail -n 1)"

if [ -z "$STORAGE_PATH" ]; then
  echo "Unable to resolve DevMemory storage path."
  exit 1
fi

echo "$STORAGE_PATH"

print_step "Step 2/5 - Validate backup file"

if [ ! -f "$BACKUP_FILE" ]; then
  echo "Backup file does not exist:"
  echo "  $BACKUP_FILE"
  exit 1
fi

if [ ! -s "$BACKUP_FILE" ]; then
  echo "Backup file exists but is empty:"
  echo "  $BACKUP_FILE"
  exit 1
fi

echo "Backup file is available."

print_step "Step 3/5 - Validate checksum"
verify_checksum_if_available "$BACKUP_FILE"

print_step "Step 4/5 - Prepare restore"

if [ "$DRY_RUN" = "true" ]; then
  echo "Dry-run enabled. No files will be modified."
  echo
  echo "Restore source:"
  echo "  $BACKUP_FILE"
  echo
  echo "Restore target:"
  echo "  $STORAGE_PATH"
  echo
  echo "DevMemory local data restore dry-run completed successfully."
  exit 0
fi

confirm_restore "$STORAGE_PATH" "$BACKUP_FILE"

print_step "Step 5/5 - Restore backup"
create_current_backup "$STORAGE_PATH"

mkdir -p "$(dirname "$STORAGE_PATH")"
cp "$BACKUP_FILE" "$STORAGE_PATH"

echo "Backup restored to:"
echo "  $STORAGE_PATH"

echo
echo "DevMemory local data restore completed successfully."