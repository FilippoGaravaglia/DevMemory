#!/usr/bin/env bash
set -euo pipefail

TOOL_COMMAND="${DEVMEMORY_TOOL_COMMAND:-devmemory}"
BACKUP_DIR="${DEVMEMORY_BACKUP_DIR:-$HOME/.devmemory/backups}"
TIMESTAMP="$(date +"%Y%m%d_%H%M%S")"

print_step() {
  echo
  echo "$1"
  printf '%s\n' "$1" | sed 's/./-/g'
}

create_checksum() {
  local file_path="$1"
  local checksum_path="$2"

  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum "$file_path" > "$checksum_path"
    return
  fi

  if command -v shasum >/dev/null 2>&1; then
    shasum -a 256 "$file_path" > "$checksum_path"
    return
  fi

  echo "SHA-256 checksum tool not found. Skipping checksum creation."
}

echo "Backing up DevMemory local data..."
echo
echo "Tool command: $TOOL_COMMAND"
echo "Backup dir:   $BACKUP_DIR"
echo

if ! command -v "$TOOL_COMMAND" >/dev/null 2>&1; then
  echo "DevMemory CLI tool was not found in PATH."
  echo
  echo "Install it first with:"
  echo "  ./scripts/install-local-tool.sh"
  exit 1
fi

print_step "Step 1/4 - Resolve storage path"
STORAGE_PATH="$("$TOOL_COMMAND" storage | tail -n 1)"

if [ -z "$STORAGE_PATH" ]; then
  echo "Unable to resolve DevMemory storage path."
  exit 1
fi

echo "$STORAGE_PATH"

print_step "Step 2/4 - Validate storage file"

if [ ! -f "$STORAGE_PATH" ]; then
  echo "DevMemory storage file does not exist yet:"
  echo "  $STORAGE_PATH"
  echo
  echo "Create at least one memory first with:"
  echo "  devmemory add"
  exit 1
fi

if [ ! -s "$STORAGE_PATH" ]; then
  echo "DevMemory storage file exists but is empty:"
  echo "  $STORAGE_PATH"
  exit 1
fi

echo "Storage file is available."

print_step "Step 3/4 - Create backup"
mkdir -p "$BACKUP_DIR"

BACKUP_FILE="$BACKUP_DIR/devmemory_$TIMESTAMP.json"
cp "$STORAGE_PATH" "$BACKUP_FILE"

echo "Backup created:"
echo "  $BACKUP_FILE"

print_step "Step 4/4 - Create checksum"
CHECKSUM_FILE="$BACKUP_FILE.sha256"
create_checksum "$BACKUP_FILE" "$CHECKSUM_FILE"

if [ -f "$CHECKSUM_FILE" ]; then
  echo "Checksum created:"
  echo "  $CHECKSUM_FILE"
fi

echo
echo "DevMemory local data backup completed successfully."