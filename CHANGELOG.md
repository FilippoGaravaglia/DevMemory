# Changelog

All notable changes to DevMemory will be documented in this file.

The format is inspired by Keep a Changelog, and the project follows Semantic Versioning once public releases begin.

---

## [0.2.0] - 2026-05-31

### Added

- Added persistent AI/RAG configuration through `devmemory config show`, `devmemory config set <key> <value>`, and `devmemory config reset`.
- Added `devmemory edit <memory-id> [options]` to update existing memories without manually editing the JSON storage file.
- Added `devmemory related <memory-id>` to find indexed memories semantically related to another memory.
- Added `devmemory timeline` to explore saved memories chronologically with optional project, area, tag, and limit filters.
- Added `devmemory doctor` for general local health checks covering storage, Markdown, configuration, AI runtime, and Git availability.
- Added `scripts/demo-local.sh` to run an isolated local demo without touching the real `~/.devmemory` directory.
- Added `docs/demo.md` with a complete guide for the isolated local demo.
- Added `docs/releases/v0.2.0.md` as a draft release note and publication preparation document.

### Changed

- Improved the memory lifecycle so DevMemory now supports add, list, show, search, edit, and delete workflows.
- Improved `devmemory delete` so it removes the primary JSON memory, the derived Markdown export, and the Qdrant vector point when a vector store is configured.
- Improved CLI help output with the new edit, delete, related, timeline, doctor, and configuration commands.
- Improved AI/RAG usability by allowing persistent local configuration instead of requiring environment variables for every command.
- Improved demo and onboarding readiness for GitHub visitors, reviewers, and future contributors.

### Fixed

- Fixed the memory delete lifecycle so derived Markdown exports are cleaned up when a memory is removed.
- Fixed vector index cleanup behavior by deleting the related Qdrant point when possible.
- Fixed CLI test reliability by serializing CLI tests that capture global console streams.

### Quality

- Expanded test coverage across CLI, application, and infrastructure layers.
- Added tests for memory editing, memory deletion, Markdown cleanup, Qdrant deletion, related memories, timeline output, general diagnostics, and persistent AI configuration.
- Kept `dotnet format`, `dotnet format --verify-no-changes`, shell script validation, build, and test checks passing.
- Maintained local-first behavior with JSON as the primary source of truth and Markdown/Qdrant as derived artifacts.

### Notes

- This release keeps DevMemory local-first.
- JSON remains the primary storage mechanism.
- Qdrant remains a derived vector index.
- Ollama and Qdrant are optional and only required for semantic search, related memories, and RAG features.
- Public NuGet publishing is still not configured.

---

## [0.1.3] - 2026-05-29

### Added

- Added GitHub Actions CI pipeline aligned with the shared local release-check workflow.
- Added formatting verification with `dotnet format`.
- Added repository code quality baseline with `.editorconfig`, `.gitattributes` and `Directory.Build.props`.
- Added local release packaging workflow through `pack-release.sh`.
- Added CLI `version` command.
- Added global CLI aliases:
  - `devmemory --version`
  - `devmemory -v`
  - `devmemory --help`
  - `devmemory -h`
- Added installed CLI verification script.
- Added end-to-end smoke test script for the installed global tool.
- Added release-check pipeline with formatting, build, tests, repository hygiene, changelog verification, version consistency, package verification, package smoke test, and final checksum generation.
- Added NuGet package artifact verification for the DevMemory CLI tool.
- Added SHA-256 checksum generation and validation for release artifacts.
- Added repository hygiene verification to prevent generated artifacts from being tracked.
- Added local data backup and restore scripts.
- Added local AI setup, diagnostics, and smoke-test scripts.
- Added vector indexing dry-run, scoped filters, text preview, and indexable text limits.
- Added README sections for demo, quality practices and release status.
- Added improved local development workflow documentation.

### Changed

- Improved CLI output formatting for `list`, `search` and `show`.
- Improved README presentation for portfolio release.
- Updated local build script to verify formatting before build and tests.
- Cleaned up CLI analyzer warnings.
- Cleaned up Markdown exporter analyzer warnings.
- Improved command dispatching behavior for help and version aliases.
- Improved release packaging metadata for the CLI package.
- Aligned CI and local release packaging around the shared release-check pipeline.
- Aligned `pack-release.sh` with the final release-check workflow.
- Improved release safety by generating the final checksum after the CLI package smoke test.

### Notes

- This is the first portfolio-ready local release candidate.
- DevMemory can be packaged and installed locally as a .NET global tool.
- The package is not published to NuGet yet.
- Storage is currently JSON-based.
- CLI parsing is currently implemented manually.
- Local AI end-to-end validation still requires Ollama, Qdrant, and at least one local memory available for indexing.

---

## [0.1.0] - 2026-05-10

### Added

- Added structured local task memories.
- Added local JSON storage under `~/.devmemory`.
- Added configurable storage directory through `DEVMEMORY_HOME`.
- Added resilient JSON persistence with temporary file and backup handling.
- Added automatic Markdown export for saved memories.
- Added ranked memory search with project, area and tag filters.
- Added Git repository inspection.
- Added memory creation from Git context.
- Added knowledge graph JSON export.
- Added local HTML knowledge graph view.
- Added generated file filtering for Git inspection and graph generation.
- Added CLI command dispatcher and dedicated command handlers.
- Added local development scripts for build, test, packaging and cleanup.
- Added CI workflow for build, test and package validation.
- Added unit tests for application, infrastructure and CLI behavior.

### Changed

- Refactored the CLI entry point into a minimal composition root.
- Improved project structure with dedicated command handlers.
- Improved README documentation for GitHub presentation.

### Notes

- This was the first structured development version of DevMemory.
- The package was not published publicly.
- Storage was JSON-based.
- CLI parsing was implemented manually.