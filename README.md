# My Awesome Copilot

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub Copilot](https://img.shields.io/badge/GitHub%20Copilot-Customization-blue?logo=github)](https://code.visualstudio.com/docs/copilot/copilot-customization)
[![Skills](https://img.shields.io/badge/Skills-35-58a6ff)](skills/)
[![Instructions](https://img.shields.io/badge/Instructions-36-3fb950)](instructions/)
[![Agents](https://img.shields.io/badge/Agents-10-bc8cff)](agents/)
[![Gallery](https://img.shields.io/badge/Gallery-Live-orange)](https://congiuluc.github.io/my-awesome-copilot/)

A curated collection of **skills**, **instructions**, and **agents** for [GitHub Copilot Customization](https://code.visualstudio.com/docs/copilot/copilot-customization) — targeting **.NET**, **React**, and **full-stack** development workflows.

> [**Browse the Gallery →**](https://congiuluc.github.io/my-awesome-copilot/)

---

## Quick Start

**Clone & open in VS Code:**

[![Open in VS Code](https://img.shields.io/badge/Open%20in-VS%20Code-007ACC?logo=visual-studio-code)](vscode://vscode.git/clone?url=https://github.com/congiuluc/my-awesome-copilot.git)
[![Open in VS Code Insiders](https://img.shields.io/badge/Open%20in-VS%20Code%20Insiders-24bfa5?logo=visual-studio-code)](vscode-insiders://vscode.git/clone?url=https://github.com/congiuluc/my-awesome-copilot.git)

Or manually:

```bash
git clone https://github.com/congiuluc/my-awesome-copilot.git
```

Copy the files you need into your project's `.github/` directory to activate them in Copilot.

---

## What's Inside

### 🧩 Skills (35)

Reusable knowledge modules that give Copilot deep expertise in specific domains.

| Category | Skills |
|----------|--------|
| **Backend (.NET)** | Backend .NET, Repository (EF Core), Repository (Dapper), Error Handling, Security, Performance, Testing, API Documentation, Notification, Aspire + OpenTelemetry, Authentication, Logging |
| **Database** | SQLite, SQL Server, Cosmos DB, MongoDB, Database Migration |
| **Frontend (React)** | Frontend React, TailwindCSS Components, Accessibility, Responsive Design, Error Handling, Security, Performance, Testing, Notification, State Management, API Client |
| **DevOps & Planning** | Docker, CI/CD GitHub Actions, Project Management, Feature Testing, MCP Discovery, Version Tracking, VS Code Extension, APM |

### 📋 Instructions (36)

Concise coding guidelines that shape Copilot's output style and patterns. One instruction file per skill area, covering conventions, patterns, and constraints.

### 🤖 Agents (10)

Specialized agent personas for different development roles:

| Agent | Role |
|-------|------|
| **Tech Lead** | Orchestrates full-stack feature delivery by delegating to specialized agents |
| **Backend Creator** | Builds .NET 10 Minimal API features with Clean Architecture |
| **Frontend Creator** | Builds React 19 + TypeScript features with TailwindCSS |
| **Test Writer** | Writes xUnit, Vitest, and Playwright tests |
| **Backend Reviewer** | Code review for .NET backend (architecture, security, performance) |
| **Frontend Reviewer** | Code review for React frontend (a11y, responsiveness, security) |
| **Feature Planner** | Plans features, user stories, acceptance criteria, task breakdown |
| **DevOps** | Docker, CI/CD pipelines, GitHub Actions, Aspire orchestration |
| **Release Manager** | Changelog management, SemVer versioning, release workflow |
| **VS Code Extension Creator** | Build VS Code extensions: commands, tree views, webview panels |

---

## Project Structure

```
my-awesome-copilot/
├── agents/                  # Agent persona definitions (.agent.md)
├── instructions/            # Coding guidelines (.instructions.md)
├── skills/                  # Deep knowledge modules (SKILL.md + references)
│   └── <skill-name>/
│       ├── SKILL.md
│       ├── references/
│       └── samples/
├── docs/
│   └── gallery/
│       └── index.html       # Interactive gallery (GitHub Pages)
└── LICENSE
```

---

## Gallery

The project includes an interactive gallery hosted on GitHub Pages with:

- **Search** — filter by name, description, or tag
- **Filters** — toggle between Skills, Instructions, and Agents
- **Dark/Light theme** — auto-detects system preference
- **Direct links** — each card links to its source on GitHub

**[→ Browse the Gallery](https://congiuluc.github.io/my-awesome-copilot/)**

---

## Tech Stack Coverage

| Layer | Technologies |
|-------|-------------|
| **Backend** | .NET 10, Minimal API, Clean Architecture, Serilog, MediatR, SignalR |
| **Frontend** | React 19, TypeScript, Vite, TailwindCSS v4, Sonner, TanStack Query |
| **Database** | SQLite, SQL Server, Azure Cosmos DB, MongoDB, EF Core, Dapper |
| **Testing** | xUnit, Moq, FluentAssertions, Vitest, React Testing Library, Playwright |
| **DevOps** | Docker, GitHub Actions, .NET Aspire, OpenTelemetry |
| **Security** | OWASP Top 10, JWT/RBAC, CORS, CSP, DOMPurify, FluentValidation |

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/new-skill`)
3. Add your skill, instruction, or agent following the existing conventions
4. Submit a pull request

---

## License

This project is licensed under the [MIT License](LICENSE).

---

<p align="center">
  Built for <a href="https://code.visualstudio.com/docs/copilot/copilot-customization">GitHub Copilot Customization</a>
</p>
