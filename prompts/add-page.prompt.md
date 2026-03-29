---
description: "Scaffold a new frontend page: types, API service, components, page layout, and route registration."
agent: "frontend-creator"
argument-hint: "Page name and description (e.g., 'Product list page with search, filters, and pagination')"
---
Scaffold a complete frontend page with all supporting files:

$ARGUMENTS

Implementation order:
1. TypeScript interfaces/types for the feature data
2. API service function(s) with typed responses
3. Shared components if needed (using project component library)
4. Feature component(s) with accessibility baked in
5. Page component with loading, error, and empty states
6. Route registration with lazy loading
7. Verify build produces zero errors and zero warnings
8. List all components/hooks that need tests
