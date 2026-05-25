#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMPOSE_FILE="$ROOT_DIR/docker-compose.ai.yml"

find_compose_command() {
  if command -v docker >/dev/null 2>&1; then
    echo "docker compose"
    return 0
  fi

  if command -v docker-compose >/dev/null 2>&1; then
    echo "docker-compose"
    return 0
  fi

  return 1
}

echo "Stopping DevMemory local AI runtime..."
echo

if ! COMPOSE_COMMAND="$(find_compose_command)"; then
  echo "Docker was not found."
  echo
  echo "Nothing was stopped because no Docker-compatible command is available."

  exit 1
fi

if [ ! -f "$COMPOSE_FILE" ]; then
  echo "Compose file not found: $COMPOSE_FILE"
  exit 1
fi

echo "Using compose command: $COMPOSE_COMMAND"
echo "Using compose file: $COMPOSE_FILE"
echo

# shellcheck disable=SC2086
$COMPOSE_COMMAND -f "$COMPOSE_FILE" down

echo
echo "DevMemory local AI runtime stopped."