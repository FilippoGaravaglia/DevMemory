#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

print_step() {
  echo
  echo "$1"
  printf '%s\n' "$1" | sed 's/./-/g'
}

fail_with_tracked_files() {
  local title="$1"
  local files="$2"

  echo "$title"
  echo
  printf '%s\n' "$files"
  echo
  echo "These files are generated artifacts and should not be tracked."
  echo "Remove them from Git tracking with:"
  echo "  git rm --cached <file>"
  echo
  echo "Then make sure .gitignore contains the correct ignore rules."

  exit 1
}

cd "$ROOT_DIR"

echo "Verifying repository hygiene..."

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  echo "This directory is not inside a Git repository."
  exit 1
fi

print_step "Step 1/3 - Check tracked build outputs"

TRACKED_BUILD_OUTPUTS="$(
  git ls-files | grep -E '(^|/)(bin|Bin|obj|Obj)/' || true
)"

if [ -n "$TRACKED_BUILD_OUTPUTS" ]; then
  fail_with_tracked_files "Tracked build outputs found:" "$TRACKED_BUILD_OUTPUTS"
fi

echo "No tracked build outputs found."

print_step "Step 2/3 - Check tracked release artifacts"

TRACKED_RELEASE_ARTIFACTS="$(
  git ls-files | grep -E '(^artifacts/|\.nupkg$|\.snupkg$|\.nupkg\.sha256$)' || true
)"

if [ -n "$TRACKED_RELEASE_ARTIFACTS" ]; then
  fail_with_tracked_files "Tracked release artifacts found:" "$TRACKED_RELEASE_ARTIFACTS"
fi

echo "No tracked release artifacts found."

print_step "Step 3/3 - Check tracked local runtime data"

TRACKED_LOCAL_RUNTIME_DATA="$(
  git ls-files | grep -E '(^\.devmemory/|^devmemory-work/|^\.qdrant/|^qdrant_storage/|^ollama/|^\.env\.local$)' || true
)"

if [ -n "$TRACKED_LOCAL_RUNTIME_DATA" ]; then
  fail_with_tracked_files "Tracked local runtime data found:" "$TRACKED_LOCAL_RUNTIME_DATA"
fi

echo "No tracked local runtime data found."

echo
echo "Repository hygiene verification completed successfully."