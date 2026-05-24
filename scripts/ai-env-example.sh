#!/usr/bin/env bash

# DevMemory local AI runtime configuration.
#
# Usage:
#   source scripts/ai-env-example.sh
#
# Then run:
#   dotnet run --project src/DevMemory.Cli -- ai-doctor

export DEVMEMORY_CHAT_PROVIDER=ollama
export DEVMEMORY_EMBEDDING_PROVIDER=ollama
export DEVMEMORY_VECTOR_STORE=qdrant

export DEVMEMORY_OLLAMA_ENDPOINT=http://localhost:11434
export DEVMEMORY_OLLAMA_CHAT_MODEL=llama3.2
export DEVMEMORY_OLLAMA_EMBEDDING_MODEL=nomic-embed-text

export DEVMEMORY_QDRANT_ENDPOINT=http://localhost:6333
export DEVMEMORY_QDRANT_COLLECTION=devmemory_memories