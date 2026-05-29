#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_PROJECT="$ROOT_DIR/src/DevMemory.Cli/DevMemory.Cli.csproj"
PACKAGE_DIR="$ROOT_DIR/artifacts/packages"
PACKAGE_ID="DevMemory.Cli"
TOOL_COMMAND="devmemory"

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

require_command() {
  local command_name="$1"

  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "Required command not found: $command_name"
    exit 1
  fi
}

echo "Verifying DevMemory NuGet package artifact..."

if [ ! -f "$CLI_PROJECT" ]; then
  echo "CLI project file not found:"
  echo "  $CLI_PROJECT"
  exit 1
fi

require_command "unzip"

print_step "Step 1/5 - Read project version"
PROJECT_VERSION="$(extract_project_version "$CLI_PROJECT")"
EXPECTED_PACKAGE_NAME="$PACKAGE_ID.$PROJECT_VERSION.nupkg"

echo "Project version:       $PROJECT_VERSION"
echo "Expected package name: $EXPECTED_PACKAGE_NAME"

print_step "Step 2/5 - Pack CLI"
rm -rf "$PACKAGE_DIR"
mkdir -p "$PACKAGE_DIR"

dotnet pack "$CLI_PROJECT" \
  --configuration Release \
  --output "$PACKAGE_DIR"

print_step "Step 3/5 - Locate package artifact"
PACKAGE_PATH="$PACKAGE_DIR/$EXPECTED_PACKAGE_NAME"

if [ ! -f "$PACKAGE_PATH" ]; then
  echo "Expected package artifact was not found:"
  echo "  $PACKAGE_PATH"
  echo
  echo "Available packages:"
  find "$PACKAGE_DIR" -maxdepth 1 -type f -name "*.nupkg" -print | sort
  exit 1
fi

PACKAGE_COUNT="$(find "$PACKAGE_DIR" -maxdepth 1 -type f -name "*.nupkg" | wc -l | tr -d ' ')"

if [ "$PACKAGE_COUNT" != "1" ]; then
  echo "Expected exactly one .nupkg artifact, but found: $PACKAGE_COUNT"
  find "$PACKAGE_DIR" -maxdepth 1 -type f -name "*.nupkg" -print | sort
  exit 1
fi

echo "Package artifact found:"
echo "  $PACKAGE_PATH"

print_step "Step 4/5 - Verify package structure"
PACKAGE_LIST_FILE="$(mktemp)"
unzip -l "$PACKAGE_PATH" > "$PACKAGE_LIST_FILE"

if ! grep -q "tools/net10.0/any/DevMemory.Cli.dll" "$PACKAGE_LIST_FILE"; then
  echo "Package does not contain the expected CLI assembly."
  echo "Expected path:"
  echo "  tools/net10.0/any/DevMemory.Cli.dll"
  exit 1
fi

if ! grep -q "tools/net10.0/any/DotnetToolSettings.xml" "$PACKAGE_LIST_FILE"; then
  echo "Package does not contain DotnetToolSettings.xml."
  exit 1
fi

if ! grep -q "$PACKAGE_ID.nuspec" "$PACKAGE_LIST_FILE"; then
  echo "Package does not contain the expected nuspec file."
  echo "Expected:"
  echo "  $PACKAGE_ID.nuspec"
  exit 1
fi

echo "Package structure is valid."

print_step "Step 5/5 - Verify tool command metadata"
TOOL_SETTINGS="$(unzip -p "$PACKAGE_PATH" "tools/net10.0/any/DotnetToolSettings.xml")"

if ! printf '%s\n' "$TOOL_SETTINGS" | grep -q "<Command Name=\"$TOOL_COMMAND\""; then
  echo "DotnetToolSettings.xml does not declare the expected command name."
  echo "Expected command:"
  echo "  $TOOL_COMMAND"
  echo
  echo "Actual DotnetToolSettings.xml:"
  printf '%s\n' "$TOOL_SETTINGS"
  exit 1
fi

echo "Tool command metadata is valid:"
echo "  $TOOL_COMMAND"

echo
echo "DevMemory NuGet package artifact verification completed successfully."