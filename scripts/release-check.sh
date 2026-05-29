#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "Running DevMemory release check..."

echo
echo "Step 1/4 - Running build and test validation"
echo "-------------------------------------------"
"$ROOT_DIR/scripts/build-test.sh"

echo
echo "Step 2/4 - Verifying version consistency"
echo "----------------------------------------"
"$ROOT_DIR/scripts/verify-version-consistency.sh"

echo
echo "Step 3/4 - Verifying package artifact"
echo "-------------------------------------"
"$ROOT_DIR/scripts/verify-package-artifact.sh"

echo
echo "Step 4/4 - Running CLI package smoke test"
echo "----------------------------------------"
"$ROOT_DIR/scripts/smoke-test-cli-package.sh"

echo
echo "DevMemory release check completed successfully."