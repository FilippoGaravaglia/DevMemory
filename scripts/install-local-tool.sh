#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

CLI_PROJECT="$ROOT_DIR/src/DevMemory.Cli/DevMemory.Cli.csproj"
SOLUTION_FILE="$ROOT_DIR/DevMemory.slnx"
PACKAGES_DIR="$ROOT_DIR/artifacts/packages"

PACKAGE_ID="DevMemory.Cli"
TOOL_COMMAND="devmemory"

cd "$ROOT_DIR"

echo "Building solution..."
dotnet build "$SOLUTION_FILE"

echo
echo "Running tests..."
dotnet test "$SOLUTION_FILE" --no-build

echo
echo "Cleaning local package output..."
rm -rf "$PACKAGES_DIR"
mkdir -p "$PACKAGES_DIR"

echo
echo "Packing CLI tool..."
dotnet pack "$CLI_PROJECT" -c Release -o "$PACKAGES_DIR"

echo
echo "Installing local global tool..."

if dotnet tool list --global | grep -qi "^${PACKAGE_ID}[[:space:]]"; then
    echo "Existing global tool installation found. Uninstalling..."
    dotnet tool uninstall --global "$PACKAGE_ID"
fi

dotnet tool install --global "$PACKAGE_ID" --add-source "$PACKAGES_DIR"

echo
echo "Verifying installed command..."
"$TOOL_COMMAND" help > /dev/null

echo
echo "DevMemory local global tool installed successfully."
echo "Command available:"
echo "  $TOOL_COMMAND help"