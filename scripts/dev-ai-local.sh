#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

print_help() {
  cat <<EOF
DevMemory local AI helper

Usage:
  ./scripts/dev-ai-local.sh <command>

Commands:
  setup        Run full local AI setup: start services, pull models, run doctor
  start        Start local AI services
  stop         Stop local AI services
  doctor       Run local AI doctor
  pull-models  Pull configured local Ollama models
  help         Show this help message

Examples:
  ./scripts/dev-ai-local.sh setup
  ./scripts/dev-ai-local.sh doctor
  ./scripts/dev-ai-local.sh start
  ./scripts/dev-ai-local.sh pull-models
  ./scripts/dev-ai-local.sh stop
EOF
}

require_script() {
  local script_path="$1"

  if [ ! -f "$script_path" ]; then
    echo "Required script not found: $script_path"
    exit 1
  fi

  if [ ! -x "$script_path" ]; then
    echo "Required script is not executable: $script_path"
    echo "Run:"
    echo "  chmod +x $script_path"
    exit 1
  fi
}

run_script() {
  local script_name="$1"
  local script_path="$ROOT_DIR/scripts/$script_name"

  require_script "$script_path"

  "$script_path"
}

COMMAND="${1:-help}"

case "$COMMAND" in
  setup)
    run_script "setup-ai-local.sh"
    ;;

  start)
    run_script "start-ai-local.sh"
    ;;

  stop)
    run_script "stop-ai-local.sh"
    ;;

  doctor)
    run_script "ai-doctor-local.sh"
    ;;

  pull-models)
    run_script "pull-ollama-models-local.sh"
    ;;

  help|--help|-h)
    print_help
    ;;

  *)
    echo "Unknown command: $COMMAND"
    echo
    print_help
    exit 1
    ;;
esac