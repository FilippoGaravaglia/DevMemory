# DevMemory release process

This document describes the release process for DevMemory.

DevMemory uses a simple release workflow:

```text
feature/* -> dev -> main -> tag -> GitHub Release
```

The `main` branch represents the latest stable public release.

The `dev` branch is the integration branch for the next release.

Feature branches are merged into `dev` after local validation and CI checks.

---

## Branch model

```text
main      stable public branch, aligned with released versions
dev       integration branch for upcoming work
feature/* individual feature branches
```

Rules:

- do not develop directly on `main`;
- do not merge incomplete work into `main`;
- merge feature branches into `dev`;
- merge `dev` into `main` only when preparing a stable release;
- create release tags only from `main`.

---

## Feature development flow

Start from `dev`:

```bash
git checkout dev
git pull origin dev
git checkout -b feature/my-feature
```

After implementing the change, run:

```bash
dotnet format DevMemory.slnx
dotnet format DevMemory.slnx --verify-no-changes
./scripts/build-test.sh
```

For release-sensitive changes, also run:

```bash
./scripts/release-check.sh
```

Commit and push:

```bash
git status --short
git add -A
git commit -m "Describe the change"
git push -u origin feature/my-feature
```

When CI is green, merge into `dev`:

```bash
git checkout dev
git pull origin dev
git merge --no-ff feature/my-feature
git push origin dev
```

Delete the feature branch:

```bash
git branch -d feature/my-feature
git push origin --delete feature/my-feature
```

---

## Release preparation checklist

Before releasing a new version, make sure `dev` is stable:

```bash
git checkout dev
git pull origin dev
dotnet format DevMemory.slnx
dotnet format DevMemory.slnx --verify-no-changes
./scripts/build-test.sh
./scripts/release-check.sh
```

Then review:

- `README.md`;
- `CHANGELOG.md`;
- `docs/demo.md`;
- package metadata;
- screenshots, if changed;
- release notes draft;
- current version number;
- GitHub Actions status.

---

## Version update

Update the project version in the CLI package project file.

Current package project:

```text
src/DevMemory.Cli/DevMemory.Cli.csproj
```

Update the version to the new release number.

Example:

```xml
<Version>0.3.0</Version>
```

Then update `CHANGELOG.md` with a new release entry.

Example:

```markdown
## [0.3.0] - YYYY-MM-DD

### Added

- Added first-run setup guidance.
- Added repository contribution and support documentation.
- Added Dependabot configuration.
- Added repository support file verification in release checks.

### Changed

- CI now runs on `dev` and `feature/*` branches.

### Fixed

- ...
```

Run:

```bash
./scripts/release-check.sh
```

The release check validates:

1. build and tests;
2. repository hygiene;
3. repository support files;
4. changelog entry;
5. version consistency;
6. package artifact structure;
7. CLI package smoke test;
8. final package checksum.

---

## Merge `dev` into `main`

When `dev` is ready:

```bash
git checkout main
git pull origin main
git merge --no-ff dev
./scripts/release-check.sh
git push origin main
```

After pushing to `main`, wait for GitHub Actions CI to complete successfully.

---

## Create the release tag

Create an annotated tag from `main`:

```bash
git tag -a v0.3.0 -m "Release v0.3.0"
git push origin v0.3.0
```

Verify the remote tag:

```bash
git ls-remote --tags origin | grep v0.3.0
```

Verify the local tag points to the expected commit:

```bash
git show v0.3.0 --stat
```

---

## Generate release artifacts locally

Run:

```bash
./scripts/pack-release.sh
```

Expected artifacts:

```text
artifacts/packages/DevMemory.Cli.<version>.nupkg
artifacts/packages/DevMemory.Cli.<version>.nupkg.sha256
```

Verify the checksum:

```bash
shasum -a 256 -c artifacts/packages/DevMemory.Cli.<version>.nupkg.sha256
```

---

## Create the GitHub Release

On GitHub:

1. Go to the repository.
2. Open **Releases**.
3. Click **Draft a new release**.
4. Select the tag, for example `v0.3.0`.
5. Set the release title, for example:

```text
DevMemory v0.3.0
```

6. Paste the release notes from `CHANGELOG.md`.
7. Attach:

```text
DevMemory.Cli.<version>.nupkg
DevMemory.Cli.<version>.nupkg.sha256
```

8. Mark it as the latest release.
9. Publish the release.

---

## Post-release verification

After publishing the release:

```bash
git checkout main
git pull origin main
git tag --list "v0.3.0"
git ls-remote --tags origin | grep v0.3.0
./scripts/release-check.sh
```

Verify on GitHub:

- the release is visible;
- the package artifact is attached;
- the checksum artifact is attached;
- the README is rendered correctly;
- the CI badge is green;
- the latest release points to the expected version.

---

## Continue development after release

After the release is published, continue from `dev`.

Bring `dev` in sync with `main` if needed:

```bash
git checkout dev
git pull origin dev
git merge --no-ff main
git push origin dev
```

Then create the next feature branch:

```bash
git checkout -b feature/next-feature
```

---

## Release principles

A DevMemory release should be:

- reproducible;
- validated locally;
- validated by CI;
- documented in `CHANGELOG.md`;
- tagged from `main`;
- published with package and checksum artifacts;
- consistent with the README;
- safe for users to clone, inspect and try.