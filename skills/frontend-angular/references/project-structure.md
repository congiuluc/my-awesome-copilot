# Angular Project Structure

## Feature-Based Layout

```
src/web-app/
├── src/
│   ├── app/
│   │   ├── core/                          # App-wide singletons
│   │   │   ├── guards/                    # Functional route guards
│   │   │   ├── interceptors/              # Functional HTTP interceptors
│   │   │   ├── models/                    # Shared interfaces/types
│   │   │   └── services/                  # App-wide services
│   │   ├── features/                      # Feature modules
│   │   │   └── {feature}/
│   │   │       ├── components/            # Feature-specific components
│   │   │       ├── models/                # Feature interfaces
│   │   │       ├── pages/                 # Routed page components
│   │   │       ├── services/              # Feature services
│   │   │       └── {feature}.routes.ts    # Feature routes
│   │   ├── shared/                        # Shared, reusable items
│   │   │   ├── components/                # Shared components
│   │   │   ├── directives/                # Shared directives
│   │   │   └── pipes/                     # Shared pipes
│   │   ├── app.component.ts               # Root component
│   │   ├── app.config.ts                  # App configuration (providers)
│   │   └── app.routes.ts                  # Top-level routes
│   ├── assets/                            # Static assets
│   ├── environments/                      # Environment configs
│   ├── index.html
│   ├── main.ts                            # Bootstrap entry
│   └── styles.css                         # Global styles
├── angular.json
├── package.json
└── tsconfig.json
```

## File Naming Conventions

| Type         | Pattern                               | Example                           |
| ------------ | ------------------------------------- | --------------------------------- |
| Component    | `{name}.component.ts`                 | `item-list.component.ts`          |
| Template     | `{name}.component.html`               | `item-list.component.html`        |
| Styles       | `{name}.component.css`                | `item-list.component.css`         |
| Service      | `{name}.service.ts`                   | `item.service.ts`                 |
| Guard        | `{name}.guard.ts`                     | `auth.guard.ts`                   |
| Interceptor  | `{name}.interceptor.ts`               | `error.interceptor.ts`            |
| Pipe         | `{name}.pipe.ts`                      | `truncate.pipe.ts`                |
| Directive    | `{name}.directive.ts`                 | `highlight.directive.ts`          |
| Model        | `{name}.model.ts` or `{name}.ts`      | `item.model.ts`                   |
| Test         | `{name}.component.spec.ts`            | `item-list.component.spec.ts`     |
| Route config | `{feature}.routes.ts`                 | `items.routes.ts`                 |

## Import Order

1. Angular core (`@angular/core`, `@angular/common`)
2. Angular modules (`@angular/forms`, `@angular/router`)
3. Third-party libraries (`rxjs`, `@angular/cdk`)
4. App core (`@app/core/`)
5. App shared (`@app/shared/`)
6. Feature-local imports (`./`, `../`)
