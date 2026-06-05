# DevMemory architecture

This document describes the current architecture of DevMemory.

DevMemory is a local-first .NET CLI designed to capture, search, export, visualize, report and query developer memories.

The project follows a layered architecture with a thin CLI composition layer and clear separation between domain, application logic and infrastructure concerns.

---

## Architecture goals

DevMemory is designed around a few core architectural goals:

* keep the primary memory data local;
* keep JSON storage as the current source of truth;
* treat Markdown, project reports, graph files and vector points as derived artifacts;
* keep AI/RAG optional;
* avoid cloud dependencies for core memory features;
* keep the CLI thin and focused on input/output;
* keep application orchestration independent from technical implementations;
* keep infrastructure details replaceable over time;
* make the project testable at application, infrastructure and CLI level.

---

## Solution structure

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
├── docs
├── scripts
└── docker-compose.ai.yml
```

---

## Layer overview

```mermaid
flowchart LR
    CLI[DevMemory.Cli<br/>Command-line interface] --> APP[DevMemory.Application<br/>Use cases and orchestration]
    APP --> CORE[DevMemory.Core<br/>Domain models]
    APP --> ABSTRACTIONS[Application abstractions]

    INFRA[DevMemory.Infrastructure<br/>Technical implementations] --> ABSTRACTIONS

    INFRA --> JSON[(Local JSON storage)]
    INFRA --> MD[Markdown export]
    INFRA --> REPORTS[Project reports]
    INFRA --> GIT[Git inspection]
    INFRA --> GRAPH[Graph export]
    INFRA --> OLLAMA[Ollama]
    INFRA --> QDRANT[Qdrant]
```

Dependency direction is intentional:

```text
CLI -> Application -> Core
Infrastructure -> Application abstractions
```

The application layer defines the contracts it needs.

The infrastructure layer implements those contracts.

The CLI composes everything together.

---

## DevMemory.Core

`DevMemory.Core` contains the core domain model.

Current main model:

```text
TaskMemory
```

This layer should remain simple and independent.

It should not depend on:

* file system APIs;
* Git command execution;
* CLI parsing;
* JSON persistence implementation details;
* Markdown report output paths;
* Ollama;
* Qdrant;
* environment variables;
* console input/output.

The goal of this layer is to represent the domain concepts without technical coupling.

---

## DevMemory.Application

`DevMemory.Application` contains use cases, orchestration and application abstractions.

Examples of application services:

```text
MemoryService
MemoryInsightsService
MemoryProjectReportService
GitMemoryDraftService
MemoryGraphService
MemoryVectorIndexingService
MemorySemanticSearchService
MemoryRagAnswerService
VectorMemoryDocumentBuilder
MemoryFileFilter
```

Examples of application models:

```text
MemoryInsights
InsightCountItem
MemoryProjectReport
```

Examples of application abstractions:

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

The application layer is responsible for coordinating business behavior, for example:

* adding a memory;
* editing a memory;
* deleting a memory;
* searching memories;
* generating aggregated memory insights;
* generating Markdown project reports;
* generating memory drafts from Git context;
* exporting graph data;
* building indexable documents;
* indexing memories;
* running semantic search;
* building RAG context;
* asking AI providers through abstractions.

The application layer should not know whether memories are stored in JSON, SQLite or another storage provider.

It should also not know whether embeddings come from Ollama, OpenAI or another provider.

Project report generation is application logic because it transforms local memory data into a structured Markdown representation. The CLI decides where to write the generated report file.

---

## DevMemory.Infrastructure

`DevMemory.Infrastructure` contains technical implementations.

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
AiRuntimeConfigurationStore
```

This layer handles:

* JSON file persistence;
* Markdown export;
* Git command execution;
* graph JSON export;
* graph HTML export;
* Ollama integration;
* Qdrant integration;
* persistent local AI/RAG configuration;
* environment-based runtime configuration;
* derived artifact cleanup.

