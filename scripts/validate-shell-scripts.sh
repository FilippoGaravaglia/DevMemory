#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SCRIPTS_DIR="$ROOT_DIR/scripts"

echo "Validating shell scripts..."
echo

if [ ! -d "$SCRIPTS_DIR" ]; then
  echo "Scripts directory not found: $SCRIPTS_DIR"
  exit 1
fi

FAILED=0

while IFS= read -r script_file; do
  relative_path="${script_file#$ROOT_DIR/}"

  echo "Checking $relative_path"

  if [ ! -s "$script_file" ]; then
    echo "  ERROR: script is empty."
    FAILED=1
    continue
  fi

  first_line="$(head -n 1 "$script_file")"

  if [ "$first_line" != "#!/usr/bin/env bash" ]; then
    echo "  ERROR: missing or invalid bash shebang."
    echo "  Expected: #!/usr/bin/env bash"
    echo "  Actual:   $first_line"
    FAILED=1
  fi

  if [ ! -x "$script_file" ]; then
    echo "  ERROR: script is not executable."
    echo "  Fix: chmod +x $relative_path"
    FAILED=1
  fi

  if ! bash -n "$script_file"; then
    echo "  ERROR: bash syntax validation failed."
    FAILED=1
  fi

  echo "  OK"
  echo
done < <(find "$SCRIPTS_DIR" -maxdepth 1 -type f -name "*.sh" | sort)

if [ "$FAILED" -ne 0 ]; then
  echo "Shell script validation failed."
  exit 1
fi

echo "Shell script validation completed successfully."