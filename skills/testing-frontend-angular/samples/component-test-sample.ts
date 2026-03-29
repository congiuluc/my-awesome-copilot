import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProductListComponent } from './product-list.component';
import { ProductService } from '../services/product.service';
import { Product } from '../models/product.model';

describe('ProductListComponent', () => {
  let component: ProductListComponent;
  let fixture: ComponentFixture<ProductListComponent>;
  let productServiceSpy: jasmine.SpyObj<ProductService>;

  const mockProducts: Product[] = [
    {
      id: '1',
      name: 'Widget',
      description: 'A useful widget',
      price: 29.99,
      imageUrl: 'https://example.com/widget.jpg',
    },
    {
      id: '2',
      name: 'Gadget',
      description: 'A fancy gadget',
      price: 49.99,
      imageUrl: 'https://example.com/gadget.jpg',
    },
  ];

  beforeEach(async () => {
    productServiceSpy = jasmine.createSpyObj('ProductService', ['getAll']);

    await TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        { provide: ProductService, useValue: productServiceSpy },
        provideRouter([]),
      ],
    }).compileComponents();
  });

  // #region Rendering Tests

  it('should display a list of products', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    const items = fixture.nativeElement.querySelectorAll('[role="list"] li');
    expect(items.length).toBe(2);
  });

  it('should display product names', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('Widget');
    expect(content).toContain('Gadget');
  });

  it('should display product prices', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    const content = fixture.nativeElement.textContent;
    expect(content).toContain('$29.99');
    expect(content).toContain('$49.99');
  });

  // #endregion

  // #region State Handling Tests

  it('should show error message on API failure', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: false, data: null as any, error: 'Server error' }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]');
    expect(alert).toBeTruthy();
    expect(alert.textContent).toContain('Server error');
  });

  it('should show error on network failure', () => {
    productServiceSpy.getAll.and.returnValue(
      throwError(() => new Error('Network error')),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]');
    expect(alert).toBeTruthy();
    expect(alert.textContent).toContain('unexpected error');
  });

  it('should show empty state when no products', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: [] }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No products found');
  });

  it('should retry loading when retry button is clicked', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: false, data: null as any, error: 'Server error' }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    // Reset to successful response
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    const retryButton = fixture.nativeElement.querySelector(
      '[role="alert"] button',
    );
    retryButton.click();
    fixture.detectChanges();

    const items = fixture.nativeElement.querySelectorAll('[role="list"] li');
    expect(items.length).toBe(2);
  });

  // #endregion

  // #region Search/Filter Tests

  it('should filter products by search term', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const searchInput: HTMLInputElement =
      fixture.nativeElement.querySelector('input[type="search"]');
    searchInput.value = 'Widget';
    searchInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const items = fixture.nativeElement.querySelectorAll('[role="list"] li');
    expect(items.length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Widget');
    expect(fixture.nativeElement.textContent).not.toContain('Gadget');
  });

  it('should display count of filtered vs total products', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('2 of 2');

    const searchInput: HTMLInputElement =
      fixture.nativeElement.querySelector('input[type="search"]');
    searchInput.value = 'Widget';
    searchInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('1 of 2');
  });

  // #endregion

  // #region Accessibility Tests

  it('should have a labeled search input', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    const label = fixture.nativeElement.querySelector('label[for="search"]');
    expect(label).toBeTruthy();
  });

  it('should use proper list role for product grid', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    const list = fixture.nativeElement.querySelector('[role="list"]');
    expect(list).toBeTruthy();
  });

  it('should have alt text on product images', () => {
    productServiceSpy.getAll.and.returnValue(
      of({ success: true, data: mockProducts }),
    );

    fixture = TestBed.createComponent(ProductListComponent);
    fixture.detectChanges();

    const images = fixture.nativeElement.querySelectorAll('img');
    images.forEach((img: HTMLImageElement) => {
      expect(img.alt).toBeTruthy();
    });
  });

  // #endregion
});
