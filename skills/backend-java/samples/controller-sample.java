// Sample: Spring Boot REST Controller with Hexagonal Architecture
// This shows the complete pattern for a feature controller.

package com.myapp.api.controller;

import java.net.URI;
import java.util.List;

import jakarta.validation.Valid;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;

import com.myapp.api.dto.ApiResponse;
import com.myapp.api.dto.CreateProductRequest;
import com.myapp.api.dto.ProductResponse;
import com.myapp.api.dto.UpdateProductRequest;
import com.myapp.application.service.ProductService;

/**
 * REST controller for product management endpoints.
 */
@RestController
@RequestMapping("/api/products")
@Tag(name = "Product", description = "Product management")
public class ProductController {

    private final ProductService productService;

    public ProductController(ProductService productService) {
        this.productService = productService;
    }

    /**
     * Retrieves all products.
     *
     * @return list of all products
     */
    @GetMapping
    @Operation(summary = "Get all products")
    public ResponseEntity<ApiResponse<List<ProductResponse>>> getAll() {
        var products = productService.findAll();
        return ResponseEntity.ok(ApiResponse.ok(products));
    }

    /**
     * Retrieves a product by its unique identifier.
     *
     * @param id the product identifier
     * @return the product if found
     */
    @GetMapping("/{id}")
    @Operation(summary = "Get a product by ID")
    public ResponseEntity<ApiResponse<ProductResponse>> getById(@PathVariable String id) {
        var product = productService.findById(id);
        return ResponseEntity.ok(ApiResponse.ok(product));
    }

    /**
     * Creates a new product.
     *
     * @param request the product creation request
     * @return the created product
     */
    @PostMapping
    @Operation(summary = "Create a new product")
    public ResponseEntity<ApiResponse<ProductResponse>> create(
            @Valid @RequestBody CreateProductRequest request) {
        var product = productService.create(request);
        var uri = URI.create("/api/products/" + product.id());
        return ResponseEntity.created(uri).body(ApiResponse.ok(product));
    }

    /**
     * Updates an existing product.
     *
     * @param id      the product identifier
     * @param request the product update request
     * @return the updated product
     */
    @PutMapping("/{id}")
    @Operation(summary = "Update an existing product")
    public ResponseEntity<ApiResponse<ProductResponse>> update(
            @PathVariable String id,
            @Valid @RequestBody UpdateProductRequest request) {
        var product = productService.update(id, request);
        return ResponseEntity.ok(ApiResponse.ok(product));
    }

    /**
     * Deletes a product.
     *
     * @param id the product identifier
     * @return no content
     */
    @DeleteMapping("/{id}")
    @Operation(summary = "Delete a product")
    public ResponseEntity<Void> delete(@PathVariable String id) {
        productService.delete(id);
        return ResponseEntity.noContent().build();
    }
}

// --- DTOs (in api/dto/) ---

// ApiResponse.java
/**
 * Standard API response envelope.
 *
 * @param success whether the request succeeded
 * @param data    the response data (null on error)
 * @param error   error message (null on success)
 */
public record ApiResponse<T>(boolean success, T data, String error) {

    public static <T> ApiResponse<T> ok(T data) {
        return new ApiResponse<>(true, data, null);
    }

    public static <T> ApiResponse<T> error(String message) {
        return new ApiResponse<>(false, null, message);
    }
}

// CreateProductRequest.java
/**
 * Request to create a new product.
 */
public record CreateProductRequest(
        @NotBlank @Size(max = 100) String name,
        @Size(max = 500) String description,
        @NotNull @Positive BigDecimal price) {
}

// UpdateProductRequest.java
/**
 * Request to update an existing product.
 */
public record UpdateProductRequest(
        @NotBlank @Size(max = 100) String name,
        @Size(max = 500) String description,
        @NotNull @Positive BigDecimal price) {
}

// ProductResponse.java
/**
 * Product data returned to API consumers.
 */
public record ProductResponse(
        String id,
        String name,
        String description,
        BigDecimal price,
        Instant createdAt) {

    /**
     * Maps a domain Product to an API response.
     */
    public static ProductResponse from(Product product) {
        return new ProductResponse(
                product.getId(),
                product.getName(),
                product.getDescription(),
                product.getPrice(),
                product.getCreatedAt());
    }
}

// --- Service interface (in application/service/) ---

/**
 * Service for product business logic.
 */
public interface ProductService {
    List<ProductResponse> findAll();
    ProductResponse findById(String id);
    ProductResponse create(CreateProductRequest request);
    ProductResponse update(String id, UpdateProductRequest request);
    void delete(String id);
}

// --- Service implementation (in application/service/impl/) ---

/**
 * Product service implementation using repository pattern.
 */
@Service
@Transactional(readOnly = true)
public class ProductServiceImpl implements ProductService {

    private static final Logger log = LoggerFactory.getLogger(ProductServiceImpl.class);

    private final ProductRepository productRepository;

    public ProductServiceImpl(ProductRepository productRepository) {
        this.productRepository = Objects.requireNonNull(productRepository);
    }

    @Override
    public List<ProductResponse> findAll() {
        log.debug("Fetching all products");
        return productRepository.findAll().stream()
                .map(ProductResponse::from)
                .toList();
    }

    @Override
    public ProductResponse findById(String id) {
        Objects.requireNonNull(id, "id must not be null");
        log.debug("Fetching product with id={}", id);
        return productRepository.findById(id)
                .map(ProductResponse::from)
                .orElseThrow(() -> new ProductNotFoundException(id));
    }

    @Override
    @Transactional
    public ProductResponse create(CreateProductRequest request) {
        log.info("Creating product with name={}", request.name());
        var product = new Product();
        product.setName(request.name());
        product.setDescription(request.description());
        product.setPrice(request.price());
        product.setCreatedAt(Instant.now());
        var saved = productRepository.save(product);
        return ProductResponse.from(saved);
    }

    @Override
    @Transactional
    public ProductResponse update(String id, UpdateProductRequest request) {
        log.info("Updating product with id={}", id);
        var product = productRepository.findById(id)
                .orElseThrow(() -> new ProductNotFoundException(id));
        product.setName(request.name());
        product.setDescription(request.description());
        product.setPrice(request.price());
        var saved = productRepository.save(product);
        return ProductResponse.from(saved);
    }

    @Override
    @Transactional
    public void delete(String id) {
        log.info("Deleting product with id={}", id);
        if (!productRepository.existsById(id)) {
            throw new ProductNotFoundException(id);
        }
        productRepository.deleteById(id);
    }
}
