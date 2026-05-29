#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_PROJECT="$ROOT_DIR/src/DevMemory.Cli/DevMemory.Cli.csproj"
CHANGELOG_FILE="$ROOT_DIR/CHANGELOG.md"

print_step() {
  echo
  echo "$1"
  printf '%s\n' "$1" | sed 's/./-/g'
}

extract_project_version() {
  local project_file="$1"

  local version
  version="$(grep -E "<Version>.*</Version>" "$project_file" | sed -E 's/.*<Version>(.*)<\/Version>.*/\1/' | head -n 1)"

  if [ -z "$version" ]; then
    echo "Unable to read <Version> from:"
    echo "  $project_file"
    exit 1
  fi

  echo "$version"
}

extract_changelog_section() {
  local changelog_file="$1"
  local project_version="$2"

  awk -v raw_version="$project_version" '
    BEGIN {
      escaped_version = raw_version
      gsub(/\./, "\\.", escaped_version)
      in_section = 0
    }

    $0 ~ "^##[[:space:]]+\\[?" escaped_version "\\]?" {
      in_section = 1
      next
    }

    in_section && $0 ~ "^##[[:space:]]+" {
      exit
    }

    in_section {
      print
    }
  ' "$changelog_file"
}

echo "Verifying DevMemory changelog..."

if [ ! -f "$CLI_PROJECT" ]; then
  echo "CLI project file not found:"
  echo "  $CLI_PROJECT"
  exit 1
fi

if [ ! -f "$CHANGELOG_FILE" ]; then
  echo "CHANGELOG.md was not found:"
  echo "  $CHANGELOG_FILE"
  exit 1
fi

print_step "Step 1/3 - Read project version"
PROJECT_VERSION="$(extract_project_version "$CLI_PROJECT")"
echo "Project version: $PROJECT_VERSION"

print_step "Step 2/3 - Find changelog entry"

CHANGELOG_SECTION="$(extract_changelog_section "$CHANGELOG_FILE" "$PROJECT_VERSION")"

if [ -z "$CHANGELOG_SECTION" ]; then
  echo "CHANGELOG.md does not contain an entry for version $PROJECT_VERSION."
  echo
  echo "Expected one of:"
  echo "  ## [$PROJECT_VERSION] - YYYY-MM-DD"
  echo "  ## $PROJECT_VERSION - YYYY-MM-DD"
  exit 1
fi

echo "Changelog entry found for version $PROJECT_VERSION."

print_step "Step 3/3 - Validate changelog entry content"

MEANINGFUL_CONTENT="$(
  printf '%s\n' "$CHANGELOG_SECTION" |
    grep -Ev '^[[:space:]]*$|^###[[:space:]]+' || true
)"

if [ -z "$MEANINGFUL_CONTENT" ]; then
  echo "CHANGELOG.md entry for version $PROJECT_VERSION exists but has no meaningful content."
  exit 1
fi

echo "Changelog entry contains release notes."

echo
echo "DevMemory changelog verification completed successfully."