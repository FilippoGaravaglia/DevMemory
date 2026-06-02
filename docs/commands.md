# DevMemory CLI command reference

This document provides a practical reference for the DevMemory command-line interface.

DevMemory can be used either through `dotnet run` during development or through the installed `devmemory` tool.

Recommended installed usage:

```bash
devmemory <command> [options]
```

Development usage:

```bash
dotnet run --project src/DevMemory.Cli -- <command> [options]
```

---

## Global commands

### Show help

```bash
devmemory help
devmemory help setup
devmemory help config
devmemory --help
devmemory -h
```

Shows the main CLI help output.

`devmemory help setup` shows command-specific help for the first-run setup command.

`devmemory help config` shows command-specific help for persistent local configuration.

---

### Show version

```bash
devmemory version
devmemory --version
devmemory -v
```

Shows the current DevMemory version.

---

## First-run setup

### General setup guidance

```bash
devmemory setup
```

Prints safe first-run setup guidance.

This command does not modify local data.

---

### Interactive setup wizard

```bash
devmemory setup --wizard
```

Runs a safe interactive first-run setup wizard.

The wizard does not modify local data, does not write configuration and does not start external services.

It guides the user through the recommended first-run commands and can optionally print the local AI/RAG setup flow.

Use it when installing DevMemory for the first time and you want a guided onboarding experience.

---

### Recommended next steps

```bash
devmemory setup --next
```

Prints the recommended next actions for a new user.

Typical guidance includes:

```text
devmemory doctor
devmemory storage
devmemory add
devmemory list
devmemory show <memory-id>
devmemory search "your topic"
devmemory timeline
devmemory graph-export
devmemory graph-view
```

---

### First-run checklist

```bash
devmemory setup --checklist
```

Prints a checklist for validating a new local DevMemory setup.

Useful when installing DevMemory for the first time or when onboarding a new environment.

---

### Local AI setup guidance

```bash
devmemory setup --local-ai
```

Prints local Ollama and Qdrant setup guidance for semantic search and RAG.

This command does not start external services and does not modify local data.

---

### Demo guidance

```bash
devmemory setup --demo
```

Prints instructions for running the isolated local demo.

---

### Setup validation guidance

```bash
devmemory setup --check
```

Prints commands that can be used to validate the local setup.

---

## Local storage

### Show storage path

```bash
devmemory storage
```

Shows the current local JSON storage path.

Default path:

```text
~/.devmemory/devmemory.json
```

If `DEVMEMORY_HOME` is set, DevMemory uses that directory instead.

---

### Show Markdown export directory

```bash
devmemory markdown
```

Shows the directory where Markdown exports are written.

Default path:

```text
~/.devmemory/markdown/
```

---

## Memory lifecycle

### Add a memory

```bash
devmemory add
```

Starts the interactive flow for creating a structured memory.

A memory captures:

```text
Title
Project
Area
Branch
Tags
Problem
Solution
Decisions
Files touched
Tests
Lessons learned
```

---

### List memories

```bash
devmemory list
```

Lists saved memories from local JSON storage.

---

### Show a memory

```bash
devmemory show <memory-id>
```

Shows the full details of a specific memory.

Example:

```bash
devmemory show 01HXYZ...
```

---

### Search memories

```bash
devmemory search <query>
```

Searches local memories using classic text search.

Examples:

```bash
devmemory search "revision"
devmemory search "mongodb mapping"
```

With filters:

```bash
devmemory search "revision" --project LogicalCommon
devmemory search "revision" --area Estimate
devmemory search "revision" --tag mongodb
devmemory search "revision" --project LogicalCommon --area Estimate --tag mongodb
```

Classic search does not require AI, Ollama or Qdrant.

---

### Edit a memory

```bash
devmemory edit <memory-id> [options]
```

Examples:

```bash
devmemory edit <memory-id> --title "Updated title"
devmemory edit <memory-id> --solution "Updated implementation notes"
devmemory edit <memory-id> --add-tag rag
devmemory edit <memory-id> --remove-tag test
devmemory edit <memory-id> --add-file src/Example.cs
devmemory edit <memory-id> --add-test ExampleTests
```

Editing updates the primary JSON storage and regenerates derived Markdown exports.

If the memory was previously indexed into Qdrant, rebuild the vector index to keep semantic search aligned.

---

### Delete a memory

```bash
devmemory delete <memory-id>
devmemory delete <memory-id> --yes
```

Deletes a memory from local JSON storage.

When possible, derived artifacts are also cleaned up:

```text
Markdown export
Qdrant vector point
```

---

### Timeline

```bash
devmemory timeline
```

Shows saved memories chronologically.

With filters:

```bash
devmemory timeline --project DevMemory
devmemory timeline --area AI
devmemory timeline --tag rag
devmemory timeline --limit 10
devmemory timeline --project DevMemory --limit 10
```

---

## Git integration

### Inspect Git repository

```bash
devmemory git-status
```

Inspects the current Git repository.

For a specific repository path:

```bash
devmemory git-status --path ~/work/my-repository
```

The command shows:

```text
repository path
current branch
last commit hash
last commit message
changed files
```

Generated files are filtered automatically.

---

### Create a memory draft from Git

```bash
devmemory learn-from-git
```

Creates a memory draft from the current Git context.

For a specific path:

```bash
devmemory learn-from-git --path ~/work/my-repository
```

The draft should then be completed manually with the real engineering context:

```text
problem
solution
decisions
tests
lessons learned
```

---

## Knowledge graph

### Export graph JSON

```bash
devmemory graph-export
```

Default output:

```text
~/.devmemory/graph/devmemory-graph.json
```

Custom output:

```bash
devmemory graph-export --output ./devmemory-graph.json
```

