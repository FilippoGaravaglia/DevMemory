#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "Verifying code formatting..."
dotnet format DevMemory.slnx --verify-no-changes

echo
echo "Building DevMemory..."
dotnet build DevMemory.slnx

echo
echo "Running tests..."
dotnet test DevMemory.slnx --no-build

echo
echo "Build and tests completed successfully."