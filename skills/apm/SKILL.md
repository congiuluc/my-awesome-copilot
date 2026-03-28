---
name: apm
description: >-
  Create and manage APM (Agent Package Manager) projects — manifest authoring,
  dependency management, compilation, packaging, and distribution of AI agent
  configuration. Use when: initializing apm.yml, adding APM or MCP dependencies,
  compiling AGENTS.md/CLAUDE.md, building portable bundles, publishing packages,
  or auditing supply chain security.
argument-hint: 'Describe the APM task (e.g., init project, add dependency, compile, pack).'
---

# Agent Package Manager (APM)

## When to Use

- Initializing a new APM project (`apm init`)
- Authoring or editing `apm.yml` manifests
- Adding, updating, or removing APM and MCP dependencies
- Compiling primitives into `AGENTS.md` or `CLAUDE.md`
- Packaging and distributing agent configuration bundles
- Auditing installed packages for hidden Unicode / supply-chain threats
- Publishing reusable packages of instructions, skills, prompts, agents, or hooks
- Migrating an existing project to APM

## Official Documentation

- [What is APM?](https://microsoft.github.io/apm/introduction/what-is-apm/)
- [Quick Start](https://microsoft.github.io/apm/getting-started/quick-start/)
- [Your First Package](https://microsoft.github.io/apm/getting-started/first-package/)
- [Manifest Schema](https://microsoft.github.io/apm/reference/manifest-schema/)
- [Lock File Specification](https://microsoft.github.io/apm/reference/lockfile-spec/)
- [CLI Commands](https://microsoft.github.io/apm/reference/cli-commands/)
- [Primitive Types](https://microsoft.github.io/apm/reference/primitive-types/)
- [Skills Guide](https://microsoft.github.io/apm/guides/skills/)
- [Prompts Guide](https://microsoft.github.io/apm/guides/prompts/)
- [Plugins Guide](https://microsoft.github.io/apm/guides/plugins/)
- [Pack & Distribute](https://microsoft.github.io/apm/guides/pack-distribute/)
- [CI/CD Pipelines](https://microsoft.github.io/apm/integrations/ci-cd/)
- [GitHub Repository](https://github.com/microsoft/apm)

## Procedure

### Step 1 — Install APM

Windows (PowerShell):

```powershell
irm https://aka.ms/apm-windows | iex
```

macOS / Linux:

```bash
curl -sSL https://aka.ms/apm-unix | sh
```

Alternative installs:

```bash
# Homebrew (macOS/Linux)
brew install microsoft/apm/apm

# Scoop (Windows)
scoop bucket add apm https://github.com/microsoft/scoop-apm
scoop install apm

# pip (requires Python 3.10+)
pip install apm-cli
```

Verify:

```bash
apm --version
```

---

### Step 2 — Initialize a Project

Create a new project or initialize inside an existing repo:

```bash
# New directory
apm init my-project && cd my-project

# Existing repo (interactive)
apm init

# Non-interactive with auto-detected defaults
apm init --yes

# Plugin authoring project
apm init my-plugin --plugin
```

This creates `apm.yml` — the dependency manifest for AI agent configuration:

```yaml
name: my-project
version: 1.0.0
dependencies:
  apm: []
```

Auto-detected fields: `name` from directory, `author` from `git config user.name`,
`version` defaults to `1.0.0`.

---

### Step 3 — Understand the Manifest (`apm.yml`)

A conforming manifest is a YAML 1.2 document with the following top-level fields:

```yaml
name:          <string>                  # REQUIRED — package identifier
version:       <string>                  # REQUIRED — semver (^\d+\.\d+\.\d+)
description:   <string>                  # OPTIONAL
author:        <string>                  # OPTIONAL
license:       <string>                  # OPTIONAL — SPDX identifier
target:        <enum>                    # OPTIONAL — vscode | agents | claude | all
type:          <enum>                    # OPTIONAL — instructions | skill | hybrid | prompts
scripts:       <map<string, string>>     # OPTIONAL — named commands via `apm run`
dependencies:
  apm:         <list<ApmDependency>>     # OPTIONAL
  mcp:         <list<McpDependency>>     # OPTIONAL
devDependencies:
  apm:         <list<ApmDependency>>     # OPTIONAL
  mcp:         <list<McpDependency>>     # OPTIONAL
compilation:   <CompilationConfig>       # OPTIONAL
```

#### `target` Enum

| Value     | Output                                                     |
|-----------|------------------------------------------------------------|
| `vscode`  | Emits `AGENTS.md` at project root (+ per-directory files)  |
| `agents`  | Alias for `vscode`                                         |
| `claude`  | Emits `CLAUDE.md` at project root                          |
| `all`     | Both `vscode` and `claude` targets                         |

Auto-detected when unset: `vscode` if `.github/` exists, `claude` if `.claude/` exists,
`all` if both, `minimal` if neither.

#### `type` Enum

| Value          | Behaviour                                          |
|----------------|----------------------------------------------------|
| `instructions` | Compiled into `AGENTS.md` only                     |
| `skill`        | Installed as a native skill only                   |
| `hybrid`       | Both `AGENTS.md` compilation and skill installation|
| `prompts`      | Commands/prompts only                              |

---

### Step 4 — Declare Dependencies

#### APM Dependencies (`dependencies.apm`)

Each element is either a string shorthand or an object.

**String form:**

```yaml
dependencies:
  apm:
    # GitHub shorthand (default host)
    - microsoft/apm-sample-package                # latest
    - microsoft/apm-sample-package#v1.0.0         # pinned to tag
    - microsoft/apm-sample-package#main           # branch ref

    # Non-GitHub hosts (FQDN preserved)
    - gitlab.com/acme/coding-standards
    - dev.azure.com/org/project/_git/repo

    # Virtual packages (subdirectory or single file)
    - ComposioHQ/awesome-claude-skills/brand-guidelines
    - contoso/prompts/review.prompt.md

    # Local path (development only)
    - ./packages/my-shared-skills
```

**Object form** (required when shorthand is ambiguous):

```yaml
dependencies:
  apm:
    - git: https://gitlab.com/acme/repo.git
      path: instructions/security
      ref: v2.0
      alias: acme-sec

    # Local path dependency
    - path: ./packages/my-shared-skills
```

Object fields: `git` (clone URL), `path` (subdirectory or local path),
`ref` (branch/tag/SHA), `alias` (local name).

**Virtual package classification rules:**

| Type            | Condition                                       | Example                                      |
|-----------------|-------------------------------------------------|----------------------------------------------|
| File            | Ends in `.prompt.md`, `.instructions.md`, etc.  | `owner/repo/prompts/review.prompt.md`        |
| Collection dir  | Contains `/collections/`                        | `owner/repo/collections/security`            |
| Collection file | Contains `/collections/` + `.collection.yml`    | `owner/repo/collections/sec.collection.yml`  |
| Subdirectory    | None of the above                               | `owner/repo/skills/security`                 |

**Canonical normalisation:** `github.com` is default host and MUST be stripped;
all other hosts are preserved as FQDN.

#### MCP Dependencies (`dependencies.mcp`)

**String form** — registry reference:

```yaml
dependencies:
  mcp:
    - io.github.github/github-mcp-server
```

**Object form** — registry with overlays or self-defined:

```yaml
dependencies:
  mcp:
    # Registry with overlays
    - name: io.github.github/github-mcp-server
      tools: ["repos", "issues"]
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}

    # Self-defined server (registry: false)
    - name: my-private-server
      registry: false
      transport: stdio
      command: ./bin/my-server
      args: ["--port", "3000"]
      env:
        API_KEY: ${{ secrets.KEY }}
```

Object fields: `name`, `transport` (`stdio | sse | http | streamable-http`),
`env`, `args`, `version`, `registry` (bool or string), `package` (`npm | pypi | oci`),
`headers`, `tools` (default `["*"]`), `url`, `command`.

**Validation for self-defined servers (`registry: false`):**

1. `transport` MUST be present.
2. If `transport` is `stdio`, `command` MUST be present.
3. If `transport` is `http`/`sse`/`streamable-http`, `url` MUST be present.

#### Dev Dependencies

Same structure as `dependencies`, placed under `devDependencies`. Installed locally
but excluded from `apm pack --format plugin` bundles.

```yaml
devDependencies:
  apm:
    - owner/test-helpers
    - owner/lint-rules#v2.0.0
```

Add with `apm install --dev owner/test-helpers`.

---

### Step 5 — Install Dependencies

```bash
# Install all from apm.yml
apm install

# Install specific package (adds to apm.yml automatically)
apm install microsoft/apm-sample-package#v1.0.0

# Install from any git host
apm install gitlab.com/org/repo
apm install dev.azure.com/org/project/_git/repo

# Install only APM or only MCP
apm install --only=apm
apm install --only=mcp

# Update to latest versions
apm install --update

# Preview without installing
apm install --dry-run

# Install as dev dependency
apm install --dev owner/test-helpers
```

**What happens on install:**

1. Packages downloaded to `apm_modules/` (like `node_modules/`).
2. Primitives deployed to native directories:
   - `.github/instructions/`, `.github/prompts/`, `.github/agents/`, `.github/skills/`
   - `.claude/commands/`, `.claude/agents/`, `.claude/skills/`
   - `.cursor/rules/`, `.cursor/agents/`, `.cursor/skills/`
   - `.opencode/agents/`, `.opencode/commands/`, `.opencode/skills/`
3. `apm.lock.yaml` written — pinning exact commit SHAs.

**Add `apm_modules/` to `.gitignore`.** Commit `apm.yml`, `apm.lock.yaml`, and
deployed `.github/`, `.claude/`, `.cursor/` files.

---

### Step 6 — Compilation

Compile primitives into optimized output files:

```bash
# Auto-detect targets and compile
apm compile

# Target specific format
apm compile --target vscode    # AGENTS.md + .github/
apm compile --target claude    # CLAUDE.md + .claude/
apm compile --target all       # All formats

# Watch mode (auto-recompile on changes)
apm compile --watch

# Preview without writing
apm compile --dry-run

# Validate primitives without compiling
apm compile --validate
```

**Compilation config in `apm.yml`:**

```yaml
compilation:
  target: all
  strategy: distributed        # distributed | single-file
  source_attribution: true
  resolve_links: true
  exclude:
    - "apm_modules/**"
    - "tmp/**"
  placement:
    min_instructions_per_file: 1
```

| Setting                 | Default       | Description                                     |
|-------------------------|---------------|-------------------------------------------------|
| `target`                | auto-detect   | `vscode`, `claude`, `all`                       |
| `strategy`              | `distributed` | `distributed` (per-dir) or `single-file`        |
| `source_attribution`    | `true`        | Include source-file origin comments              |
| `resolve_links`         | `true`        | Resolve relative Markdown links                  |
| `exclude`               | `[]`          | Glob patterns to skip during compilation         |

---

### Step 7 — Package Structure

APM manages **seven primitive types**:

| Primitive    | Description                     | File Convention            | Deploy Location                     |
|--------------|---------------------------------|----------------------------|-------------------------------------|
| Instructions | Coding standards and guardrails | `.instructions.md`         | `.github/instructions/`             |
| Skills       | Reusable AI capabilities        | `SKILL.md` in folder       | `.github/skills/{name}/`            |
| Prompts      | Slash commands                  | `.prompt.md`               | `.github/prompts/`                  |
| Agents       | Specialized AI personas         | `.agent.md`                | `.github/agents/`                   |
| Hooks        | Lifecycle event handlers        | `.json` in `hooks/`        | `.github/hooks/`                    |
| Plugins      | Pre-packaged agent bundles      | `plugin.json`              | Varies                              |
| MCP Servers  | External tool integrations      | Declared in `apm.yml`      | `.vscode/mcp.json` / IDE config     |

**Standard package directory layout:**

```
my-package/
├── apm.yml                  # Package manifest
└── .apm/
    ├── instructions/        # .instructions.md files
    ├── prompts/             # .prompt.md files
    ├── skills/              # Folders with SKILL.md
    ├── agents/              # .agent.md files
    └── hooks/               # .json hook definitions
```

---

### Step 8 — Lock File (`apm.lock.yaml`)

The lock file records the exact resolved state of every dependency. It is
analogous to `package-lock.json` (npm) or `.terraform.lock.hcl`.

**Structure:**

```yaml
lockfile_version: "1"
generated_at: "2026-03-09T14:00:00Z"
apm_version: "0.7.7"

dependencies:
  - repo_url: https://github.com/acme-corp/security-baseline
    resolved_commit: a1b2c3d4e5f6789012345678901234567890abcd
    resolved_ref: v2.1.0
    version: "2.1.0"
    depth: 1
    package_type: apm_package
    content_hash: "sha256:9f86d081..."
    deployed_files:
      - .github/instructions/security.instructions.md
      - .github/agents/security-auditor.agent.md

mcp_servers:
  - security-scanner
```

**Key fields per dependency entry:**

| Field              | Required   | Description                                   |
|--------------------|------------|-----------------------------------------------|
| `repo_url`         | MUST       | Source repository URL                         |
| `resolved_commit`  | MUST (remote) | Full 40-char commit SHA                    |
| `resolved_ref`     | MUST (remote) | Git ref that resolved to commit            |
| `depth`            | MUST       | 1 = direct, 2+ = transitive                  |
| `package_type`     | MUST       | `apm_package`, `plugin`, `virtual`, etc.      |
| `deployed_files`   | MUST       | Workspace-relative paths of installed files   |
| `content_hash`     | MAY        | SHA-256 of package file tree                  |
| `is_dev`           | MAY        | `true` for devDependencies                    |

**Resolver behaviour:**

1. First install — resolve all refs, write lock file.
2. Subsequent installs — reuse locked commits. New deps appended.
3. `--update` — re-resolve all refs, overwrite lock file.

**All `deployed_files` paths use forward slashes.** The lock file MUST be
committed to version control. It MUST NOT be manually edited.

---

### Step 9 — Pack & Distribute

Create portable bundles for offline or CI distribution:

```bash
# Pack to ./build/<name>-<version>/
apm pack

# Pack as .tar.gz archive
apm pack --archive

# Pack only VS Code files
apm pack --target vscode

# Export as standalone plugin
apm pack --format plugin

# Preview what would be packed
apm pack --dry-run
```

Unpack in another project:

```bash
apm unpack ./build/my-pkg-1.0.0.tar.gz
apm unpack bundle.tar.gz --output /path/to/project
```

---

### Step 10 — Security & Auditing

APM scans packages for hidden Unicode characters (supply chain attacks):

```bash
# Scan all installed packages
apm audit

# Scan a specific package
apm audit https://github.com/owner/repo

# Scan any file
apm audit --file .cursorrules

# Remove dangerous characters (preserves emoji)
apm audit --strip

# CI lockfile consistency gate
apm audit --ci

# CI gate with org policy checks
apm audit --ci --policy org

# SARIF output for GitHub Code Scanning
apm audit -f sarif -o report.sarif
```

**Threat detection tiers:**

| Severity | Characters                                                    |
|----------|---------------------------------------------------------------|
| Critical | Tag chars (U+E0001–E007F), bidi overrides, variation sel 17–256 |
| Warning  | Zero-width spaces/joiners, bidi marks, invisible operators    |
| Info     | Non-breaking spaces, unusual whitespace, emoji presentation   |

---

### Step 11 — Manage Dependencies

```bash
# List installed packages with primitive counts
apm deps list

# Show dependency tree
apm deps tree

# Show detailed info for a package
apm deps info compliance-rules

# Update all or specific dependencies
apm deps update
apm deps update compliance-rules

# Remove a package and its deployed files
apm uninstall microsoft/apm-sample-package

# Remove orphaned packages
apm prune

# Remove all dependencies
apm deps clean
```

---

### Step 12 — Publish a Package

Any git repository is a valid APM package. Publishing = pushing to a git remote:

```bash
git init
git add .
git commit -m "Initial APM package"
git remote add origin https://github.com/you/my-package.git
git push -u origin main
```

**Consumers install with:**

```bash
apm install you/my-package
# or pin a version
apm install you/my-package#v1.0.0
```

---

## Complete Manifest Example

```yaml
name: my-project
version: 1.0.0
description: AI-native web application
author: Contoso
license: MIT
target: all
type: hybrid

scripts:
  review: "copilot -p 'code-review.prompt.md'"
  impl:   "copilot -p 'implement-feature.prompt.md'"

dependencies:
  apm:
    - microsoft/apm-sample-package#v1.0.0
    - gitlab.com/acme/coding-standards#main
    - git: https://gitlab.com/acme/repo.git
      path: instructions/security
      ref: v2.0
  mcp:
    - io.github.github/github-mcp-server
    - name: my-private-server
      registry: false
      transport: stdio
      command: ./bin/my-server
      env:
        API_KEY: ${{ secrets.KEY }}

devDependencies:
  apm:
    - owner/test-helpers

compilation:
  target: all
  strategy: distributed
  exclude:
    - "apm_modules/**"
  placement:
    min_instructions_per_file: 1
```

## Day-to-Day Workflow Summary

```bash
# New developer joins
git clone <org/repo> && cd <repo> && apm install

# Add a new dependency
apm install owner/package#v1.0.0

# Update dependencies
apm install --update

# Compile for Codex / Gemini
apm compile

# Audit supply chain
apm audit

# Package for distribution
apm pack --archive

# Check for CLI updates
apm update --check
```

## Constraints & Rules

- `name` and `version` are the only REQUIRED fields in `apm.yml`.
- `version` MUST follow semver (`^\d+\.\d+\.\d+`); pre-release/build suffixes allowed.
- `github.com` is the default host — strip from canonical entries; preserve all other FQDNs.
- Lock files use forward slashes for all paths regardless of OS.
- `apm.lock.yaml` MUST be committed; `apm_modules/` MUST be in `.gitignore`.
- Hidden Unicode scanning runs automatically on `apm install`, `apm compile`, and `apm pack`.
- Dev dependencies (`devDependencies`) are excluded from `apm pack --format plugin` bundles.
- Unknown keys in `apm.yml` MUST be preserved on read/write for forward compatibility.
- Local primitives always override dependency primitives (priority system).
- Dependencies are processed in declaration order; first declared wins conflicts.
