#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "Verifying documentation links..."
echo

markdown_files=(
  "README.md"
  "CHANGELOG.md"
  "CONTRIBUTING.md"
  "SECURITY.md"
  "docs/demo.md"
  "docs/release-process.md"
)

missing_targets=()

for markdown_file in "${markdown_files[@]}"; do
  full_markdown_path="$ROOT_DIR/$markdown_file"

  if [ ! -f "$full_markdown_path" ]; then
    echo "  SKIP $markdown_file (file not found)"
    continue
  fi

  echo "Checking $markdown_file"

  while IFS= read -r target; do
    # Ignore empty targets.
    if [ -z "$target" ]; then
      continue
    fi

    # Ignore anchors.
    if [[ "$target" == \#* ]]; then
      continue
    fi

    # Ignore absolute URLs.
    if [[ "$target" == http://* || "$target" == https://* ]]; then
      continue
    fi

    # Ignore mail links.
    if [[ "$target" == mailto:* ]]; then
      continue
    fi

    # Ignore badges or dynamic shields that may use external URLs.
    if [[ "$target" == *"img.shields.io"* ]]; then
      continue
    fi

    # Strip anchor from local path, for example docs/demo.md#section.
    target_without_anchor="${target%%#*}"

    # Strip query string if present.
    target_without_query="${target_without_anchor%%\?*}"

    # Ignore empty target after stripping.
    if [ -z "$target_without_query" ]; then
      continue
    fi

    markdown_dir="$(dirname "$markdown_file")"

    if [ "$markdown_dir" = "." ]; then
      resolved_target="$ROOT_DIR/$target_without_query"
    else
      resolved_target="$ROOT_DIR/$markdown_dir/$target_without_query"
    fi

    if [ ! -e "$resolved_target" ]; then
      missing_targets+=("$markdown_file -> $target")
      echo "  MISSING $target"
    else
      echo "  OK $target"
    fi
  done < <(
    grep -Eo '!\[[^]]*\]\([^)]*\)|\[[^]]+\]\([^)]*\)' "$full_markdown_path" \
      | sed -E 's/^!?\[[^]]*\]\(([^)]*)\)$/\1/' \
      || true
  )

  echo
done

if [ "${#missing_targets[@]}" -gt 0 ]; then
  echo "Documentation link verification failed."
  echo
  echo "Missing local targets:"

  for missing_target in "${missing_targets[@]}"; do
    echo "  - $missing_target"
  done

  exit 1
fi

echo "Documentation link verification completed successfully."