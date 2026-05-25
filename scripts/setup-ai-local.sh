#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "Setting up DevMemory local AI runtime..."
echo

START_EXIT_CODE=0
PULL_EXIT_CODE=0
DOCTOR_EXIT_CODE=0

echo "Step 1/3 - Starting local AI services"
echo "-------------------------------------"

if "$ROOT_DIR/scripts/start-ai-local.sh"; then
  echo
  echo "Local AI services startup completed."
else
  START_EXIT_CODE=$?
  echo
  echo "Local AI services startup did not complete successfully."
  echo "This usually means Docker or a Docker-compatible CLI is not available."
fi

echo
echo "Step 2/3 - Pulling local Ollama models"
echo "--------------------------------------"

if "$ROOT_DIR/scripts/pull-ollama-models-local.sh"; then
  echo
  echo "Ollama model preparation completed."
else
  PULL_EXIT_CODE=$?
  echo
  echo "Ollama model preparation did not complete successfully."
  echo "This usually means Ollama is not installed or not available in PATH."
fi

echo
echo "Step 3/3 - Running AI doctor"
echo "----------------------------"

if "$ROOT_DIR/scripts/ai-doctor-local.sh"; then
  echo
  echo "DevMemory local AI runtime is ready."
else
  DOCTOR_EXIT_CODE=$?
  echo
  echo "DevMemory local AI runtime still requires attention."
fi

echo
echo "Setup summary"
echo "-------------"
echo "Services startup: $([ "$START_EXIT_CODE" -eq 0 ] && echo "ok" || echo "failed")"
echo "Model pull:       $([ "$PULL_EXIT_CODE" -eq 0 ] && echo "ok" || echo "failed")"
echo "AI doctor:        $([ "$DOCTOR_EXIT_CODE" -eq 0 ] && echo "ok" || echo "failed")"

if [ "$START_EXIT_CODE" -eq 0 ] && [ "$PULL_EXIT_CODE" -eq 0 ] && [ "$DOCTOR_EXIT_CODE" -eq 0 ]; then
  echo
  echo "Local AI setup completed successfully."

  exit 0
fi

echo
echo "Local AI setup completed with warnings or failures."
echo
echo "Next checks:"
echo "  - install Docker Desktop or a Docker-compatible CLI for Qdrant"
echo "  - install Ollama for local LLM and embeddings"
echo "  - rerun ./scripts/setup-ai-local.sh"
echo
echo "You can also inspect the current status with:"
echo "  ./scripts/ai-doctor-local.sh"

exit 1