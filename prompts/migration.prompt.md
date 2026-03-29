---
description: "Generate a database migration with safety checks: schema change, migration file, rollback plan, data preservation verification."
agent: "backend-creator"
argument-hint: "Describe the schema change (e.g., 'Add Email column to Users table')"
---
Generate a database migration for the following schema change:

$ARGUMENTS

Steps:
1. Review the current data model and existing migrations
2. Create the migration following naming convention: `{Timestamp}_{Description}`
3. Verify the migration is safe:
   - No data loss
   - Backward compatible if possible
   - Indexes for new columns used in queries
   - Default values for new non-nullable columns
4. Document the rollback strategy
5. Verify build after adding the migration
