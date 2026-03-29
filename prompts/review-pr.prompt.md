---
description: "Run all code reviewers against changed or specified files: backend reviewer, frontend reviewer, security checks."
agent: "tech-lead"
argument-hint: "Files or PR to review (or leave blank to review recent changes)"
---
Run a comprehensive code review on the following scope:

$ARGUMENTS

Review steps:
1. Detect the languages and frameworks involved
2. Run the appropriate backend reviewer(s) on backend files
3. Run the appropriate frontend reviewer(s) on frontend files
4. Summarize all findings in a single consolidated report with:
   - Critical issues (must fix)
   - Important issues (should fix)
   - Accessibility issues (if frontend)
   - Suggestions (nice to have)
   - Recommended tests
