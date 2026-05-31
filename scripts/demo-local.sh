#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DEMO_HOME="$(mktemp -d "${TMPDIR:-/tmp}/devmemory-demo.XXXXXX")"

cleanup() {
  if [[ "${DEVMEMORY_KEEP_DEMO_HOME:-false}" != "true" ]]; then
    rm -rf "$DEMO_HOME"
  fi
}

trap cleanup EXIT

run_devmemory() {
  dotnet run --project "$ROOT_DIR/src/DevMemory.Cli" -- "$@"
}

print_section() {
  echo
  echo "============================================================"
  echo "$1"
  echo "============================================================"
  echo
}

export DEVMEMORY_HOME="$DEMO_HOME"

print_section "DevMemory local isolated demo"

echo "Demo home:"
echo "  $DEVMEMORY_HOME"
echo

echo "This demo uses an isolated temporary DEVMEMORY_HOME."
echo "Your real ~/.devmemory data will not be touched."
echo

mkdir -p "$DEVMEMORY_HOME/markdown"
mkdir -p "$DEVMEMORY_HOME/graph"

cat > "$DEVMEMORY_HOME/devmemory.json" <<'JSON'
[
  {
    "Id": "7340ac82-4ed6-41b1-b790-e15edfaf39b4",
    "Title": "Local AI runtime validation",
    "Project": "DevMemory",
    "Area": "AI",
    "Branch": "main",
    "Tags": [
      "dotnet",
      "ollama",
      "qdrant",
      "rag"
    ],
    "Problem": "Validate the local AI runtime end-to-end for DevMemory.",
    "Solution": "Configured Ollama for chat and embeddings, started Qdrant, indexed local memories and validated semantic search plus RAG.",
    "Decisions": [
      "Keep JSON as the primary local source of truth.",
      "Use Qdrant as a derived vector index.",
      "Keep Ollama as the default local AI runtime."
    ],
    "FilesTouched": [
      "src/DevMemory.Cli/Commands/Ai/IndexCommandHandler.cs",
      "src/DevMemory.Cli/Commands/Ai/SemanticSearchCommandHandler.cs",
      "src/DevMemory.Cli/Commands/Ai/AskCommandHandler.cs"
    ],
    "Tests": [
      "devmemory index --limit 1",
      "devmemory semantic-search \"local ai runtime\"",
      "devmemory ask --rag \"How did we validate the local AI runtime?\""
    ],
    "LessonsLearned": "Local RAG works well when storage, embeddings and vector indexing are treated as separate layers.",
    "CreatedAt": "2026-05-30T10:00:00Z"
  },
  {
    "Id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "Title": "Persistent AI configuration",
    "Project": "DevMemory",
    "Area": "Configuration",
    "Branch": "main",
    "Tags": [
      "dotnet",
      "configuration",
      "ollama",
      "qdrant"
    ],
    "Problem": "Running AI commands required repeating environment variables every time.",
    "Solution": "Added persistent configuration through devmemory config show/set/reset with precedence over defaults and below environment variables.",
    "Decisions": [
      "Use ~/.devmemory/config.json for persistent local configuration.",
      "Keep environment variables as the highest-precedence override.",
      "Expose configuration status through ai-status."
    ],
    "FilesTouched": [
      "src/DevMemory.Infrastructure/AiRuntimeConfigurationStore.cs",
      "src/DevMemory.Cli/Commands/System/ConfigCommandHandler.cs",
      "src/DevMemory.Cli/Commands/Ai/AiStatusCommandHandler.cs"
    ],
    "Tests": [
      "devmemory config set chat-provider ollama",
      "devmemory config show",
      "devmemory ai-status"
    ],
    "LessonsLearned": "Persistent configuration makes the CLI much easier to use daily.",
    "CreatedAt": "2026-05-30T11:30:00Z"
  },
  {
    "Id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "Title": "Memory lifecycle with edit and delete",
    "Project": "DevMemory",
    "Area": "Memory",
    "Branch": "main",
    "Tags": [
      "dotnet",
      "cli",
      "lifecycle"
    ],
    "Problem": "Memories could be added and searched, but the lifecycle was incomplete.",
    "Solution": "Added edit and delete commands. Delete removes the primary JSON memory, Markdown export and Qdrant vector point when configured.",
    "Decisions": [
      "Keep JSON as the primary source of truth.",
      "Treat Markdown and Qdrant as derived artifacts.",
      "Make Qdrant cleanup best-effort so primary deletion is not blocked."
    ],
    "FilesTouched": [
      "src/DevMemory.Cli/Commands/Memory/EditCommandHandler.cs",
      "src/DevMemory.Cli/Commands/Memory/DeleteCommandHandler.cs",
      "src/DevMemory.Application/MemoryService.cs"
    ],
    "Tests": [
      "MemoryServiceTests",
      "EditCommandHandlerTests",
      "DeleteCommandHandlerTests"
    ],
    "LessonsLearned": "A useful CLI needs a complete lifecycle: add, show, search, edit and delete.",
    "CreatedAt": "2026-05-30T14:45:00Z"
  },
  {
    "Id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
    "Title": "Related memories and timeline",
    "Project": "DevMemory",
    "Area": "Discovery",
    "Branch": "main",
    "Tags": [
      "semantic-search",
      "timeline",
      "portfolio"
    ],
    "Problem": "A developer memory tool should help reconnect related work and show project evolution.",
    "Solution": "Added related memories based on semantic search and a timeline command for chronological project exploration.",
    "Decisions": [
      "Reuse the semantic search pipeline for related memories.",
      "Keep timeline independent from AI so it works without Ollama or Qdrant.",
      "Make both commands easy to show in demos and screenshots."
    ],
    "FilesTouched": [
      "src/DevMemory.Cli/Commands/Ai/RelatedCommandHandler.cs",
      "src/DevMemory.Cli/Commands/Memory/TimelineCommandHandler.cs"
    ],
    "Tests": [
      "RelatedCommandHandlerTests",
      "TimelineCommandHandlerTests"
    ],
    "LessonsLearned": "Discovery features make saved memories much more valuable than plain notes.",
    "CreatedAt": "2026-05-30T16:00:00Z"
  }
]
JSON

