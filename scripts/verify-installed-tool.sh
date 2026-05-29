#!/usr/bin/env bash
set -euo pipefail

TOOL_COMMAND="${DEVMEMORY_TOOL_COMMAND:-devmemory}"
TEMP_DIR="$(mktemp -d)"
DEVMEMORY_HOME_DIR="$TEMP_DIR/devmemory-home"

cleanup() {
  rm -rf "$TEMP_DIR"
}

trap cleanup EXIT

export DEVMEMORY_HOME="$DEVMEMORY_HOME_DIR"

print_step() {
  echo
  echo "$1"
  printf '%s\n' "$1" | sed 's/./-/g'
}

echo "Verifying installed DevMemory CLI tool..."
echo
echo "Tool command:   $TOOL_COMMAND"
echo "Temporary home: $DEVMEMORY_HOME"
echo

if ! command -v "$TOOL_COMMAND" >/dev/null 2>&1; then
  echo "DevMemory CLI tool was not found in PATH."
  echo
  echo "Install it first with:"
  echo "  ./scripts/install-local-tool.sh"
  echo
  echo "Or install from a packed NuGet package with:"
  echo "  dotnet tool install --global DevMemory.Cli --add-source artifacts/packages"
  exit 1
fi

print_step "Step 1/6 - Show installed tool path"
command -v "$TOOL_COMMAND"

print_step "Step 2/6 - Show version"
"$TOOL_COMMAND" version

print_step "Step 3/6 - Show help"
"$TOOL_COMMAND" --help

print_step "Step 4/6 - Show storage path"
"$TOOL_COMMAND" storage

print_step "Step 5/6 - Show AI status"
"$TOOL_COMMAND" ai-status

print_step "Step 6/6 - Run index dry-run"
"$TOOL_COMMAND" index --dry-run

echo
echo "Installed DevMemory CLI tool verification completed successfully."