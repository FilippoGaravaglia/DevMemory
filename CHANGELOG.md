# Changelog

All notable changes to DevMemory will be documented in this file.

The format is inspired by Keep a Changelog, and the project follows Semantic Versioning once public releases begin.

---

## [0.1.3] - 2026-05-10

### Added

- Added GitHub Actions CI pipeline for build, test and package validation.
- Added formatting verification with `dotnet format`.
- Added repository code quality baseline with `.editorconfig`, `.gitattributes` and `Directory.Build.props`.
- Added local release packaging script.
- Added CLI `version` command.
- Added global CLI aliases:
  - `devmemory --version`
  - `devmemory -v`
  - `devmemory --help`
  - `devmemory -h`
- Added end-to-end smoke test script for the installed global tool.
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

### Notes

- This is the first portfolio-ready local release candidate.
- DevMemory can be packaged and installed locally as a .NET global tool.
- The package is not published to NuGet yet.
- Storage is currently JSON-based.
- CLI parsing is currently implemented manually.

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