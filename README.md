<div align="center">

# DevMemory

### Local-first developer memory for .NET engineers

DevMemory is a local CLI tool that helps developers capture, search, export and visualize technical knowledge produced during day-to-day software engineering work.

It is designed to avoid losing context across tasks, branches, commits, code reviews and AI/chat sessions.

</div>

---

## Overview

DevMemory is a **local-first developer memory CLI** built with .NET.

It allows developers to save structured engineering memories containing:

- project
- area
- branch
- tags
- problem
- solution
- decisions
- files touched
- tests
- lessons learned

Those memories can then be searched, exported to Markdown, enriched from Git context, and visualized as a knowledge graph.

The project is currently under active development and is intended both as a practical daily developer tool and as a portfolio project focused on clean .NET architecture, CLI tooling, Git integration and local knowledge management.

---

## Why DevMemory?

When working on multiple tasks, branches, repositories and AI-assisted development sessions, useful technical context is often lost.

DevMemory helps answer questions such as:

- What did I change last time in this area?
- Which files are usually involved in this feature?
- What decisions did I make during that refactor?
- Which previous tasks touched this component?
- What lessons did I learn from a bug fix?
- Which memories are related to this project, tag or file?

---

## Demo

DevMemory is designed to be used directly from the terminal.

Example workflow:

```bash
devmemory add
devmemory list
devmemory search revision
devmemory show <memory-id>
devmemory git-status
devmemory learn-from-git
devmemory graph-export
devmemory graph-view
```

Example use cases:

- save technical context after completing a task;
- preserve decisions made during a bug fix or refactor;
- search previous work by project, area or tag;
- export memories to Markdown for documentation or AI-assisted continuation;
- inspect the current Git repository and create a prefilled memory draft;
- visualize relationships between memories, projects, tags and files.

### Screenshots

Screenshots will be added as the project reaches the first stable portfolio release.

Planned screenshots:

- CLI help output;
- memory list output;
- ranked search output;
- HTML knowledge graph view.

<!--
![DevMemory help](docs/assets/devmemory-help.png)
![DevMemory search](docs/assets/devmemory-search.png)
![DevMemory graph](docs/assets/devmemory-graph.png)
-->

---

## Features

### Structured task memories

Create a new memory from the terminal:

```bash
devmemory add
```

Each memory stores technical context in a structured format:

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

### Local-first storage

By default, DevMemory stores all data locally:

```text
~/.devmemory/devmemory.json
```

No cloud sync, no external services and no LLM API are required.

You can customize the storage directory with:

```bash
DEVMEMORY_HOME=~/devmemory-work devmemory storage
```

---

### Resilient JSON persistence

DevMemory currently uses JSON storage with defensive writes:

- writes to a temporary file;
- creates a backup file;
- replaces the main storage file only after a successful write.

This reduces the risk of corrupting the local memory store.

---

### Markdown export

Every saved memory is automatically exported to Markdown:

```text
~/.devmemory/markdown/
```

The generated Markdown includes:

- metadata;
- problem;
- solution;
- decisions;
- files touched;
- tests;
- lessons learned;
- continuation prompt.

This makes memories easy to reuse in documentation, GitHub Copilot, ChatGPT or future AI-assisted workflows.

---

### Ranked search with filters

Search memories by free text:

```bash
devmemory search revision
```

Filter by project:

```bash
devmemory search revision --project LogicalCommon
```

Filter by area:

```bash
devmemory search revision --area Estimate
```

Filter by tag:

```bash
devmemory search revision --tag dotnet
```

Search results include a relevance score.

---

### Git inspection

Inspect the current Git repository:

```bash
devmemory git-status
```

Or inspect a specific repository:

```bash
devmemory git-status --path ~/work/LogicalCommon
```

DevMemory reads:

- repository path;
- current branch;
- last commit hash;
- last commit message;
- changed files.

Generated files are automatically excluded.

---

### Learn from Git

Create a new memory starting from the current Git context:

```bash
devmemory learn-from-git
```

This pre-fills:

- project;
- branch;
- files touched.

You then complete the memory manually with:

- area;
- tags;
- problem;
- solution;
- decisions;
- tests;
- lessons learned.

---

### Knowledge graph export

Export all stored memories as a graph JSON:

```bash
devmemory graph-export
```

Default output:

```text
~/.devmemory/graph/devmemory-graph.json
```

The graph contains relationships such as:

```text
Memory -> Project
Memory -> Area
Memory -> Tag
Memory -> File
```

---

### HTML knowledge graph view

Generate a local HTML visualization of the knowledge graph:

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

