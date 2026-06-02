#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "Running DevMemory release check..."

echo
echo "Step 1/9 - Running build and test validation"
echo "-------------------------------------------"
"$ROOT_DIR/scripts/build-test.sh"

echo
echo "Step 2/9 - Verifying repository hygiene"
echo "--------------------------------------"
"$ROOT_DIR/scripts/verify-repository-hygiene.sh"

echo
echo "Step 3/9 - Verifying repository support files"
echo "------------------------------------------------"
"$ROOT_DIR/scripts/verify-repository-support-files.sh"

echo
echo "Step 4/9 - Verifying documentation links"
echo "----------------------------------------"
"$ROOT_DIR/scripts/verify-documentation-links.sh"

echo
echo "Step 5/9 - Verifying changelog"
echo "------------------------------"
"$ROOT_DIR/scripts/verify-changelog.sh"

echo
echo "Step 6/9 - Verifying version consistency"
echo "----------------------------------------"
"$ROOT_DIR/scripts/verify-version-consistency.sh"

echo
echo "Step 7/9 - Verifying package artifact"
echo "-------------------------------------"
"$ROOT_DIR/scripts/verify-package-artifact.sh"

echo
echo "Step 8/9 - Running CLI package smoke test"
echo "----------------------------------------"
"$ROOT_DIR/scripts/smoke-test-cli-package.sh"

echo
echo "Step 9/9 - Generating final package checksum"
echo "--------------------------------------------"
"$ROOT_DIR/scripts/generate-package-checksum.sh"

echo
echo "DevMemory release check completed successfully."