The infrastructure layer is where external technical details belong.

Project reports are currently written by the CLI command handler because they are command outputs, not primary persistence. They are still derived artifacts generated from local JSON memory data.

---

## DevMemory.Cli

`DevMemory.Cli` contains the command-line interface and composition root.

Main responsibilities:

* parse command-line arguments;
* call the appropriate command handler;
* compose application services and infrastructure implementations;
* print results to the terminal;
* write command output files when explicitly requested;
* return meaningful exit codes.

The CLI should stay thin.

Command handlers should delegate meaningful work to application services instead of containing business logic directly.

Main command areas:

```text
memory lifecycle
search
timeline
insights
project reports
Git integration
Markdown export
knowledge graph
diagnostics
setup guidance
AI/RAG
configuration
version/help
```

---

## Local-first data model

DevMemory is local-first.

The primary source of truth is the local JSON storage file.

Default storage path:

```text
~/.devmemory/devmemory.json
```

If `DEVMEMORY_HOME` is set, DevMemory uses that directory instead.

```text
DEVMEMORY_HOME=~/devmemory-work
```

The storage model is intentionally simple at this stage:

```text
TaskMemory[]
```

JSON is currently preferred because it is:

* easy to inspect;
* easy to back up;
* easy to restore;
* portable;
* simple for an early local-first CLI.

SQLite may be introduced later as an optional storage provider.

---

## Source of truth and derived artifacts

DevMemory separates primary data from derived artifacts.

```mermaid
flowchart TD
    A[devmemory.json<br/>Source of truth] --> B[Markdown export]
    A --> C[Knowledge graph JSON]
    A --> D[Knowledge graph HTML]
    A --> E[Memory insights]
    A --> F[Project Markdown reports]
    A --> G[Indexable text]
    G --> H[Embedding vector]
    H --> I[Qdrant point]
```

Primary data:

```text
~/.devmemory/devmemory.json
```

Derived artifacts:

```text
~/.devmemory/markdown/
~/.devmemory/graph/
~/.devmemory/reports/
Qdrant vector points
```

If derived artifacts are deleted, they can be regenerated from JSON storage.

If Qdrant data is lost, memories are not lost.

The vector index can be rebuilt.

---

## Memory lifecycle

The memory lifecycle is centered on local storage.

```mermaid
flowchart TD
    A[devmemory add] --> B[Create TaskMemory]
    B --> C[Persist to JSON]
    C --> D[Export Markdown]

    E[devmemory edit] --> F[Load memory]
    F --> G[Update fields]
    G --> C

    H[devmemory delete] --> I[Remove from JSON]
    I --> J[Clean Markdown export]
    I --> K[Delete Qdrant point when configured]

    C --> L[devmemory insights]
    C --> M[devmemory report]
```

Supported lifecycle commands:

```text
devmemory add
devmemory list
devmemory show <memory-id>
devmemory search <query>
devmemory edit <memory-id> [options]
devmemory delete <memory-id> [--yes]
devmemory timeline
devmemory insights
devmemory report --project <project>
```

---

## Memory insights

Memory insights provide aggregated statistics and practical suggestions based on local memories.

Command:

```bash
devmemory insights
```

The command reads local JSON storage and computes:

```text
total memories
project count
area count
tag count
file reference count
most active projects
most common areas
most used tags
recent activity
suggestions
```

The insights feature is implemented in the application layer through:

```text
MemoryInsightsService
MemoryInsights
InsightCountItem
```

The CLI command handler is responsible only for:

* loading memories through `MemoryService`;
* calling the insights service;
* rendering the result to the terminal;
* returning the appropriate exit code.

Insights do not require AI, Ollama or Qdrant.

They are computed directly from local JSON memory data.

---

## Project reports

Project reports generate Markdown summaries from local memories for a specific project.

Command:

```bash
devmemory report --project <project>
```

With custom output:

