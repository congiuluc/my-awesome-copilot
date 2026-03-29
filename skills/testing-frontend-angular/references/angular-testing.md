# Angular Testing Reference

## Component Testing

### Basic Component Test

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ItemListComponent } from './item-list.component';
import { ItemService } from '../services/item.service';
import { of } from 'rxjs';

describe('ItemListComponent', () => {
  let component: ItemListComponent;
  let fixture: ComponentFixture<ItemListComponent>;
  let itemServiceSpy: jasmine.SpyObj<ItemService>;

  beforeEach(async () => {
    itemServiceSpy = jasmine.createSpyObj('ItemService', ['getAll']);

    await TestBed.configureTestingModule({
      imports: [ItemListComponent],
      providers: [
        { provide: ItemService, useValue: itemServiceSpy },
      ],
    }).compileComponents();
  });

  it('should render items when data loads successfully', () => {
    itemServiceSpy.getAll.and.returnValue(of({
      success: true,
      data: [
        { id: '1', name: 'Item 1', description: 'Desc 1' },
        { id: '2', name: 'Item 2', description: 'Desc 2' },
      ],
    }));

    fixture = TestBed.createComponent(ItemListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const items = fixture.nativeElement.querySelectorAll('[role="list"] li');
    expect(items.length).toBe(2);
  });

  it('should show loading spinner initially', () => {
    itemServiceSpy.getAll.and.returnValue(of()); // never emits
    fixture = TestBed.createComponent(ItemListComponent);
    fixture.detectChanges();

    const spinner = fixture.nativeElement.querySelector('[aria-label="Loading"]');
    expect(spinner).toBeTruthy();
  });

  it('should show error message on failure', () => {
    itemServiceSpy.getAll.and.returnValue(of({
      success: false,
      data: null,
      error: 'Server error',
    }));

    fixture = TestBed.createComponent(ItemListComponent);
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]');
    expect(alert).toBeTruthy();
    expect(alert.textContent).toContain('Server error');
  });

  it('should show empty state when no items', () => {
    itemServiceSpy.getAll.and.returnValue(of({
      success: true,
      data: [],
    }));

    fixture = TestBed.createComponent(ItemListComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No items found');
  });
});
```

### Testing with Input Signals

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ComponentRef } from '@angular/core';

describe('ItemCardComponent', () => {
  let fixture: ComponentFixture<ItemCardComponent>;
  let componentRef: ComponentRef<ItemCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ItemCardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ItemCardComponent);
    componentRef = fixture.componentRef;
  });

  it('should display item name', () => {
    componentRef.setInput('item', { id: '1', name: 'Test Item' });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Test Item');
  });
});
```

## Service Testing with HttpTestingController

```typescript
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

describe('ItemService', () => {
  let service: ItemService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ItemService,
      ],
    });
    service = TestBed.inject(ItemService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Ensure no outstanding requests
  });

  it('should fetch all items', () => {
    const mockResponse = {
      success: true,
      data: [{ id: '1', name: 'Item 1' }],
    };

    service.getAll().subscribe((response) => {
      expect(response.success).toBeTrue();
      expect(response.data.length).toBe(1);
    });

    const req = httpMock.expectOne('/api/items');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should create an item', () => {
    const newItem = { name: 'New Item', description: 'Desc' };
    const mockResponse = {
      success: true,
      data: { id: '2', ...newItem },
    };

    service.create(newItem).subscribe((response) => {
      expect(response.success).toBeTrue();
      expect(response.data.id).toBe('2');
    });

    const req = httpMock.expectOne('/api/items');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newItem);
    req.flush(mockResponse);
  });
});
```

## Reactive Form Testing

```typescript
describe('ItemFormComponent', () => {
  let component: ItemFormComponent;
  let fixture: ComponentFixture<ItemFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ItemFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ItemFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should mark form invalid when name is empty', () => {
    component.form.controls.name.setValue('');
    expect(component.form.valid).toBeFalse();
  });

  it('should mark form valid when all required fields filled', () => {
    component.form.controls.name.setValue('Test Item');
    component.form.controls.category.setValue('category-1');
    expect(component.form.valid).toBeTrue();
  });

  it('should show validation error when name exceeds max length', () => {
    component.form.controls.name.setValue('a'.repeat(101));
    expect(component.form.controls.name.hasError('maxlength')).toBeTrue();
  });
});
```

## Pipe Testing

```typescript
describe('TruncatePipe', () => {
  const pipe = new TruncatePipe();

  it('should return the same string if under max length', () => {
    expect(pipe.transform('Hello', 10)).toBe('Hello');
  });

  it('should truncate and add ellipsis', () => {
    expect(pipe.transform('Hello World', 5)).toBe('Hello…');
  });

  it('should use default max length of 50', () => {
    const longText = 'a'.repeat(60);
    expect(pipe.transform(longText).length).toBe(51); // 50 chars + ellipsis
  });
});
```

## Testing Accessibility

```typescript
it('should have proper ARIA attributes', () => {
  const button = fixture.nativeElement.querySelector('button');
  expect(button.getAttribute('aria-label')).toBeTruthy();
});

it('should be keyboard accessible', () => {
  const element = fixture.nativeElement.querySelector('[tabindex]');
  expect(element).toBeTruthy();
});

it('should have focus indicator styles', () => {
  const interactive = fixture.nativeElement.querySelector('button');
  interactive.focus();
  fixture.detectChanges();
  // Verify focus is properly managed
  expect(document.activeElement).toBe(interactive);
});
```

## Test Conventions

- **One spec file per source file** — `item-list.component.spec.ts` next to `item-list.component.ts`
- **Use `jasmine.createSpyObj`** for service mocks
- **Always call `fixture.detectChanges()`** after changing signals or inputs
- **Verify `httpMock.verify()`** in `afterEach` for HTTP tests
- **No real network calls** — always mock HTTP with `HttpTestingController`
- **Test observable completion** — ensure subscriptions complete properly
