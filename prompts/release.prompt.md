---
description: "Run the release checklist: verify tests pass, update changelog, bump version, and prepare for deployment."
agent: "release-manager"
argument-hint: "Release type (patch, minor, major) or leave blank for auto-detect"
---
Prepare a release following the full release checklist:

$ARGUMENTS

Steps:
1. Verify all tests pass across all layers
2. Check for any unresolved Critical or Important review issues
3. Scan for known vulnerabilities in dependencies
4. Determine version bump type from commit history (or use specified type)
5. Update CHANGELOG.md with all changes since last release
6. Bump version numbers in all relevant files
7. Present the release summary for approval before any push/tag operations