print_section "Doctor"

run_devmemory doctor || true

print_section "List memories"

run_devmemory list

print_section "Search memories"

run_devmemory search "qdrant"

print_section "Show one memory"

run_devmemory show "7340ac82-4ed6-41b1-b790-e15edfaf39b4"

print_section "Timeline"

run_devmemory timeline --project DevMemory --limit 10

print_section "Edit memory"

run_devmemory edit "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" --add-tag demo --solution "Updated during the isolated local demo."

print_section "Show edited memory"

run_devmemory show "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"

print_section "Graph export"

run_devmemory graph-export

print_section "Graph view"

run_devmemory graph-view

print_section "Optional local AI demo"

echo "The next commands require Ollama and Qdrant to be running."
echo "If they are not running, this demo will skip the AI section gracefully."
echo

set +e

DEVMEMORY_CHAT_PROVIDER=ollama \
DEVMEMORY_EMBEDDING_PROVIDER=ollama \
DEVMEMORY_VECTOR_STORE=qdrant \
run_devmemory index --limit 4

INDEX_EXIT_CODE=$?

if [[ "$INDEX_EXIT_CODE" -eq 0 ]]; then
  DEVMEMORY_CHAT_PROVIDER=ollama \
  DEVMEMORY_EMBEDDING_PROVIDER=ollama \
  DEVMEMORY_VECTOR_STORE=qdrant \
  run_devmemory semantic-search "local AI runtime qdrant" --limit 3

  DEVMEMORY_CHAT_PROVIDER=ollama \
  DEVMEMORY_EMBEDDING_PROVIDER=ollama \
  DEVMEMORY_VECTOR_STORE=qdrant \
  run_devmemory related "7340ac82-4ed6-41b1-b790-e15edfaf39b4" --limit 3

  DEVMEMORY_CHAT_PROVIDER=ollama \
  DEVMEMORY_EMBEDDING_PROVIDER=ollama \
  DEVMEMORY_VECTOR_STORE=qdrant \
  run_devmemory ask --rag "How did we validate the local AI runtime?" --limit 3
else
  echo
  echo "AI demo skipped because indexing failed."
  echo "Start the local AI runtime with:"
  echo "  ./scripts/start-ai-local.sh"
  echo
fi

set -e

print_section "Demo completed"

echo "Temporary demo home:"
echo "  $DEMO_HOME"
echo

if [[ "${DEVMEMORY_KEEP_DEMO_HOME:-false}" == "true" ]]; then
  echo "Demo data was kept because DEVMEMORY_KEEP_DEMO_HOME=true."
else
  echo "Demo data will be deleted automatically."
fi