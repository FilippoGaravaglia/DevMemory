#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "Running DevMemory release check..."

echo
echo "Step 1/7 - Running build and test validation"
echo "-------------------------------------------"
"$ROOT_DIR/scripts/build-test.sh"

echo
echo "Step 2/7 - Verifying repository hygiene"
echo "--------------------------------------"
"$ROOT_DIR/scripts/verify-repository-hygiene.sh"

echo
echo "Step 3/7 - Verifying changelog"
echo "------------------------------"
"$ROOT_DIR/scripts/verify-changelog.sh"

echo
echo "Step 4/7 - Verifying version consistency"
echo "----------------------------------------"
"$ROOT_DIR/scripts/verify-version-consistency.sh"

echo
echo "Step 5/7 - Verifying package artifact"
echo "-------------------------------------"
"$ROOT_DIR/scripts/verify-package-artifact.sh"

echo
echo "Step 6/7 - Running CLI package smoke test"
echo "----------------------------------------"
"$ROOT_DIR/scripts/smoke-test-cli-package.sh"

echo
echo "Step 7/7 - Generating final package checksum"
echo "--------------------------------------------"
"$ROOT_DIR/scripts/generate-package-checksum.sh"

echo
echo "DevMemory release check completed successfully."