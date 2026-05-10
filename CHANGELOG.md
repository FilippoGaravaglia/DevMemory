# Changelog

All notable changes to DevMemory will be documented in this file.

The format is inspired by Keep a Changelog, and the project follows Semantic Versioning once public releases begin.

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

- This is the first portfolio-ready development version.
- The package is not published publicly yet.
- Storage is currently JSON-based.
- CLI parsing is currently implemented manually.