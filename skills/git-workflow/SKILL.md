---
name: git-workflow
description: "Configure Git branching strategy, PR workflows, hooks, and repository conventions. Use when: setting up a branching model, defining merge policies, creating PR templates, configuring git hooks, generating .gitignore files, or resolving merge conflicts."
argument-hint: 'Describe the Git workflow need (e.g., branching strategy, PR template, git hooks, .gitignore, etc.).'
---

# Git Workflow

## When to Use

- Setting up or changing a branching strategy (GitHub Flow, GitFlow, trunk-based)
- Defining branch naming conventions
- Creating or updating PR templates and review checklists
- Configuring Git hooks (pre-commit, commit-msg)
- Generating or updating `.gitignore` for .NET + React projects
- Establishing merge and rebase policies
- Resolving merge conflicts

## Official Documentation

- [Git Branching](https://git-scm.com/book/en/v2/Git-Branching-Branching-Workflows)
- [GitHub Flow](https://docs.github.com/en/get-started/using-github/github-flow)
- [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
- [Husky (Git Hooks)](https://typicode.github.io/husky/)
- [gitignore Patterns](https://git-scm.com/docs/gitignore)
- [PR Templates](https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/creating-a-pull-request-template-for-your-repository)

## Procedure

1. Choose a branching strategy from [branching strategies](./references/branching-strategies.md)
2. Define branch naming and merge policy from [branch conventions](./references/branch-conventions.md)
3. Set up PR template from [PR workflow](./references/pr-workflow.md)
4. Configure Git hooks from [git hooks](./references/git-hooks.md)
5. Generate `.gitignore` using [sample .gitignore](./samples/.gitignore.sample)
6. Review [sample PR template](./samples/pull-request-template.sample.md)
7. Document chosen strategy in `CONTRIBUTING.md` or project wiki