```bash
devmemory report --project DevMemory --output ./devmemory-report.md
```

With overwrite enabled:

```bash
devmemory report --project DevMemory --output ./devmemory-report.md --force
```

The report includes:

```text
summary
areas
tags
files touched
timeline
problems
solutions
decisions
tests
lessons learned
suggested next actions
```

The feature is implemented in the application layer through:

```text
MemoryProjectReportService
MemoryProjectReport
```

The application service builds the Markdown content from `TaskMemory` values.

The CLI command handler is responsible for:

* parsing `--project`, `--output` and `--force`;
* loading local memories through `MemoryService`;
* invoking `MemoryProjectReportService`;
* resolving the output path;
* preventing accidental overwrite unless `--force` is used;
* writing the Markdown report to disk;
* printing a concise command summary.

The default report output directory is a derived artifact location under:

```text
~/.devmemory/reports/
```

or under the configured `DEVMEMORY_HOME` directory.

Project reports do not require AI, Ollama or Qdrant.

They are generated entirely from local JSON memory data.

---

## Git integration

Git integration is implemented as an infrastructure concern behind an application abstraction.

The CLI command:

```bash
devmemory git-status
```

uses the Git repository inspector to collect:

```text
repository path
current branch
last commit hash
last commit message
changed files
```

The command:

```bash
devmemory learn-from-git
```

uses Git context to create a memory draft.

Generated files are filtered to avoid polluting memories with build output, local artifacts or generated files.

---

## Markdown export

Each saved memory is exported to Markdown.

Default directory:

```text
~/.devmemory/markdown/
```

Markdown export is a derived artifact.

It exists to make memories easy to reuse in:

* documentation;
* AI assistant prompts;
* code review notes;
* project notes;
* external tools.

Markdown export should not become the primary source of truth.

Project reports are separate Markdown artifacts: they summarize multiple memories for a project, while per-memory Markdown exports represent individual memories.

---

## Knowledge graph

DevMemory can export memories as a local graph.

Commands:

```bash
devmemory graph-export
devmemory graph-view
```

The graph currently represents relationships between:

```text
Memory
Project
Area
Tag
File
```

The graph JSON and HTML files are derived artifacts generated from local memory storage.

---

## AI/RAG architecture

AI/RAG is optional.

Core memory commands do not require AI providers, Ollama or Qdrant.

This includes:

```text
devmemory add
devmemory list
devmemory search
devmemory show
devmemory edit
devmemory delete
devmemory timeline
devmemory insights
devmemory report
devmemory graph-export
devmemory graph-view
```

The AI/RAG flow uses three main abstractions:

```text
IEmbeddingService
IVectorMemoryStore
IChatCompletionService
```

Current local implementations:

```text
OllamaEmbeddingService
OllamaChatCompletionService
QdrantVectorMemoryStore
```

---

## Vector indexing flow

```mermaid
flowchart TD
    A[Local memories] --> B[Build indexable document]
    B --> C[Compute content hash]
    C --> D{Already indexed<br/>and unchanged?}
    D -->|Yes| E[Skip]
    D -->|No| F[Generate embedding]
    F --> G[Upsert into Qdrant]
```

Indexing command:

```bash
devmemory index
```

Dry-run command:

```bash
devmemory index --dry-run
```

The index can be rebuilt because the source of truth remains the local JSON storage.

---

## Semantic search flow

```mermaid
flowchart TD
    A[Search query] --> B[Generate query embedding]
    B --> C[Search Qdrant]
    C --> D[Return matching memory payloads]
    D --> E[Render semantic results]
```

Command:

```bash
devmemory semantic-search "your topic"
```

Semantic search requires:

* configured embedding provider;
* configured vector store;
* indexed memories.

---

## RAG flow

