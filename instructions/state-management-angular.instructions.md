---
description: "Use when managing Angular frontend state: signals for component state, NgRx SignalStore for feature state, services for global UI state, URL state for filters/pagination."
applyTo: "src/web-app/src/app/stores/**,src/web-app/src/app/services/**"
---
# Angular State Management Guidelines

## State Categories

| Category | Tool | When |
|----------|------|------|
| **Local UI** | `signal()`, `computed()` | Form inputs, open/close, selection |
| **Feature state** | NgRx SignalStore | Feature-level CRUD, complex workflows |
| **Global UI** | Service + signals | Theme, auth status, sidebar |
| **URL state** | `ActivatedRoute`, `Router` | Filters, sort, page number |

## Signals (Default)

- Use `signal()` for mutable component state.
- Use `computed()` for derived state — never store what you can derive.
- Use `update()` for transitions based on previous value.
- Expose `asReadonly()` from services — keep the writable signal private.

## NgRx SignalStore

- Use for feature-level state shared across multiple components.
- Use `withState` for shape, `withComputed` for derivations, `withMethods` for actions.
- Use `rxMethod` + `tapResponse` for async operations.
- Use `patchState` for immutable updates — never mutate directly.

## Service State

- Use `@Injectable({ providedIn: 'root' })` services with private signals.
- Keep services single-responsibility — one service per domain concern.
- Expose readonly signals and methods — don't leak implementation.

## URL State

- Use `toSignal()` to bridge `ActivatedRoute` query params into signals.
- Use `queryParamsHandling: 'merge'` when updating params.
- Filters, sort, page number belong in query params.

## RxJS Bridge

- Use `toSignal()` when consuming observables in templates (replaces `async` pipe).
- Use `toObservable()` when feeding signals into RxJS pipelines.
- Always provide `initialValue` for `toSignal()`.

## Anti-Patterns

- Don't use `BehaviorSubject` when `signal()` suffices.
- Don't use full NgRx Store for simple CRUD — use SignalStore.
- Don't subscribe manually in components — use `toSignal()` or `async` pipe.
- Don't mutate signal values — use `set()`, `update()`, or `patchState()`.
