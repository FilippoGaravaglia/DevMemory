# DevMemory

<div align="center">

### Local-first AI memory for developers

**DevMemory** is a local-first developer memory CLI that helps you capture technical context, index it semantically, and ask questions about your past work using local AI.

It is designed for developers who want to preserve decisions, fixes, task context, Git activity, lessons learned, and project knowledge across branches, repositories, code reviews, and AI-assisted development sessions.

</div>

---

## What is DevMemory?

DevMemory is a **.NET CLI tool** that acts as a personal engineering memory.

It lets you save structured memories about the work you do every day:

* what problem you solved;
* what solution you implemented;
* what decisions you made;
* which files you touched;
* which tests you ran;
* what you learned;
* which project, area, branch and tags the task belongs to.

Those memories are stored locally, can be searched with classic text search, exported to Markdown, visualized as a knowledge graph, indexed into a vector database, and queried through a local RAG pipeline powered by Ollama and Qdrant.

The goal is simple:

> Stop losing technical context between tasks, branches, commits and AI chat sessions.

---

## Why DevMemory?

As developers, we often solve problems and then lose the context a few days later.

Questions like these are common:

* What did I change last time in this area?
* Why did I make that technical decision?
* Which files were involved in that refactor?
* How did I solve a similar MongoDB mapping issue?
* What tests did I run for that bug fix?
* What did I learn from that task?
* Can I ask an AI assistant using my previous technical work as context?

DevMemory is built to answer those questions from your own local engineering memory.

---

## Key features

* Local-first structured developer memories.
* JSON-based local storage.
* Markdown export for every memory.
* Ranked text search with filters.
* Git repository inspection.
* Memory draft creation from Git context.
* Knowledge graph JSON export.
* Local HTML knowledge graph view.
* Generated-file filtering.
* Vector indexing with embeddings.
* Semantic search through Qdrant.
* Local RAG answers through Ollama.
* AI runtime diagnostics.
* Backup and restore scripts.
* Release-ready package validation.
* Installable as a .NET global tool.
* CI, formatting checks, tests, release checks and package artifact verification.

---

## Core workflow

A typical DevMemory workflow looks like this:

```bash
devmemory add
devmemory list
devmemory search "revision"
devmemory index
devmemory semantic-search "estimate revision cloning"
devmemory ask --rag "How did we validate the local AI runtime?"
```

At a high level:

```text
Structured memory
      ↓
Local JSON storage
      ↓
Indexable text
      ↓
Embedding model
      ↓
Qdrant vector store
      ↓
Semantic retrieval
      ↓
RAG prompt
      ↓
Local LLM answer
```

---

## Local-first by default

DevMemory stores your primary data locally.

Default storage path:

```text
~/.devmemory/devmemory.json
```

Markdown exports:

```text
~/.devmemory/markdown/
```

Knowledge graph exports:

```text
~/.devmemory/graph/
```

You can customize the storage directory with:

```bash
DEVMEMORY_HOME=~/devmemory-work devmemory storage
```

The local JSON storage is the source of truth.
The vector database is a derived index and can be rebuilt.

---

## AI and RAG support

DevMemory supports a local AI workflow using:

* **Ollama** for local chat and embeddings;
* **nomic-embed-text** for embeddings;
* **llama3.2** for local chat/RAG answers;
* **Qdrant** as local vector store;
* **Docker** to run Qdrant locally.

The validated local AI flow is:

```text
DevMemory memories
      ↓
nomic-embed-text embeddings through Ollama
      ↓
Qdrant vector index
      ↓
semantic search
      ↓
retrieved memory context
      ↓
llama3.2 answer through Ollama
```

Example:

```bash
DEVMEMORY_CHAT_PROVIDER=ollama \
DEVMEMORY_EMBEDDING_PROVIDER=ollama \
DEVMEMORY_VECTOR_STORE=qdrant \
devmemory ask --rag "How did we validate the local AI runtime in DevMemory?" --limit 3
```

