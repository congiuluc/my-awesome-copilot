# Task Breakdown Guide

## How to Decompose a User Story into Tasks

Every user story should be broken into implementable tasks across these layers:

### 1. Backend Tasks

```markdown
- [ ] **Domain Model**: Create/update entity in `MyApp.Core/Models/`
- [ ] **DTO**: Create request/response DTOs in `MyApp.Core/DTOs/`
- [ ] **Repository Interface**: Add methods to `IRepository<T>` or create new interface
- [ ] **Repository Implementation**: Implement for SQLite and/or Cosmos DB
- [ ] **Service Layer**: Create service with business logic in `MyApp.Core/Services/`
- [ ] **API Endpoint**: Map route in `MyApp.Api/Endpoints/`
- [ ] **Validation**: Add FluentValidation rules
- [ ] **Middleware/Filters**: Update if needed (auth, caching)
- [ ] **Backend Tests**: Unit tests for service, integration tests for endpoint
```

### 2. Frontend Tasks

```markdown
- [ ] **Types**: Define TypeScript interfaces in `types/`
- [ ] **API Service**: Create/update API client function in `services/`
- [ ] **Custom Hook**: Create data-fetching hook with loading/error states
- [ ] **Component(s)**: Build UI components (list, detail, form, etc.)
- [ ] **Page/Route**: Wire component into router
- [ ] **Image Preloading**: Add `useImagePreload` if data contains image URLs
- [ ] **Accessibility**: Verify keyboard nav, ARIA, screen reader support
- [ ] **Responsive Design**: Test at 320px, 768px, 1024px, 1280px
- [ ] **Frontend Tests**: Component tests with React Testing Library
```

### 3. Cross-Cutting Tasks

```markdown
- [ ] **Database Migration**: Add migration if schema changes
- [ ] **API Documentation**: Verify Swagger/OpenAPI is updated
- [ ] **Error Handling**: Verify error states in both frontend and backend
- [ ] **Performance**: Check pagination, caching, lazy loading as needed
- [ ] **Security**: Input validation, authorization, XSS prevention
- [ ] **Code Review**: PR created and reviewed
```

## Task Sizing Rules

| Duration | Action |
|----------|--------|
| < 2 hours | Single task — implement directly |
| 2–8 hours | Split into 2–4 subtasks |
| > 8 hours | This is a story, not a task — break it down |

## Dependency Mapping

Create tasks in this order to avoid blocking:

```
1. Domain Model → 2. Repository Interface → 3. Repository Implementation
                                           ↘
                                            4. Service Layer → 5. API Endpoint → 6. Frontend API Service
                                                                                ↘
                                                                                 7. Components → 8. Tests
```

## Example

**Story:** "As a user, I want to view a list of products so I can browse available items."

**Tasks:**
1. ✅ Backend: Create `Product` entity and `ProductDto`
2. ✅ Backend: Add `GetAllAsync` to `IRepository<Product>`
3. ✅ Backend: Implement repository for SQLite
4. ✅ Backend: Create `ProductService.GetAllAsync()` with pagination
5. ✅ Backend: Map `GET /api/products` endpoint with OpenAPI metadata
6. ✅ Backend: Write unit test for service + integration test for endpoint
7. ✅ Frontend: Define `Product` TypeScript type and API service
8. ✅ Frontend: Create `useProducts` hook with loading/error states
9. ✅ Frontend: Build `ProductList` component with card grid
10. ✅ Frontend: Preload product images during data loading
11. ✅ Frontend: Write component tests
12. ✅ Cross-cutting: Verify accessibility, responsive design, Swagger docs
