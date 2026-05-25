#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
REQUIRE_CLEAN_WORKTREE=false

print_help() {
  cat <<EOF
DevMemory release check

Usage:
  ./scripts/release-check.sh [options]

Options:
  --require-clean   Fail if the Git working tree has uncommitted changes
  --help, -h        Show this help message

Checks:
  1. Validate shell scripts
  2. Verify code formatting
  3. Build solution
  4. Run tests
  5. Pack CLI
  6. Smoke test the generated CLI package

Examples:
  ./scripts/release-check.sh
  ./scripts/release-check.sh --require-clean
EOF
}

while [ "$#" -gt 0 ]; do
  case "$1" in
    --require-clean)
      REQUIRE_CLEAN_WORKTREE=true
      shift
      ;;

    --help|-h)
      print_help
      exit 0
      ;;

    *)
      echo "Unknown option: $1"
      echo
      print_help
      exit 1
      ;;
  esac
done

cd "$ROOT_DIR"

echo "Running DevMemory release check..."
echo

if [ "$REQUIRE_CLEAN_WORKTREE" = true ]; then
  echo "Checking Git working tree..."
  echo

  if ! git diff --quiet || ! git diff --cached --quiet; then
    echo "Git working tree is not clean."
    echo
    echo "Commit or stash your changes before running release check with --require-clean."

    exit 1
  fi

  echo "Git working tree is clean."
  echo
fi

echo "Step 1/2 - Running build and test validation"
echo "-------------------------------------------"
"$ROOT_DIR/scripts/build-test.sh"

echo
echo "Step 2/2 - Running CLI package smoke test"
echo "----------------------------------------"
BUILD_CONFIGURATION=Release "$ROOT_DIR/scripts/smoke-test-cli-package.sh"

echo
echo "DevMemory release check completed successfully."