#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

PACKAGE_ID="${DEVMEMORY_PACKAGE_ID:-DevMemory.Cli}"
TOOL_COMMAND="${DEVMEMORY_TOOL_COMMAND:-devmemory}"
BUILD_CONFIGURATION="${BUILD_CONFIGURATION:-Release}"
PACKAGE_DIR="${DEVMEMORY_PACKAGE_DIR:-$ROOT_DIR/artifacts/packages}"

TEMP_DIR="$(mktemp -d)"
TOOL_DIR="$TEMP_DIR/tool"
DEVMEMORY_HOME_DIR="$TEMP_DIR/devmemory-home"

cleanup() {
  rm -rf "$TEMP_DIR"
}

trap cleanup EXIT

echo "Running DevMemory CLI package smoke test..."
echo
echo "Package id:       $PACKAGE_ID"
echo "Tool command:     $TOOL_COMMAND"
echo "Configuration:    $BUILD_CONFIGURATION"
echo "Package dir:      $PACKAGE_DIR"
echo

mkdir -p "$PACKAGE_DIR"
mkdir -p "$TOOL_DIR"
mkdir -p "$DEVMEMORY_HOME_DIR"

echo "Cleaning previous local packages..."
rm -f "$PACKAGE_DIR"/*.nupkg
echo

echo "Packing CLI from current source..."
dotnet pack "$ROOT_DIR/src/DevMemory.Cli/DevMemory.Cli.csproj" \
  --configuration "$BUILD_CONFIGURATION" \
  --output "$PACKAGE_DIR"

echo
echo "Available packages:"
find "$PACKAGE_DIR" -maxdepth 1 -name "*.nupkg" -print | sort
echo

echo "Installing CLI tool from local package source..."
dotnet tool install "$PACKAGE_ID" \
  --tool-path "$TOOL_DIR" \
  --add-source "$PACKAGE_DIR" \
  --ignore-failed-sources

echo
echo "Installed tools:"
dotnet tool list --tool-path "$TOOL_DIR"
echo

export DEVMEMORY_HOME="$DEVMEMORY_HOME_DIR"

TOOL_PATH="$TOOL_DIR/$TOOL_COMMAND"

if [ ! -x "$TOOL_PATH" ]; then
  echo "Expected tool executable not found: $TOOL_PATH"
  exit 1
fi

echo "Running installed CLI smoke commands..."
echo

"$TOOL_PATH" --version
echo

"$TOOL_PATH" help
echo

"$TOOL_PATH" storage
echo

"$TOOL_PATH" ai-status
echo

"$TOOL_PATH" index --dry-run
echo

"$TOOL_PATH" ai-doctor || true
echo

echo "DevMemory CLI package smoke test completed successfully."