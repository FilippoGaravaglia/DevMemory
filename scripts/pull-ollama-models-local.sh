#!/usr/bin/env bash
set -euo pipefail

CHAT_MODEL="${DEVMEMORY_OLLAMA_CHAT_MODEL:-llama3.2}"
EMBEDDING_MODEL="${DEVMEMORY_OLLAMA_EMBEDDING_MODEL:-nomic-embed-text}"

echo "Preparing DevMemory local Ollama models..."
echo

if ! command -v ollama >/dev/null 2>&1; then
  echo "Ollama was not found."
  echo
  echo "Install Ollama before pulling local AI models."
  echo
  echo "Expected models:"
  echo "  Chat model:      $CHAT_MODEL"
  echo "  Embedding model: $EMBEDDING_MODEL"
  echo
  echo "After installing Ollama, run:"
  echo "  ./scripts/pull-ollama-models-local.sh"
  echo
  echo "Then verify the AI runtime with:"
  echo "  ./scripts/ai-doctor-local.sh"

  exit 1
fi

pull_model() {
  local model="$1"
  local label="$2"

  if [ -z "$model" ]; then
    echo "$label model is empty. Skipping."
    return 0
  fi

  echo "Pulling $label model: $model"
  ollama pull "$model"
  echo
}

pull_model "$CHAT_MODEL" "chat"

if [ "$EMBEDDING_MODEL" != "$CHAT_MODEL" ]; then
  pull_model "$EMBEDDING_MODEL" "embedding"
else
  echo "Embedding model is the same as chat model. Skipping duplicate pull."
  echo
fi

echo "Ollama models requested successfully."
echo
echo "Run:"
echo "  ./scripts/ai-doctor-local.sh"
echo
echo "to verify model availability."