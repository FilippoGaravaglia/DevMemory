#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "Verifying repository support files..."
echo

required_files=(
  "CONTRIBUTING.md"
  "SECURITY.md"
  ".github/dependabot.yml"
  ".github/workflows/ci.yml"
  ".github/ISSUE_TEMPLATE/bug_report.md"
  ".github/ISSUE_TEMPLATE/feature_request.md"
  ".github/pull_request_template.md"
)

missing_or_empty_files=()

for relative_path in "${required_files[@]}"; do
  full_path="$ROOT_DIR/$relative_path"

  if [ ! -f "$full_path" ]; then
    missing_or_empty_files+=("$relative_path")
    continue
  fi

  if [ ! -s "$full_path" ]; then
    missing_or_empty_files+=("$relative_path")
    continue
  fi

  echo "  OK $relative_path"
done

echo

if [ "${#missing_or_empty_files[@]}" -gt 0 ]; then
  echo "Repository support file verification failed."
  echo
  echo "Missing or empty files:"

  for relative_path in "${missing_or_empty_files[@]}"; do
    echo "  - $relative_path"
  done

  exit 1
fi

echo "Repository support file verification completed successfully."