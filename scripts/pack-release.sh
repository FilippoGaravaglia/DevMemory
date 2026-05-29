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

echo "Preparing DevMemory release package..."

if [ ! -f "$CLI_PROJECT" ]; then
  echo "CLI project file not found:"
  echo "  $CLI_PROJECT"
  exit 1
fi

PROJECT_VERSION="$(extract_project_version "$CLI_PROJECT")"
PACKAGE_PATH="$PACKAGE_DIR/$PACKAGE_ID.$PROJECT_VERSION.nupkg"
CHECKSUM_PATH="$PACKAGE_PATH.sha256"

echo
echo "Package id:      $PACKAGE_ID"
echo "Version:         $PROJECT_VERSION"
echo "Package dir:     $PACKAGE_DIR"
echo "Package path:    $PACKAGE_PATH"
echo "Checksum path:   $CHECKSUM_PATH"

print_step "Step 1/3 - Run release check"
"$ROOT_DIR/scripts/release-check.sh"

print_step "Step 2/3 - Validate final release artifacts"

if [ ! -f "$PACKAGE_PATH" ]; then
  echo "Expected package artifact was not found:"
  echo "  $PACKAGE_PATH"
  exit 1
fi

if [ ! -s "$PACKAGE_PATH" ]; then
  echo "Package artifact exists but is empty:"
  echo "  $PACKAGE_PATH"
  exit 1
fi

if [ ! -f "$CHECKSUM_PATH" ]; then
  echo "Expected checksum artifact was not found:"
  echo "  $CHECKSUM_PATH"
  exit 1
fi

if [ ! -s "$CHECKSUM_PATH" ]; then
  echo "Checksum artifact exists but is empty:"
  echo "  $CHECKSUM_PATH"
  exit 1
fi

echo "Release artifacts are available."

print_step "Step 3/3 - Print release summary"

echo "Package:"
echo "  $PACKAGE_PATH"
echo
echo "Checksum:"
echo "  $CHECKSUM_PATH"
echo
echo "Artifacts directory:"
echo "  $PACKAGE_DIR"
echo
echo "Available artifacts:"
find "$PACKAGE_DIR" -maxdepth 1 -type f \( -name "*.nupkg" -o -name "*.sha256" \) -print | sort

echo
echo "DevMemory release package is ready."