#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_PROJECT="$ROOT_DIR/src/DevMemory.Cli/DevMemory.Cli.csproj"

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

extract_cli_version() {
  local output="$1"

  local version
  version="$(printf '%s\n' "$output" | sed -E 's/^DevMemory[[:space:]]+([0-9]+\.[0-9]+\.[0-9]+.*)$/\1/' | head -n 1)"

  if [ -z "$version" ]; then
    echo "Unable to read CLI version from output:"
    echo "$output"
    exit 1
  fi

  echo "$version"
}

echo "Verifying DevMemory version consistency..."

if [ ! -f "$CLI_PROJECT" ]; then
  echo "CLI project file not found:"
  echo "  $CLI_PROJECT"
  exit 1
fi

print_step "Step 1/4 - Read project version"
PROJECT_VERSION="$(extract_project_version "$CLI_PROJECT")"
echo "Project version: $PROJECT_VERSION"

print_step "Step 2/4 - Read CLI version command"
VERSION_OUTPUT="$(dotnet run --project "$CLI_PROJECT" -- version)"
VERSION_COMMAND_VALUE="$(extract_cli_version "$VERSION_OUTPUT")"
echo "devmemory version: $VERSION_COMMAND_VALUE"

print_step "Step 3/4 - Read CLI --version alias"
VERSION_ALIAS_OUTPUT="$(dotnet run --project "$CLI_PROJECT" -- --version)"
VERSION_ALIAS_VALUE="$(extract_cli_version "$VERSION_ALIAS_OUTPUT")"
echo "devmemory --version: $VERSION_ALIAS_VALUE"

print_step "Step 4/4 - Compare versions"

if [ "$PROJECT_VERSION" != "$VERSION_COMMAND_VALUE" ]; then
  echo "Version mismatch detected."
  echo "Project version:        $PROJECT_VERSION"
  echo "CLI version command:    $VERSION_COMMAND_VALUE"
  exit 1
fi

if [ "$PROJECT_VERSION" != "$VERSION_ALIAS_VALUE" ]; then
  echo "Version mismatch detected."
  echo "Project version:        $PROJECT_VERSION"
  echo "CLI --version command:  $VERSION_ALIAS_VALUE"
  exit 1
fi

echo "Version consistency verified successfully."