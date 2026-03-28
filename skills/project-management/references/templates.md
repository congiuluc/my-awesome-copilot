# Planning Templates

## Epic Template

```markdown
## Epic: {Epic Title}

**Business Goal:** {Why does this feature exist? What problem does it solve?}

**Target Users:** {Who benefits from this?}

**Success Metrics:**
- {Measurable outcome 1}
- {Measurable outcome 2}

**Scope:**
- In scope: {what's included}
- Out of scope: {what's excluded}

**Dependencies:**
- {Dependency 1}
- {Dependency 2}

**Risks:**
- {Risk 1 — mitigation}
- {Risk 2 — mitigation}
```

## User Story Template

```markdown
### Story: {Title}

**As a** {role/persona},
**I want to** {action/capability},
**So that** {benefit/business value}.

**Acceptance Criteria:**

- [ ] **Given** {precondition}, **When** {action}, **Then** {expected result}
- [ ] **Given** {precondition}, **When** {action}, **Then** {expected result}
- [ ] **Given** {precondition}, **When** {action}, **Then** {expected result}

**Priority:** {High | Medium | Low}
**Estimate:** {1 | 2 | 3 | 5 | 8} points

**Notes:**
- {Technical notes, design decisions, edge cases}
```

## Bug Report Template

```markdown
### Bug: {Title}

**Severity:** {Critical | High | Medium | Low}

**Steps to Reproduce:**
1. {Step 1}
2. {Step 2}
3. {Step 3}

**Expected Behavior:** {What should happen}

**Actual Behavior:** {What happens instead}

**Environment:** {Browser, OS, API version}

**Screenshots/Logs:** {Attach if available}
```

## Technical Task Template

```markdown
### Task: {Title}

**Parent Story:** #{story-number}
**Type:** {Backend | Frontend | Infrastructure | Testing | Documentation}
**Estimate:** {hours or points}

**Description:**
{What needs to be done}

**Acceptance Criteria:**
- [ ] {Criterion 1}
- [ ] {Criterion 2}

**Files to Modify:**
- `{file-path-1}`
- `{file-path-2}`
```

## Sprint Planning Checklist

```markdown
### Sprint {N} Planning

**Sprint Goal:** {One-sentence goal}

**Capacity:** {team-size × days × hours-per-day}

**Selected Stories:**
| # | Story | Points | Assignee | Status |
|---|-------|--------|----------|--------|
| 1 | {title} | {pts} | {name} | Not Started |
| 2 | {title} | {pts} | {name} | Not Started |

**Total Points:** {sum}

**Risks / Blockers:**
- {Risk 1}

**Definition of Done:** See project-management SKILL.md
```
