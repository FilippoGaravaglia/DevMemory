#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

CHAT_PROVIDER="${DEVMEMORY_CHAT_PROVIDER:-ollama}"
EMBEDDING_PROVIDER="${DEVMEMORY_EMBEDDING_PROVIDER:-ollama}"
VECTOR_STORE="${DEVMEMORY_VECTOR_STORE:-qdrant}"

OLLAMA_ENDPOINT="${DEVMEMORY_OLLAMA_ENDPOINT:-http://localhost:11434}"
OLLAMA_CHAT_MODEL="${DEVMEMORY_OLLAMA_CHAT_MODEL:-llama3.2}"
OLLAMA_EMBEDDING_MODEL="${DEVMEMORY_OLLAMA_EMBEDDING_MODEL:-nomic-embed-text}"

QDRANT_ENDPOINT="${DEVMEMORY_QDRANT_ENDPOINT:-http://localhost:6333}"
QDRANT_COLLECTION="${DEVMEMORY_QDRANT_COLLECTION:-devmemory_memories}"

QUERY="${DEVMEMORY_AI_SMOKE_QUERY:-estimate revision cloning}"
QUESTION="${DEVMEMORY_AI_SMOKE_QUESTION:-How did we handle estimate revision cloning?}"

TEMP_DIR="$(mktemp -d)"
USES_TEMP_HOME="false"

if [ -n "${DEVMEMORY_AI_SMOKE_HOME:-}" ]; then
  DEVMEMORY_HOME_DIR="$DEVMEMORY_AI_SMOKE_HOME"
elif [ -n "${DEVMEMORY_HOME:-}" ]; then
  DEVMEMORY_HOME_DIR="$DEVMEMORY_HOME"
else
  DEVMEMORY_HOME_DIR="$TEMP_DIR/devmemory-home"
  USES_TEMP_HOME="true"
fi

cleanup() {
  rm -rf "$TEMP_DIR"
}

trap cleanup EXIT

export DEVMEMORY_HOME="$DEVMEMORY_HOME_DIR"
export DEVMEMORY_CHAT_PROVIDER="$CHAT_PROVIDER"
export DEVMEMORY_EMBEDDING_PROVIDER="$EMBEDDING_PROVIDER"
export DEVMEMORY_VECTOR_STORE="$VECTOR_STORE"
export DEVMEMORY_OLLAMA_ENDPOINT="$OLLAMA_ENDPOINT"
export DEVMEMORY_OLLAMA_CHAT_MODEL="$OLLAMA_CHAT_MODEL"
export DEVMEMORY_OLLAMA_EMBEDDING_MODEL="$OLLAMA_EMBEDDING_MODEL"
export DEVMEMORY_QDRANT_ENDPOINT="$QDRANT_ENDPOINT"
export DEVMEMORY_QDRANT_COLLECTION="$QDRANT_COLLECTION"

cd "$ROOT_DIR"

run_cli() {
  dotnet run --project "$ROOT_DIR/src/DevMemory.Cli" -- "$@"
}

print_step() {
  echo
  echo "$1"
  printf '%s\n' "$1" | sed 's/./-/g'
}

fail_no_memories() {
  echo
  echo "Local AI smoke test cannot continue because no memories are available for indexing."
  echo
  echo "This smoke test must validate the real flow:"
  echo "  memory -> embedding -> Qdrant index -> semantic search -> RAG answer"
  echo
  echo "Create at least one memory first, or point the smoke test to an existing DevMemory home."
  echo
  echo "Examples:"
  echo "  devmemory add"
  echo "  DEVMEMORY_AI_SMOKE_HOME=\"\$HOME/.devmemory\" ./scripts/dev-ai-local.sh smoke"
  echo "  DEVMEMORY_HOME=\"\$HOME/.devmemory\" ./scripts/dev-ai-local.sh smoke"

  if [ "$USES_TEMP_HOME" = "true" ]; then
    echo
    echo "Current run is using a temporary empty DEVMEMORY_HOME:"
    echo "  $DEVMEMORY_HOME"
  fi

  exit 1
}

extract_total_documents() {
  local output_file="$1"

  awk -F': ' '/^Total documents:/ { print $2; exit }' "$output_file"
}

echo "Running DevMemory local AI smoke test..."
echo
echo "DEVMEMORY_HOME:           $DEVMEMORY_HOME"
echo "Chat provider:            $DEVMEMORY_CHAT_PROVIDER"
echo "Embedding provider:       $DEVMEMORY_EMBEDDING_PROVIDER"
echo "Vector store:             $DEVMEMORY_VECTOR_STORE"
echo "Ollama endpoint:          $DEVMEMORY_OLLAMA_ENDPOINT"
echo "Ollama chat model:        $DEVMEMORY_OLLAMA_CHAT_MODEL"
echo "Ollama embedding model:   $DEVMEMORY_OLLAMA_EMBEDDING_MODEL"
echo "Qdrant endpoint:          $DEVMEMORY_QDRANT_ENDPOINT"
echo "Qdrant collection:        $DEVMEMORY_QDRANT_COLLECTION"
echo

print_step "Step 1/8 - Build CLI"
dotnet build "$ROOT_DIR/DevMemory.slnx"

print_step "Step 2/8 - Check AI runtime status"
run_cli ai-status

print_step "Step 3/8 - Diagnose local AI runtime"
run_cli ai-doctor

print_step "Step 4/8 - Inspect indexable memories"
DRY_RUN_OUTPUT_FILE="$TEMP_DIR/index-dry-run-output.txt"
run_cli index --dry-run --show-text --limit 1 | tee "$DRY_RUN_OUTPUT_FILE"

TOTAL_DOCUMENTS="$(extract_total_documents "$DRY_RUN_OUTPUT_FILE")"

if [ -z "$TOTAL_DOCUMENTS" ] || [ "$TOTAL_DOCUMENTS" -le 0 ]; then
  fail_no_memories
fi

print_step "Step 5/8 - Index first memory"
run_cli index --limit 1

print_step "Step 6/8 - Run semantic search"
run_cli semantic-search "$QUERY" --limit 3

print_step "Step 7/8 - Run RAG question"
run_cli ask --rag "$QUESTION" --limit 3

print_step "Step 8/8 - Run RAG question with context"
run_cli ask --rag --show-context "$QUESTION" --limit 3

echo
echo "DevMemory local AI smoke test completed successfully."