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

echo "Starting DevMemory local AI runtime..."
echo

if ! COMPOSE_COMMAND="$(find_compose_command)"; then
  echo "Docker was not found."
  echo
  echo "Install Docker Desktop, Podman with Docker-compatible CLI, or docker-compose before starting the local AI runtime."
  echo
  echo "You can still run:"
  echo "  ./scripts/ai-doctor-local.sh"
  echo
  echo "but Qdrant will remain unreachable until a container runtime is available."

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
$COMPOSE_COMMAND -f "$COMPOSE_FILE" up -d

echo
echo "Local AI services requested."
echo
echo "Running AI doctor..."
echo

"$ROOT_DIR/scripts/ai-doctor-local.sh"