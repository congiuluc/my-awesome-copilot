---
description: "Angular testing conventions: Jasmine, Karma, TestBed, HttpTestingController, standalone component testing patterns."
applyTo: "src/web-app/src/app/**/*.spec.ts"
---

# Angular Testing Conventions

## Test Setup
- Use `TestBed.configureTestingModule({ imports: [ComponentUnderTest] })` for standalone components
- Use `jasmine.createSpyObj` for service mocks
- Use `provideHttpClient()` + `provideHttpClientTesting()` for HTTP tests
- Always call `httpMock.verify()` in `afterEach`

## Component Tests
- Always call `fixture.detectChanges()` after changing signals or inputs
- Use `componentRef.setInput()` for input signals
- Query elements by accessible role/label, not CSS classes
- Test loading, error, and empty states

## Service Tests
- Use `HttpTestingController` — no real network calls
- Verify request method and URL with `expectOne()`
- Test both success and error responses

## Naming
- File: `{component-name}.component.spec.ts` or `{service-name}.service.spec.ts`
- Suite: `describe('{ClassName}', () => { ... })`
- Case: `it('should {expected behavior}', ...)`

## Assertions
- Use Jasmine matchers: `expect(...).toBe()`, `toBeTrue()`, `toBeTruthy()`
- Verify DOM content with `fixture.nativeElement.textContent`
- Verify ARIA attributes with `getAttribute('aria-label')`
