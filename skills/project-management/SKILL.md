---
name: project-management
description: >-
  Plan features, create user stories, define acceptance criteria, and break work
  into actionable tasks. Use when: planning a new feature, writing user stories,
  creating sprint tasks, defining acceptance criteria, estimating effort, or
  organizing a product backlog.
argument-hint: 'Describe the feature or epic you want to plan.'
---

# Project Management — Feature Planning & User Stories

## When to Use

- Planning a new feature or epic from requirements
- Writing user stories with acceptance criteria
- Breaking an epic into implementable tasks (frontend, backend, tests, docs)
- Estimating effort and defining a Definition of Done
- Organizing backlog items with priorities and dependencies

## Official Documentation

- [User Stories (Atlassian)](https://www.atlassian.com/agile/project-management/user-stories)
- [Acceptance Criteria (ProductPlan)](https://www.productplan.com/glossary/acceptance-criteria/)
- [INVEST Principle](https://www.agilealliance.org/glossary/invest/)
- [GitHub Issues & Projects](https://docs.github.com/en/issues)
- [Definition of Done (Scrum.org)](https://www.scrum.org/resources/what-definition-done)

## Procedure

1. Gather requirements — clarify scope, stakeholders, constraints
2. Write the **epic** description with business goal — see [epic template](./references/templates.md#epic-template)
3. Break the epic into **user stories** following INVEST — see [story template](./references/templates.md#user-story-template)
4. Define **acceptance criteria** for each story (Given/When/Then format)
5. Decompose each story into **tasks** — see [task breakdown](./references/task-breakdown.md)
6. Estimate using T-shirt sizes or story points
7. Identify dependencies, risks, and blockers
8. Review the [sample feature plan](./samples/feature-plan-sample.md)
9. Create GitHub Issues or project board cards from the plan

## Definition of Done (Default)

A feature is **done** when all of the following are met:

- [ ] All user stories have passing acceptance tests
- [ ] Backend API endpoints implemented with tests
- [ ] Frontend UI implemented with tests
- [ ] Accessibility verified (WCAG 2.1 AA)
- [ ] Responsive on mobile, tablet, desktop
- [ ] API documentation updated (Swagger/OpenAPI)
- [ ] Code reviewed and approved
- [ ] No new lint/build warnings
- [ ] Deployed to staging and smoke-tested

## Story Point Reference

| Points | Complexity | Example |
|--------|-----------|---------|
| 1 | Trivial | Fix a typo, update a label |
| 2 | Simple | Add a new field to an existing form |
| 3 | Moderate | New CRUD endpoint with validation |
| 5 | Complex | New feature with frontend + backend + tests |
| 8 | Large | Multi-step workflow with state management |
| 13 | Epic-sized | Break this down further |