```mermaid
sequenceDiagram
    participant User
    participant CLI as DevMemory CLI
    participant Emb as Embedding service
    participant Vector as Vector store
    participant Chat as Chat completion service

    User->>CLI: devmemory ask --rag "Question"
    CLI->>Emb: Generate question embedding
    Emb-->>CLI: Query vector
    CLI->>Vector: Search related memories
    Vector-->>CLI: Relevant memory payloads
    CLI->>CLI: Build RAG prompt
    CLI->>Chat: Ask with context
    Chat-->>CLI: Answer
    CLI-->>User: Answer
```

Command:

```bash
devmemory ask --rag "your question"
```

RAG requires:

* chat provider;
* embedding provider;
* vector store;
* indexed memories.

---

## Configuration model

DevMemory supports runtime configuration through environment variables and persistent local configuration.

Configuration precedence:

```text
Environment variables > ~/.devmemory/config.json > default values
```

This allows two usage styles:

* temporary overrides through environment variables;
* persistent local defaults through `devmemory config set`.

Examples:

```bash
devmemory config set chat-provider ollama
devmemory config set embedding-provider ollama
devmemory config set vector-store qdrant
```

---

## Diagnostics

Diagnostics are split into two levels.

General diagnostics:

```bash
devmemory doctor
```

Checks:

```text
storage readability
Markdown directory
persistent configuration
AI/RAG runtime configuration
Git availability
memory count
```

AI runtime diagnostics:

```bash
devmemory ai-doctor
```

Checks:

```text
chat provider
embedding provider
vector store
Ollama/Qdrant readiness when configured
```

---

## Testing strategy

The project has tests at multiple levels:

```text
DevMemory.Application.Tests
DevMemory.Infrastructure.Tests
DevMemory.Cli.Tests
```

The test suite covers:

* memory service behavior;
* memory insights aggregation;
* project report generation;
* validation and normalization;
* ranked search;
* editing;
* deletion;
* Markdown cleanup;
* JSON storage;
* Markdown export;
* Git memory draft creation;
* generated file filtering;
* graph export;
* vector indexing;
* semantic search;
* related memories;
* RAG orchestration;
* persistent AI configuration;
* doctor diagnostics;
* setup command behavior;
* CLI command parsing;
* package smoke behavior.

CLI tests that depend on local storage use isolated temporary `DEVMEMORY_HOME` directories to avoid reading or modifying real user data.

---

## Release engineering

The release pipeline is shared between local development and GitHub Actions.

Main validation command:

```bash
./scripts/release-check.sh
```

The release check validates:

```text
build and tests
repository hygiene
repository support files
documentation links
changelog entry
version consistency
package artifact structure
CLI package smoke test
final package checksum
```

This makes local validation and CI validation consistent.

The CLI package smoke test also verifies non-AI commands such as setup, help, storage, insights and report generation using isolated temporary local data.

---

## Current architectural limitations

Current limitations are intentional and acceptable for the current stage:

* primary storage is JSON-based;
* CLI parsing is manual;
* SQLite storage is not available yet;
* hosted/cloud sync is not available;
* graph visualization is simple and static;
* setup guidance is not fully interactive yet;
* local AI/RAG requires external local services such as Ollama and Qdrant;
* project reports are generated as static Markdown files;
* report filtering is currently project-based only.

These are candidates for future evolution, not blockers for the current architecture.

---

## Future architectural directions

Possible future improvements:

* optional SQLite storage provider;
* interactive setup wizard;
* richer CLI rendering;
* stronger command-line parsing with `System.CommandLine` or `Spectre.Console`;
* richer memory insights with filters and trends;
* richer report generation with date ranges, tag filters and area filters;
* improved graph layout and filtering;
* local web UI or TUI;
* VS Code extension;
* MCP integration;
* optional cloud LLM provider hardening;
* release automation.

---

## Design principle

The most important architectural principle is:

> Local JSON memory is the source of truth. Everything else is derived, replaceable or optional.

This keeps DevMemory safe, inspectable and portable while still allowing richer features such as Markdown export, project reports, graph views, semantic search and RAG.