With `--show-context`, DevMemory also prints the memories used as RAG context:

```bash
DEVMEMORY_CHAT_PROVIDER=ollama \
DEVMEMORY_EMBEDDING_PROVIDER=ollama \
DEVMEMORY_VECTOR_STORE=qdrant \
devmemory ask --rag --show-context "How did we validate the local AI runtime in DevMemory?" --limit 3
```

---

## Validated local AI runtime

The local AI runtime has been validated end-to-end with:

```text
Ollama
llama3.2
nomic-embed-text
Docker
Qdrant
DevMemory vector indexing
DevMemory semantic search
DevMemory RAG answers
```

Validated flow:

```text
JSON memory
→ indexable document
→ Ollama embedding
→ Qdrant upsert
→ semantic retrieval
→ RAG context
→ Ollama chat completion
→ answer shown in CLI
```

---

## Installation

### Prerequisites

* .NET SDK 10
* Git
* macOS, Linux or Windows terminal environment
* Docker Desktop, only for local Qdrant/vector search
* Ollama, only for local AI/RAG

---

### Package the CLI

From the repository root:

```bash
dotnet pack src/DevMemory.Cli/DevMemory.Cli.csproj -c Release -o artifacts/packages
```

---

### Install as a .NET global tool

```bash
dotnet tool install --global DevMemory.Cli --add-source ./artifacts/packages
```

If already installed:

```bash
dotnet tool update --global DevMemory.Cli --add-source ./artifacts/packages
```

Make sure .NET global tools are available in your `PATH`:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

Then verify:

```bash
devmemory --version
devmemory help
```

---

## Recommended local installation

For local development, use the provided script:

```bash
./scripts/install-local-tool.sh
```

This script:

* builds the solution;
* runs the test suite;
* packages the CLI;
* uninstalls any previous local global-tool installation;
* installs the new package globally;
* verifies the installed command.

After this step, you can use:

```bash
devmemory
```

from anywhere.

---

## Basic usage

### Add a memory

```bash
devmemory add
```

A memory contains:

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

---

### Search memories

```bash
devmemory search "revision"
```

With filters:

```bash
devmemory search "revision" --project LogicalCommon
devmemory search "revision" --area Estimate
devmemory search "revision" --tag mongodb
```

---

### Show a memory

```bash
devmemory show <memory-id>
```

---

### Show storage path

```bash
devmemory storage
```

---

### Show Markdown export directory

```bash
devmemory markdown
```

---

## Git integration

### Inspect current repository

```bash
devmemory git-status
```

Inspect another repository:

```bash
devmemory git-status --path ~/work/my-repository
```

DevMemory reads:

* repository path;
* current branch;
* last commit hash;
* last commit message;
* changed files.

Generated files are filtered automatically.

---

### Create a memory draft from Git context

```bash
devmemory learn-from-git
```

This creates a memory draft using current Git information, including project, branch and files touched.

You then complete the meaningful engineering context manually:

* problem;
* solution;
* decisions;
* tests;
* lessons learned.

---

## Markdown export

Every saved memory is exported to Markdown.

Default directory:

```text
~/.devmemory/markdown/
```

Markdown exports include:

* metadata;
* problem;
* solution;
* decisions;
* files touched;
* tests;
* lessons learned;
* continuation prompt.

This makes memories easy to reuse in documentation, ChatGPT, GitHub Copilot, Claude, local LLMs or future AI-assisted workflows.

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

The graph includes relationships such as:

```text
Memory → Project
Memory → Area
Memory → Tag
Memory → File
```

---

### Generate local HTML graph view

```bash
devmemory graph-view
```

Default output:

```text
~/.devmemory/graph/devmemory-graph.html
```

Open it in the browser:

```bash
open ~/.devmemory/graph/devmemory-graph.html
```

The graph currently visualizes:

* memory nodes;
* project nodes;
* area nodes;
* tag nodes;
* file nodes.

---

## Local AI setup

