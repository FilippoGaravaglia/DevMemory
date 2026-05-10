#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-}"

if [ -z "$VERSION" ]; then
  echo "Usage: ./scripts/pack-release.sh <version>"
  echo "Example: ./scripts/pack-release.sh 0.1.0"
  exit 1
fi

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT_DIR"

echo "Packing DevMemory.Cli version $VERSION..."

rm -rf artifacts/packages
mkdir -p artifacts/packages

dotnet build DevMemory.slnx -c Release
dotnet test DevMemory.slnx -c Release --no-build
dotnet pack src/DevMemory.Cli/DevMemory.Cli.csproj \
  -c Release \
  --no-build \
  -o artifacts/packages \
  /p:Version="$VERSION" \
  /p:PackageVersion="$VERSION" \
  /p:InformationalVersion="$VERSION"

echo
echo "Package created:"
ls -lh artifacts/packages/*.nupkg