- memory nodes;
- project nodes;
- area nodes;
- tag nodes;
- file nodes.

---

### Generated file filtering

DevMemory automatically excludes noisy/generated files from Git inspection and graph generation:

```text
bin/
obj/
artifacts/
.git/
.vs/
.idea/
node_modules/
dist/
build/
coverage/
*.dll
*.exe
*.pdb
*.nupkg
*.snupkg
*.deps.json
*.runtimeconfig.json
```

This keeps the generated knowledge graph focused on meaningful source files.

---

## Architecture

DevMemory is structured as a layered .NET solution.

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
└── scripts
```

### DevMemory.Core

Contains the core domain models.

Example:

```text
TaskMemory
```

This project does not depend on infrastructure, CLI, file system or Git implementation details.

---

### DevMemory.Application

Contains application services, abstractions, validation, normalization, filtering, search logic, graph generation and Git memory draft creation.

Examples:

```text
MemoryService
GitMemoryDraftService
MemoryGraphService
MemoryFileFilter
IMemoryRepository
IMemoryExporter
IGitRepositoryInspector
IMemoryGraphExporter
IMemoryGraphHtmlExporter
```

The application layer defines contracts and application behavior.

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
DevMemoryStorageOptions
```

This layer handles:

- JSON storage;
- Markdown export;
- Git command execution;
- graph JSON export;
- graph HTML export;
- environment-based storage configuration.

---

### DevMemory.Cli

Contains the terminal interface and command dispatching.

