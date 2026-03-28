---
description: "Use when authoring or editing apm.yml manifests, managing APM dependencies, or configuring APM compilation. Covers manifest schema, dependency syntax, lock file conventions, and CLI commands."
applyTo: "apm.yml,apm.lock.yaml,.apm/**,apm_modules/**"
---
# APM (Agent Package Manager) Guidelines

## Manifest (`apm.yml`)

- `name` and `version` are REQUIRED; all other fields are OPTIONAL.
- `version` MUST follow semver (`^\d+\.\d+\.\d+`); pre-release suffixes allowed.
- Use `target: all` for multi-tool support (Copilot + Claude + Cursor + OpenCode).
- Use `type: hybrid` when packages contain both instructions and skills.

## Dependencies

- Prefer pinned tags (`owner/repo#v1.0.0`) over branch refs for reproducibility.
- Use string shorthand for GitHub repos; object form for non-GitHub hosts or subdirectories.
- Strip `github.com` from canonical entries; preserve all other FQDNs.
- Self-defined MCP servers (`registry: false`) MUST declare `transport` and either `command` (stdio) or `url` (http/sse).
- Use `devDependencies` for test helpers and lint rules excluded from plugin bundles.

## Lock File (`apm.lock.yaml`)

- Always commit `apm.lock.yaml` to version control.
- Never manually edit — APM is the sole writer.
- All `deployed_files` paths use forward slashes regardless of OS.
- Add `apm_modules/` to `.gitignore`.

## Compilation

- Run `apm compile` after installing to generate `AGENTS.md` / `CLAUDE.md`.
- Use `compilation.exclude` to skip directories like `apm_modules/**`, `tmp/**`.
- Use `compilation.strategy: distributed` for per-directory `AGENTS.md` files in monorepos.
- Use `--watch` during development for auto-recompilation.

## Security

- Run `apm audit` before accepting new dependencies.
- Use `apm audit --ci` in CI pipelines for lockfile consistency gates.
- Critical Unicode findings (tag chars, bidi overrides) block deployment by default.
- Use `apm audit --strip` to remove dangerous characters while preserving emoji.

## Package Authoring

- Place primitives under `.apm/` directory: `instructions/`, `prompts/`, `skills/`, `agents/`, `hooks/`.
- Use `.instructions.md` suffix for coding standards with `applyTo` frontmatter.
- Use `.prompt.md` suffix for slash commands with `description` frontmatter.
- Publish by pushing to any git remote — no registry required.
