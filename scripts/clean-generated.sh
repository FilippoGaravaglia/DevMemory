#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "Removing generated build folders..."
find . -type d \( -name "bin" -o -name "obj" \) -prune -exec rm -rf {} +

echo "Removing local artifacts..."
rm -rf artifacts

echo "Removing local DevMemory generated files from repository root..."
rm -f devmemory.json
rm -f devmemory.json.bak
rm -f devmemory.json.tmp
rm -f memory.json

echo
echo "Generated files cleaned successfully."