### 1. Install Docker Desktop

Docker is used to run Qdrant locally.

Verify Docker:

```bash
docker --version
docker compose version
```

---

### 2. Install Ollama

Ollama is used for local chat and embeddings.

Verify Ollama:

```bash
ollama --version
curl http://localhost:11434/api/tags
```

---

### 3. Pull local models

```bash
./scripts/dev-ai-local.sh pull-models
```

This pulls:

```text
llama3.2
nomic-embed-text
```

Verify:

```bash
ollama list
```

---

### 4. Start local AI services

```bash
./scripts/dev-ai-local.sh start
```

This starts Qdrant through Docker.

Verify Qdrant:

```bash
curl http://localhost:6333/collections
```

---

### 5. Diagnose local AI runtime

```bash
./scripts/dev-ai-local.sh doctor
```

Expected result when everything is ready:

```text
Result: AI environment looks ready.
```

---

## Vector indexing

### Dry-run indexing

Preview what will be indexed without generating embeddings or writing to Qdrant:

```bash
devmemory index --dry-run
```

Show indexable text:

```bash
devmemory index --dry-run --show-text --limit 1
```

---

### Real indexing

```bash
DEVMEMORY_CHAT_PROVIDER=ollama \
DEVMEMORY_EMBEDDING_PROVIDER=ollama \
DEVMEMORY_VECTOR_STORE=qdrant \
devmemory index
```

Useful options:

```bash
devmemory index --limit 3
devmemory index --force
devmemory index --project DevMemory
devmemory index --area AI
devmemory index --tag qdrant
```

---

## Semantic search

Semantic search uses embeddings and Qdrant.

```bash
DEVMEMORY_CHAT_PROVIDER=ollama \
DEVMEMORY_EMBEDDING_PROVIDER=ollama \
DEVMEMORY_VECTOR_STORE=qdrant \
devmemory semantic-search "local AI runtime validation" --limit 3
```

Unlike classic text search, semantic search can retrieve memories based on conceptual similarity.

---

## RAG questions

Ask a question using retrieved memories as context:

```bash
DEVMEMORY_CHAT_PROVIDER=ollama \
DEVMEMORY_EMBEDDING_PROVIDER=ollama \
DEVMEMORY_VECTOR_STORE=qdrant \
devmemory ask --rag "How did we validate the local AI runtime in DevMemory?" --limit 3
```

Show the retrieved context:

```bash
DEVMEMORY_CHAT_PROVIDER=ollama \
DEVMEMORY_EMBEDDING_PROVIDER=ollama \
DEVMEMORY_VECTOR_STORE=qdrant \
devmemory ask --rag --show-context "How did we validate the local AI runtime in DevMemory?" --limit 3
```

---

## AI commands

### Show AI status

```bash
devmemory ai-status
```

### Diagnose local AI runtime

```bash
devmemory ai-doctor
```

or through the helper script:

```bash
./scripts/dev-ai-local.sh doctor
```

### Ask without RAG

```bash
DEVMEMORY_CHAT_PROVIDER=ollama devmemory ask "Reply with only: hello"
```

### Ask with RAG

