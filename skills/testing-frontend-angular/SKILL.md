---
name: testing-frontend-angular
description: >-
  Write Angular frontend tests using Jasmine, Karma, and Angular TestBed. Use when: testing
  Angular components, services, pipes, directives, user interactions, form submissions,
  accessibility assertions, or mocking HTTP services.
argument-hint: 'Describe the Angular component or service you want to test.'
---

# Frontend Testing (Angular)

## When to Use

- Testing Angular standalone components (rendering, interaction, accessibility)
- Testing services with `HttpClient`
- Testing pipes and directives
- Mocking HTTP calls with `HttpTestingController`
- Writing integration tests for pages/features
- Verifying reactive form validation and submission

## Official Documentation

- [Angular Testing Guide](https://angular.dev/guide/testing)
- [Jasmine](https://jasmine.github.io/pages/docs_home.html)
- [Angular TestBed](https://angular.dev/api/core/testing/TestBed)
- [HttpTestingController](https://angular.dev/api/common/http/testing/HttpTestingController)

## Core Rule

**Every feature must have corresponding tests before it is considered complete.**

## Procedure

1. Follow [Angular testing reference](./references/angular-testing.md) for patterns and conventions
2. Review [component test sample](./samples/component-test-sample.ts)
3. Use `TestBed.configureTestingModule()` for component setup
4. Use `HttpTestingController` for HTTP mocking — no real network calls
5. Query elements by accessible role/label — use `fixture.nativeElement.querySelector`
6. Test: rendering, interaction, accessibility, edge cases, error states
7. Verify tests are deterministic
8. Run `ng test --no-watch --code-coverage` locally before committing

## Test Structure

```
tests/
  web-app.tests/
    components/          # Shared component tests
    features/            # Feature-specific tests
    services/            # Service tests
    pipes/               # Pipe tests
    setup.ts             # Test configuration
```

## Naming Convention

- Test file: `{component-name}.component.spec.ts`
- Test suite: `describe('{ComponentName}', () => { ... })`
- Test case: `it('should {expected behavior}', ...)`

## Key Patterns

- Component setup: `TestBed.configureTestingModule({ imports: [ComponentUnderTest] })`
- HTTP mocking: `TestBed.inject(HttpTestingController)`
- Detect changes: `fixture.detectChanges()`
- Query by role: `fixture.nativeElement.querySelector('[role="button"]')`
- Form testing: Set form values → call `detectChanges()` → assert state