---

### Generate HTML graph view

```bash
devmemory graph-view
```

Default output:

```text
~/.devmemory/graph/devmemory-graph.html
```

Custom output:

```bash
devmemory graph-view --output ./devmemory-graph.html
```

Open the generated file in a browser to inspect memory relationships.

---

## Diagnostics

### General diagnostics

```bash
devmemory doctor
```

Checks the local DevMemory environment:

```text
storage readability
Markdown directory
persistent configuration
AI/RAG runtime configuration
Git availability
memory count
```

---

### AI runtime diagnostics

```bash
devmemory ai-status
devmemory ai-doctor
```

`ai-status` prints the current AI/RAG configuration.

`ai-doctor` diagnoses local AI runtime readiness.

---

## Persistent configuration

### Show configuration

```bash
devmemory config show
```

Shows persistent local DevMemory configuration.

---

### Set configuration

```bash
devmemory config set <key> <value>
```

Examples:

```bash
devmemory config set chat-provider ollama
devmemory config set embedding-provider ollama
devmemory config set vector-store qdrant
devmemory config set ollama-chat-model llama3.2
devmemory config set ollama-embedding-model nomic-embed-text
devmemory config set qdrant-collection devmemory_memories
```

---

### Reset configuration

```bash
devmemory config reset
```

Removes persistent configuration and falls back to environment variables or defaults.

---

### Configuration precedence

```text
Environment variables > ~/.devmemory/config.json > default values
```

---

## Vector indexing

### Dry-run indexing

```bash
devmemory index --dry-run
```

Shows what would be indexed without generating embeddings or writing to Qdrant.

Useful options:

```bash
devmemory index --dry-run --limit 1
devmemory index --dry-run --show-text --limit 1
devmemory index --dry-run --project DevMemory
```

---

### Real indexing

```bash
devmemory index
```

Useful options:

```bash
devmemory index --limit 3
devmemory index --force
devmemory index --project DevMemory
devmemory index --area AI
devmemory index --tag qdrant
devmemory index --project DevMemory --area AI --limit 3
```

Normal indexing skips memories that are already indexed and unchanged.

Use `--force` to rebuild index entries.

---

## Semantic search

```bash
devmemory semantic-search <query>
```

Examples:

```bash
devmemory semantic-search "estimate revision cloning"
devmemory semantic-search "mongodb mapping issue" --limit 3
```

Semantic search requires:

```text
embedding provider configured
vector store configured
indexed memories
```

For local usage, this usually means Ollama and Qdrant.

---

## Related memories

```bash
devmemory related <memory-id>
```

Examples:

```bash
devmemory related <memory-id>
devmemory related <memory-id> --limit 3
devmemory related <memory-id> --show-preview
```

This command finds memories semantically related to a specific memory.

---

## RAG questions

### Ask without RAG

```bash
devmemory ask "What did I change last time in this area?"
```

This requires a configured chat provider.

---

### Ask with RAG

```bash
devmemory ask --rag "How did we handle estimate revision cloning?"
```

With context preview:

```bash
devmemory ask --rag --show-context "How did we handle estimate revision cloning?"
```

With result limit:

```bash
devmemory ask --rag "What did I change in MongoDB mapping?" --limit 3
```

RAG requires:

```text
chat provider
embedding provider
vector store
indexed memories
```

---

## Environment variables

| Variable                           | Purpose                                                          |
| ---------------------------------- | ---------------------------------------------------------------- |
| `DEVMEMORY_HOME`                   | Custom DevMemory storage directory                               |
| `DEVMEMORY_CHAT_PROVIDER`          | Chat provider: `none`, `ollama`, `openai`, `gemini`, `anthropic` |
| `DEVMEMORY_EMBEDDING_PROVIDER`     | Embedding provider: `none`, `ollama`, `openai`, `gemini`         |
| `DEVMEMORY_VECTOR_STORE`           | Vector store: `none`, `qdrant`                                   |
| `DEVMEMORY_OLLAMA_ENDPOINT`        | Ollama endpoint                                                  |
| `DEVMEMORY_OLLAMA_CHAT_MODEL`      | Ollama chat model                                                |
| `DEVMEMORY_OLLAMA_EMBEDDING_MODEL` | Ollama embedding model                                           |
| `DEVMEMORY_QDRANT_ENDPOINT`        | Qdrant endpoint                                                  |
| `DEVMEMORY_QDRANT_COLLECTION`      | Qdrant collection name                                           |

Examples:

```bash
DEVMEMORY_HOME=~/devmemory-work devmemory storage
DEVMEMORY_CHAT_PROVIDER=ollama devmemory ai-status
DEVMEMORY_CHAT_PROVIDER=ollama devmemory ask "What did I change last time?"
DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory index
DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory semantic-search "estimate revision"
```

---

## Local AI quick flow

Start local AI services:

```bash
./scripts/dev-ai-local.sh pull-models
./scripts/dev-ai-local.sh start
./scripts/dev-ai-local.sh doctor
```

Configure DevMemory:

```bash
devmemory config set chat-provider ollama
devmemory config set embedding-provider ollama
devmemory config set vector-store qdrant
```

Index and query:

```bash
devmemory index
devmemory semantic-search "your topic"
devmemory ask --rag "your question"
```

Stop local AI services:

```bash
./scripts/dev-ai-local.sh stop
```

---

## Notes

* Core memory commands work without AI.
* Classic search works without AI.
* Markdown export works without AI.
* Git inspection works without AI.
* Knowledge graph export works without AI.
* Semantic search, related memories and RAG require local or external AI/RAG configuration.
* JSON storage is the source of truth.
* Markdown, graph exports and Qdrant vectors are derived artifacts.