Current commands:

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
version
help
```

The CLI entry point is intentionally kept small and works as a composition root.

Command execution is delegated to dedicated command handlers:

```text
AddCommandHandler
ListCommandHandler
SearchCommandHandler
ShowCommandHandler
StorageCommandHandler
MarkdownCommandHandler
GitStatusCommandHandler
LearnFromGitCommandHandler
GraphExportCommandHandler
GraphViewCommandHandler
VersionCommandHandler
HelpCommandHandler
```

Supporting CLI components include:

```text
CommandDispatcher
CommandOptions
CliPrompt
CliExitCodes
MemoryConsolePrinter
```

---

## Quality and engineering practices

DevMemory is developed as a production-oriented portfolio project, with attention to maintainability, testability and release readiness.

Current quality practices include:

- layered architecture;
- separation between domain, application, infrastructure and CLI concerns;
- local-first storage abstraction;
- unit tests for application, infrastructure and CLI behavior;
- GitHub Actions CI pipeline;
- build, test and package validation on CI;
- formatting verification with `dotnet format`;
- repository-wide `.editorconfig`;
- repository line-ending normalization with `.gitattributes`;
- shared build configuration through `Directory.Build.props`;
- local development scripts for repeatable build, test, packaging and cleanup;
- NuGet global tool packaging;
- changelog and semantic versioning baseline.

The CI pipeline verifies that the solution can be restored, formatted, built, tested and packaged successfully.

---

## Installation

### Package the CLI

```bash
dotnet pack src/DevMemory.Cli/DevMemory.Cli.csproj -c Release -o artifacts/packages
```

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

Then run:

```bash
devmemory help
```

---

## Usage

### Add a memory

```bash
devmemory add
```

### List memories

```bash
devmemory list
```

### Search memories

```bash
devmemory search revision
```

With filters:

```bash
devmemory search revision --project LogicalCommon
devmemory search revision --area Estimate
devmemory search revision --tag dotnet
```

### Show a memory

```bash
devmemory show <memory-id>
```

### Show storage path

```bash
devmemory storage
```

### Show Markdown directory

```bash
devmemory markdown
```

### Inspect Git status

```bash
devmemory git-status
```

### Create memory from Git context

```bash
devmemory learn-from-git
```

### Export graph JSON

```bash
devmemory graph-export
```

### Generate HTML graph view

```bash
devmemory graph-view
```

### Show version

```bash
devmemory version
```

Short aliases:

```bash
devmemory --version
devmemory -v
```

### Show help

```bash
devmemory help
```

Short aliases:

```bash
devmemory --help
devmemory -h
```

---

## Configuration

### Default storage

```text
~/.devmemory
```

Main storage file:

```text
~/.devmemory/devmemory.json
```

Markdown exports:

```text
~/.devmemory/markdown/
```

Graph exports:

```text
~/.devmemory/graph/
```

### Custom storage directory

```bash
DEVMEMORY_HOME=~/devmemory-work devmemory storage
```

---

## Local development scripts

DevMemory includes local development scripts to simplify the build, test, packaging and cleanup workflow.

### Build and test

Run the full build and test pipeline:

```bash
./scripts/build-test.sh
```

This script:

- verifies code formatting;
- builds the solution;
- runs the full test suite;
- fails fast if formatting, build or tests fail.

---

### Install as a local global tool

Package and install DevMemory locally as a .NET global tool:

```bash
./scripts/install-local-tool.sh
```

This script:

- builds the solution;
- runs all tests;
- cleans the local package output directory;
- creates the NuGet package under `artifacts/packages`;
- uninstalls any previous local global-tool installation;
- installs the new package globally;
- verifies the installation by running:

```bash
devmemory help
```

After this step, the CLI is available from anywhere with:

```bash
devmemory
```

---

### Create a release package

Create a versioned release package:

```bash
./scripts/pack-release.sh 0.1.3
```

This script:

- cleans the local package output directory;
- builds the solution in Release mode;
- runs all tests;
- creates a versioned NuGet package under `artifacts/packages`.

The generated package can then be installed locally with:

```bash
dotnet tool install --global DevMemory.Cli --add-source ./artifacts/packages
```

---

### Clean generated files

Remove local generated files and build artifacts:

```bash
./scripts/clean-generated.sh
```

This script removes:

- `bin/`;
- `obj/`;
- `artifacts/`;
- local `devmemory.json` files generated during development.

It does not remove the real user storage under:

```text
~/.devmemory/
```

---

### Recommended local workflow

Before opening a pull request or creating a release package, run:

```bash
./scripts/build-test.sh
```

When testing the CLI as an installed tool, run:

```bash
./scripts/install-local-tool.sh
```

When you want to clean the workspace from generated files, run:

```bash
./scripts/clean-generated.sh
```

---

## Testing

Run all tests:

```bash
dotnet test DevMemory.slnx
```

The current test suite covers:

- memory service behavior;
- validation and normalization;
- ranked search;
- storage repository;
- Markdown export;
- Git memory draft creation;
- generated file filtering;
- graph generation;
- JSON graph export;
- HTML graph export;
- CLI command option parsing;
- CLI command dispatching;
- CLI version command behavior.

---

## Development commands

Build the solution:

```bash
dotnet build DevMemory.slnx
```

Run tests:

```bash
dotnet test DevMemory.slnx
```

Verify formatting:

```bash
dotnet format DevMemory.slnx --verify-no-changes
```

Run the CLI from source:

```bash
dotnet run --project src/DevMemory.Cli -- help
```

Package the CLI:

```bash
dotnet pack src/DevMemory.Cli/DevMemory.Cli.csproj -c Release -o artifacts/packages
```

Update the locally installed global tool:

```bash
dotnet tool update --global DevMemory.Cli --add-source ./artifacts/packages
```

For the recommended local workflow, prefer using the scripts under `scripts/`.

---

## Release status

Current development version:

```text
0.1.3
```

DevMemory can currently be packaged and installed locally as a .NET global tool.

The project is not published to NuGet yet. For now, releases are created locally through:

```bash
./scripts/pack-release.sh 0.1.3
```

The generated package is available under:

```text
artifacts/packages/
```

Local installation can be tested with:

```bash
./scripts/install-local-tool.sh
```

A public GitHub release can be created once the README, screenshots and final smoke tests are completed.

---

## Current limitations

DevMemory is still under active development.

Current limitations:

- CLI parsing is currently implemented manually.
- Storage is JSON-based.
- No SQLite storage yet.
- No LLM integration yet.
- No automatic semantic summary of Git diffs yet.
- HTML graph layout is simple and static.
- No published public NuGet package yet.
- GitHub releases are not automated yet.

---

## Roadmap

Planned improvements after the first portfolio-ready release:

- Add screenshots and visual examples to the README.
- Improve CLI output further with optional colors and tables.
- Evaluate `System.CommandLine` or `Spectre.Console` for more robust CLI parsing and rendering.
- Add Git diff summaries for `learn-from-git`.
- Add optional SQLite storage.
- Add optional AI provider integration.
- Improve HTML graph layout and filtering.
- Add automated release workflow.
- Publish as a public or private NuGet tool package.

---

## Privacy

DevMemory is local-first.

By default:

- no cloud sync;
- no external API calls;
- no LLM provider required;
- no data leaves the machine.

All memories are stored locally under:

```text
~/.devmemory
```

or under the directory configured with:

```bash
DEVMEMORY_HOME
```

---

## Project goal

DevMemory aims to become a personal engineering knowledge base for developers who want to preserve context across tasks, branches, code reviews and AI-assisted development sessions.

The long-term goal is to provide a local, searchable and extensible memory layer for everyday software engineering work.

---

## License

License not defined yet.
