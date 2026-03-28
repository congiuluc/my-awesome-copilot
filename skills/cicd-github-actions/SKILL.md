---
name: cicd-github-actions
description: "Create GitHub Actions CI/CD workflows for .NET and React projects. Use when: setting up CI pipelines, configuring build/test automation, deployment workflows, caching strategies, or environment protection rules."
argument-hint: 'Describe the pipeline or workflow to create (CI, CD, deploy, etc.).'
---

# CI/CD with GitHub Actions

## When to Use

- Creating or modifying GitHub Actions workflows
- Setting up CI for backend (.NET) or frontend (React/Vite)
- Configuring deployment pipelines for staging/production
- Adding code coverage, linting, or security scanning steps

## Official Documentation

- [GitHub Actions](https://docs.github.com/en/actions)
- [Workflow Syntax](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)
- [Environments & Protection Rules](https://docs.github.com/en/actions/deployment/targeting-different-environments)

## Procedure

1. Follow [workflow patterns](./references/workflow-patterns.md) for CI/CD structure
2. Review [sample CI workflow](./samples/ci-workflow-sample.yml)
3. Backend CI: restore → build → test with coverage
4. Frontend CI: install → lint → build → test
5. Both jobs must pass before merge
6. Pin action versions to major tags (`@v4`)
7. Use GitHub Secrets for sensitive values
8. Add concurrency controls to cancel duplicate runs
