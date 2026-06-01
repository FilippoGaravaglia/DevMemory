# Repository contribution and support guidelines

This block contains the files to add for the DevMemory repository contribution/support setup.

Files to create:

```text
CONTRIBUTING.md
SECURITY.md
.github/ISSUE_TEMPLATE/bug_report.md
.github/ISSUE_TEMPLATE/feature_request.md
.github/pull_request_template.md
```

---

# File: CONTRIBUTING.md

````markdown
# Contributing to DevMemory

Thank you for your interest in contributing to DevMemory.

DevMemory is a local-first developer memory CLI built with .NET. The project is currently developed as a personal open-source project, but contributions, feedback, ideas and bug reports are welcome.

The goal is to keep the project pragmatic, local-first, testable and easy to reason about.

---

## Project principles

DevMemory follows a few core principles:

- local-first by default;
- JSON storage as the current source of truth;
- derived artifacts can be regenerated;
- AI/RAG features are optional;
- no cloud dependency for core memory features;
- clear separation between domain, application, infrastructure and CLI layers;
- small, focused changes;
- tests for meaningful behavior;
- documentation updated when user-facing behavior changes.

---

## Repository structure

```text
src/
  DevMemory.Core
  DevMemory.Application
  DevMemory.Infrastructure
  DevMemory.Cli

tests/
  DevMemory.Application.Tests
  DevMemory.Infrastructure.Tests
  DevMemory.Cli.Tests

scripts/
  local development, validation, packaging and release scripts

docs/
  user-facing and demo documentation
````

---

## Branching model

The repository uses this branch model:

```text
main      stable public branch, aligned with released versions
dev       integration branch for upcoming work
feature/* individual feature branches
```

Please do not open pull requests directly against `main`.

Recommended flow:

```bash
git checkout dev
git pull origin dev
git checkout -b feature/my-change
```

When the change is ready, open a pull request into `dev`.

`main` is updated from `dev` only when preparing a new stable release.

---

## Local setup

Prerequisites:

* .NET SDK 10
* Git
* Bash-compatible shell for scripts
* Docker Desktop, only for Qdrant/local vector search
* Ollama, only for local AI/RAG features

Install the local CLI tool from source:

```bash
./scripts/install-local-tool.sh
```

Run first-run setup guidance:

```bash
devmemory setup
```

Run the isolated demo:

```bash
./scripts/demo-local.sh
```

---

## Validation before committing

Before committing changes, run:

```bash
dotnet format DevMemory.slnx
dotnet format DevMemory.slnx --verify-no-changes
./scripts/build-test.sh
```

For release-sensitive changes, also run:

```bash
./scripts/release-check.sh
```

The release check validates build, tests, formatting, repository hygiene, changelog, version consistency, package smoke tests and checksum generation.

---

## Testing expectations

Please add or update tests when changing behavior.

Examples:

* application behavior should be tested in `DevMemory.Application.Tests`;
* infrastructure behavior should be tested in `DevMemory.Infrastructure.Tests`;
* command parsing and CLI behavior should be tested in `DevMemory.Cli.Tests`.

A change that affects a command should usually include CLI tests.

A change that affects generated artifacts should usually include artifact-related tests.

---

## Code style

General guidelines:

* keep changes small and focused;
* prefer clear names over clever abstractions;
* keep CLI handlers thin;
* keep orchestration in the application layer;
* keep technical details in the infrastructure layer;
* avoid unnecessary dependencies;
* preserve local-first behavior;
* avoid hidden network calls in core commands.

For new private helper methods in C#, include an English XML summary comment:

```csharp
/// <summary>
/// Describes what the helper does.
/// </summary>
```

---

## Documentation expectations

Update documentation when changing user-facing behavior.

Examples:

* new commands;
* changed command options;
* changed storage behavior;
* changed AI/RAG configuration;
* new scripts;
* changed release process.

Relevant documentation files include:

```text
README.md
CHANGELOG.md
docs/demo.md
```

---

## Commit messages

Use clear English commit messages.

Examples:

```text
Add first-run setup command
Document setup command and verify package smoke test
Improve graph export validation
Fix markdown cleanup after memory deletion
```

---

## Pull request checklist

Before opening a pull request, check:

* [ ] The branch is based on `dev`.
* [ ] The change is focused and understandable.
* [ ] Tests were added or updated when needed.
* [ ] `dotnet format DevMemory.slnx` was executed.
* [ ] `dotnet format DevMemory.slnx --verify-no-changes` passes.
* [ ] `./scripts/build-test.sh` passes.
* [ ] Documentation was updated if user-facing behavior changed.
* [ ] `./scripts/release-check.sh` was executed for release-sensitive changes.

---

## Reporting issues

When reporting a bug, please include:

* operating system;
* .NET SDK version;
* DevMemory version;
* command executed;
* expected behavior;
* actual behavior;
* relevant logs or terminal output;
* whether `DEVMEMORY_HOME` was customized.

Please avoid sharing private memory contents unless necessary.

````

---

# File: SECURITY.md

```markdown
# Security Policy

## Supported versions

DevMemory is currently a personal open-source project in early development.

The latest GitHub Release is the only supported public release.

| Version | Supported |
| ------- | --------- |
| latest  | Yes       |
| older   | No        |

---

## Reporting a vulnerability

If you find a security issue, please do not open a public issue with sensitive details.

Instead, contact the maintainer privately through GitHub or LinkedIn.

Please include:

- affected version or commit;
- operating system;
- command or workflow involved;
- impact description;
- reproduction steps if possible;
- whether local files, environment variables, AI runtime or external services are involved.

---

## Security model

DevMemory is local-first.

By default:

- primary memory data is stored locally;
- core memory commands do not require cloud services;
- AI/RAG features are optional;
- local AI can run through Ollama;
- vector data can be stored locally through Qdrant.

Default local storage:

```text
~/.devmemory/devmemory.json
````

If `DEVMEMORY_HOME` is set, DevMemory uses that directory instead.

---

## Sensitive data guidance

DevMemory can store technical context written by the user.

Avoid storing:

* secrets;
* passwords;
* API keys;
* private customer data;
* confidential source snippets;
* personal data not needed for technical memory.

Before sharing logs, issues or screenshots, review them for sensitive information.

---

## External services

Core local memory features do not require external services.

Optional AI/RAG features may interact with configured local or external providers depending on environment variables and configuration.

Configuration precedence:

```text
Environment variables > ~/.devmemory/config.json > default values
```

Review your configuration before enabling non-local providers.

---

## Known limitations

Current limitations:

* primary storage is JSON-based;
* no encryption-at-rest is implemented by DevMemory itself;
* access control is delegated to local operating-system permissions;
* local AI/RAG quality and privacy depend on selected providers and configuration.

Use appropriate filesystem permissions and avoid storing secrets in DevMemory memories.

````

---

# File: .github/ISSUE_TEMPLATE/bug_report.md

```markdown
---
name: Bug report
about: Report a reproducible problem in DevMemory
title: "[Bug]: "
labels: bug
assignees: ""
---

## Description

Describe the problem clearly.

## Steps to reproduce

1.
2.
3.

## Expected behavior

What did you expect to happen?

## Actual behavior

What happened instead?

## Command output

```text

````

## Environment

* OS:
* .NET SDK version:
* DevMemory version:
* Installation method:

  * [ ] `dotnet run`
  * [ ] local global tool
  * [ ] other
* Custom `DEVMEMORY_HOME`:

  * [ ] yes
  * [ ] no

## AI/RAG runtime, if relevant

* Ollama running:

  * [ ] yes
  * [ ] no
  * [ ] not relevant
* Qdrant running:

  * [ ] yes
  * [ ] no
  * [ ] not relevant

## Additional context

Add any other relevant information.

Please avoid sharing secrets, private memory contents or confidential project data.

````

---

# File: .github/ISSUE_TEMPLATE/feature_request.md

```markdown
---
name: Feature request
about: Suggest an improvement for DevMemory
title: "[Feature]: "
labels: enhancement
assignees: ""
---

## Problem

What problem would this feature solve?

## Proposed solution

Describe the solution you would like.

## Example usage

If this is a CLI feature, show the desired command or workflow.

```bash

````

## Alternatives considered

Describe any alternatives you considered.

## Scope

Which area does this affect?

* [ ] CLI
* [ ] Storage
* [ ] Markdown export
* [ ] Git integration
* [ ] Knowledge graph
* [ ] Local AI/RAG
* [ ] Packaging/release
* [ ] Documentation
* [ ] Other

## Additional context

Add screenshots, examples or references if useful.

````

---

# File: .github/pull_request_template.md

```markdown
## Summary

Describe the change in a few sentences.

## Type of change

- [ ] Bug fix
- [ ] New feature
- [ ] Refactoring
- [ ] Documentation
- [ ] Tests
- [ ] Build/release
- [ ] Other

## Checklist

- [ ] The branch is based on `dev`.
- [ ] The change is focused and understandable.
- [ ] Tests were added or updated when needed.
- [ ] Documentation was updated if user-facing behavior changed.
- [ ] `dotnet format DevMemory.slnx` was executed.
- [ ] `dotnet format DevMemory.slnx --verify-no-changes` passes.
- [ ] `./scripts/build-test.sh` passes.
- [ ] `./scripts/release-check.sh` was executed for release-sensitive changes.

## Validation

Paste relevant validation output or describe what was tested.

```text

````

## Notes

Add any implementation notes, trade-offs or follow-up work.

```
```