```bash
DEVMEMORY_CHAT_PROVIDER=ollama \
DEVMEMORY_EMBEDDING_PROVIDER=ollama \
DEVMEMORY_VECTOR_STORE=qdrant \
devmemory ask --rag "What did I decide about Qdrant?"
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

Default local AI values:

```text
Ollama endpoint:          http://localhost:11434
Ollama chat model:        llama3.2
Ollama embedding model:   nomic-embed-text
Qdrant endpoint:          http://localhost:6333
Qdrant collection:        devmemory_memories
```

---

## Backup and restore

DevMemory is local-first, so backing up the local storage is important.

### Backup local data

```bash
./scripts/backup-devmemory-data.sh
```

This creates a timestamped backup and SHA-256 checksum.

You can customize the backup directory:

```bash
DEVMEMORY_BACKUP_DIR=~/Backups/devmemory ./scripts/backup-devmemory-data.sh
```

---

### Restore local data

Dry-run restore:

```bash
./scripts/restore-devmemory-data.sh ~/.devmemory/backups/<backup-file>.json --dry-run
```

Real restore:

```bash
./scripts/restore-devmemory-data.sh ~/.devmemory/backups/<backup-file>.json
```

The restore script:

* validates the backup file;
* validates checksum when available;
* creates a pre-restore backup of the current storage;
* requires explicit confirmation unless `--yes` is used.

---

## Local development scripts

### Build and test

```bash
./scripts/build-test.sh
```

This script:

* validates shell scripts;
* verifies formatting;
* builds the solution;
* runs the full test suite.

---

### Release check

```bash
./scripts/release-check.sh
```

The release check validates:

1. build and tests;
2. repository hygiene;
3. changelog entry;
4. version consistency;
5. package artifact structure;
6. CLI package smoke test;
7. final package checksum.

---

### Package release artifacts

```bash
./scripts/pack-release.sh
```

This runs the release check and prints the final package summary.

Generated artifacts:

```text
artifacts/packages/DevMemory.Cli.<version>.nupkg
artifacts/packages/DevMemory.Cli.<version>.nupkg.sha256
```

---

### Verify installed tool

```bash
./scripts/verify-installed-tool.sh
```

This verifies that the globally installed `devmemory` command works correctly.

---

### Local AI helper

```bash
./scripts/dev-ai-local.sh help
```

Available commands:

```text
setup
start
stop
doctor
pull-models
smoke
help
```

---

### Local AI smoke test

```bash
DEVMEMORY_AI_SMOKE_HOME="$HOME/.devmemory" ./scripts/dev-ai-local.sh smoke
```

This validates the local AI flow using the configured Ollama and Qdrant runtime.

---

### Clean generated files

```bash
./scripts/clean-generated.sh
```

This removes local generated files and build artifacts, but does not remove the real user storage under:

```text
~/.devmemory/
```

---

## Architecture

DevMemory follows a layered architecture.

```text
DevMemory.slnx
├── src
│   ├── DevMemory.Core
│   ├── DevMemory.Application
│   ├── DevMemory.Infrastructure
│   └── DevMemory.Cli
├── tests
│   ├── DevMemory.Application.Tests
│   ├── DevMemory.Infrastructure.Tests
│   └── DevMemory.Cli.Tests
├── scripts
└── docker-compose.ai.yml
```

---

### DevMemory.Core

Contains core domain models.

Example:

```text
TaskMemory
```

This layer does not depend on infrastructure, CLI, file system or Git implementation details.

---

### DevMemory.Application

Contains application behavior, abstractions and orchestration.

Examples:

```text
MemoryService
GitMemoryDraftService
MemoryGraphService
MemoryVectorIndexingService
MemorySemanticSearchService
MemoryRagAnswerService
VectorMemoryDocumentBuilder
MemoryFileFilter
```

Application abstractions include:

```text
IMemoryRepository
IMemoryExporter
IGitRepositoryInspector
IMemoryGraphExporter
IMemoryGraphHtmlExporter
IEmbeddingService
IChatCompletionService
IVectorMemoryStore
```

---

### DevMemory.Infrastructure

Contains technical implementations.

Examples:

```text
MemoryRepository
MarkdownMemoryExporter
GitRepositoryInspector
JsonMemoryGraphExporter
HtmlMemoryGraphExporter
OllamaChatCompletionService
OllamaEmbeddingService
QdrantVectorMemoryStore
AiRuntimeOptionsProvider
```

This layer handles:

* JSON storage;
* Markdown export;
* Git command execution;
* graph JSON export;
* graph HTML export;
* Ollama integration;
* Qdrant integration;
* environment-based runtime configuration.

---

### DevMemory.Cli

Contains the command-line interface and composition root.

Main commands:

```text
add
list
search
show
storage
markdown
git-status
learn-from-git
graph-export
graph-view
ai-status
ai-doctor
ask
index
semantic-search
version
help
```

The CLI entry point delegates command execution to dedicated command handlers.

---

## Quality and release engineering

DevMemory is developed as a production-oriented portfolio project.

Current quality practices include:

* layered architecture;
* clear separation between domain, application, infrastructure and CLI;
* unit tests for application, infrastructure and CLI behavior;
* formatting verification with `dotnet format`;
* repository-wide `.editorconfig`;
* repository line-ending normalization with `.gitattributes`;
* shared build configuration through `Directory.Build.props`;
* GitHub Actions CI;
* local release-check pipeline;
* NuGet package artifact verification;
* installed CLI smoke test;
* package checksum generation;
* repository hygiene verification;
* changelog verification;
* backup and restore scripts;
* local AI diagnostics and smoke testing.

---

## Testing

Run all tests:

```bash
dotnet test DevMemory.slnx
```

Recommended full local validation:

```bash
./scripts/release-check.sh
```

The test suite covers:

* memory service behavior;
* validation and normalization;
* ranked text search;
* JSON storage;
* Markdown export;
* Git memory draft creation;
* generated file filtering;
* graph generation;
* JSON graph export;
* HTML graph export;
* vector indexing;
* semantic search;
* RAG answer orchestration;
* Ollama integration;
* Qdrant integration;
* CLI command parsing and dispatching;
* CLI package behavior.

---

## CI

The GitHub Actions workflow runs the shared release-check pipeline.

It validates:

* formatting;
* build;
* tests;
* repository hygiene;
* changelog;
* version consistency;
* package structure;
* local package smoke test;
* checksum generation.

The workflow uploads:

```text
DevMemory.Cli.<version>.nupkg
DevMemory.Cli.<version>.nupkg.sha256
```

as build artifacts.

---

## Current release status

Current version:

```text
0.1.3
```

DevMemory can currently be packaged and installed locally as a .NET global tool.

The package is not published to NuGet yet.

Release artifacts are generated locally under:

```text
artifacts/packages/
```

---

## Current limitations

DevMemory is under active development.

Current limitations:

* CLI parsing is implemented manually.
* Primary storage is JSON-based.
* No SQLite storage yet.
* No hosted/cloud sync.
* No desktop UI.
* HTML graph layout is simple and static.
* Public NuGet publishing is not configured yet.
* GitHub Releases are not automated yet.
* Local AI requires Ollama and Docker/Qdrant to be running.
* The quality of RAG answers depends on the quality of saved memories and the selected LLM.

---

## Roadmap

Planned improvements:

* Improve README with screenshots and demo GIFs.
* Add a smoother first-run setup wizard.
* Improve CLI rendering with optional colors/tables.
* Evaluate `System.CommandLine` or `Spectre.Console`.
* Add SQLite storage option.
* Improve memory editing and deletion flows.
* Improve Git diff summarization.
* Improve graph filtering and visualization.
* Add benchmark/evaluation cases for RAG quality.
* Add optional cloud LLM provider hardening.
* Add GitHub Release automation.
* Publish as a public or private NuGet tool package.
* Explore a TUI or local web UI.

---

## Privacy

DevMemory is local-first.

By default:

* no cloud sync;
* no external API calls for core memory features;
* no data leaves your machine;
* local AI can run through Ollama;
* vector data is stored locally in Qdrant.

Primary memory data is stored in:

```text
~/.devmemory/devmemory.json
```

or in the directory configured through:

```bash
DEVMEMORY_HOME
```

---

## Project goal

DevMemory aims to become a practical local memory layer for software engineers.

It is designed to help developers preserve technical context across:

* tasks;
* branches;
* repositories;
* commits;
* code reviews;
* bug fixes;
* refactors;
* architectural decisions;
* AI-assisted development sessions.

The long-term vision is to provide a local, searchable and AI-queryable engineering memory that developers can use every day.

---

## License

License not defined yet.
