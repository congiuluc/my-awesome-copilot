# Angular TypeScript Conventions

## General Rules

- **Strict mode** enabled in `tsconfig.json`
- **No `any` type** — always use proper interfaces
- **camelCase** for method parameters and private fields
- **PascalCase** for classes, interfaces, enums, type aliases
- **Max line length**: 120 characters
- **Named exports only** — no default exports

## Interface Naming

```typescript
// Data model — plain name
export interface Item {
  id: string;
  name: string;
  description: string;
  createdAt: string;
}

// Request DTOs — suffixed with Request
export interface CreateItemRequest {
  name: string;
  description: string;
}

export interface UpdateItemRequest {
  name: string;
  description: string;
}

// API response envelope
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  error?: string;
}

// Form interfaces — suffixed with Form
export interface ItemForm {
  name: FormControl<string>;
  description: FormControl<string>;
}
```

## Component Conventions

```typescript
// Selector: kebab-case with app- prefix
@Component({
  selector: 'app-item-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  // ...
})
export class ItemListComponent {
  // 1. Injected dependencies (private readonly)
  private readonly itemService = inject(ItemService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  // 2. Signals (readonly)
  readonly items = signal<Item[]>([]);
  readonly loading = signal(true);

  // 3. Computed signals
  readonly itemCount = computed(() => this.items().length);

  // 4. Inputs/Outputs
  readonly itemId = input.required<string>();
  readonly itemSelected = output<Item>();

  // 5. Constructor
  constructor() {
    this.loadItems();
  }

  // 6. Public methods (used in templates)
  onItemClick(item: Item): void {
    this.itemSelected.emit(item);
  }

  // 7. Private methods
  private loadItems(): void {
    // ...
  }
}
```

## Service Conventions

```typescript
// providedIn: 'root' for app-wide services
@Injectable({ providedIn: 'root' })
export class ItemService {
  private readonly http = inject(HttpClient);
  // Private fields: camelCase
  private readonly baseUrl = '/api/items';

  // Public methods return Observable<T>
  getAll(): Observable<ApiResponse<Item[]>> {
    return this.http.get<ApiResponse<Item[]>>(this.baseUrl);
  }
}
```

## Enum and Constant Conventions

```typescript
// Enums: PascalCase
export enum ItemStatus {
  Active = 'Active',
  Archived = 'Archived',
  Draft = 'Draft',
}

// Constants: UPPER_SNAKE_CASE
export const MAX_ITEMS_PER_PAGE = 20;
export const API_BASE_URL = '/api';
```

## Pipe Conventions

```typescript
@Pipe({
  name: 'truncate',
  standalone: true,
})
export class TruncatePipe implements PipeTransform {
  transform(value: string, maxLength: number = 50): string {
    if (value.length <= maxLength) return value;
    return value.substring(0, maxLength) + '…';
  }
}
```

## Import Aliases

Configure path aliases in `tsconfig.json`:

```json
{
  "compilerOptions": {
    "paths": {
      "@app/*": ["src/app/*"],
      "@core/*": ["src/app/core/*"],
      "@shared/*": ["src/app/shared/*"],
      "@features/*": ["src/app/features/*"],
      "@env/*": ["src/environments/*"]
    }
  }
}
```
