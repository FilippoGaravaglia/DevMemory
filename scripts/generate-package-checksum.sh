#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_PROJECT="$ROOT_DIR/src/DevMemory.Cli/DevMemory.Cli.csproj"
PACKAGE_DIR="$ROOT_DIR/artifacts/packages"
PACKAGE_ID="DevMemory.Cli"

print_step() {
  echo
  echo "$1"
  printf '%s\n' "$1" | sed 's/./-/g'
}

extract_project_version() {
  local project_file="$1"

  local version
  version="$(grep -E "<Version>.*</Version>" "$project_file" | sed -E 's/.*<Version>(.*)<\/Version>.*/\1/' | head -n 1)"

  if [ -z "$version" ]; then
    echo "Unable to read <Version> from:"
    echo "  $project_file"
    exit 1
  fi

  echo "$version"
}

create_checksum() {
  local package_path="$1"
  local checksum_path="$2"

  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum "$package_path" > "$checksum_path"
    return
  fi

  if command -v shasum >/dev/null 2>&1; then
    shasum -a 256 "$package_path" > "$checksum_path"
    return
  fi

  echo "Required SHA-256 checksum command was not found."
  echo "Expected one of:"
  echo "  sha256sum"
  echo "  shasum"
  exit 1
}

verify_checksum() {
  local package_path="$1"
  local checksum_path="$2"

  if command -v sha256sum >/dev/null 2>&1; then
    (cd "$(dirname "$package_path")" && sha256sum -c "$(basename "$checksum_path")")
    return
  fi

  if command -v shasum >/dev/null 2>&1; then
    local expected_hash
    local actual_hash

    expected_hash="$(awk '{ print $1 }' "$checksum_path")"
    actual_hash="$(shasum -a 256 "$package_path" | awk '{ print $1 }')"

    if [ "$expected_hash" != "$actual_hash" ]; then
      echo "Checksum verification failed."
      echo "Expected: $expected_hash"
      echo "Actual:   $actual_hash"
      exit 1
    fi

    echo "$package_path: OK"
    return
  fi

  echo "Required SHA-256 checksum command was not found."
  exit 1
}

echo "Generating DevMemory package checksum..."

if [ ! -f "$CLI_PROJECT" ]; then
  echo "CLI project file not found:"
  echo "  $CLI_PROJECT"
  exit 1
fi

print_step "Step 1/4 - Resolve package artifact"
PROJECT_VERSION="$(extract_project_version "$CLI_PROJECT")"
PACKAGE_PATH="$PACKAGE_DIR/$PACKAGE_ID.$PROJECT_VERSION.nupkg"
CHECKSUM_PATH="$PACKAGE_PATH.sha256"

echo "Project version: $PROJECT_VERSION"
echo "Package path:    $PACKAGE_PATH"
echo "Checksum path:   $CHECKSUM_PATH"

if [ ! -f "$PACKAGE_PATH" ]; then
  echo
  echo "Package artifact not found."
  echo "Run package verification first:"
  echo "  ./scripts/verify-package-artifact.sh"
  exit 1
fi

if [ ! -s "$PACKAGE_PATH" ]; then
  echo "Package artifact exists but is empty:"
  echo "  $PACKAGE_PATH"
  exit 1
fi

print_step "Step 2/4 - Create checksum"
rm -f "$CHECKSUM_PATH"
create_checksum "$PACKAGE_PATH" "$CHECKSUM_PATH"

if [ ! -s "$CHECKSUM_PATH" ]; then
  echo "Checksum file was not created correctly:"
  echo "  $CHECKSUM_PATH"
  exit 1
fi

echo "Checksum created:"
echo "  $CHECKSUM_PATH"

print_step "Step 3/4 - Verify checksum"
verify_checksum "$PACKAGE_PATH" "$CHECKSUM_PATH"

print_step "Step 4/4 - Print release artifact summary"
echo "Package:"
echo "  $PACKAGE_PATH"
echo
echo "Checksum:"
echo "  $CHECKSUM_PATH"

echo
echo "DevMemory package checksum generated and verified successfully."