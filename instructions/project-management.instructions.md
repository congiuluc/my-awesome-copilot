---
description: "Use when writing feature plans, user stories, acceptance criteria, or task breakdowns. Covers story format, estimation, backlog organization, and sprint planning."
applyTo: "docs/**,.copilot/plans/**"
---
# Project Management Guidelines

## User Story Format

```
As a [role],
I want [feature/capability],
So that [business value/benefit].
```

- Every story must have **acceptance criteria** — testable conditions for "done".
- Use the Given/When/Then format for acceptance criteria.

## Acceptance Criteria

```
Given [precondition],
When [action],
Then [expected result].
```

- Keep criteria specific and measurable — avoid vague terms like "fast" or "user-friendly".
- Include both happy path and error scenarios.

## Task Breakdown

Break each story into implementable tasks:

| Layer | Example Task |
|-------|-------------|
| Backend | Create `Product` domain model, implement `IProductRepository`, build CRUD endpoints |
| Frontend | Build `ProductList` component, add search/filter, implement create form |
| Tests | Unit tests for service layer, integration tests for API, E2E for user flow |
| Docs | Update API docs, add README section |

## Estimation

- Use **complexity points** (1, 2, 3, 5, 8, 13) — not hours.
- 1 point = trivial change, well-understood.
- 13 points = consider breaking into smaller stories.

## Plan File Structure

Plans live in `.copilot/plans/{feature-slug}.plan.md` with sections:
Epic, User Stories, Task Breakdown, Implementation Order, Technical Decisions, Risks.
