// Sample: Java Backend Unit Tests with JUnit 5, Mockito, and AssertJ
// Shows Arrange-Act-Assert pattern, mocking, parameterized tests, and naming conventions.

package com.myapp.application.service;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.List;
import java.util.Optional;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.NullAndEmptySource;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import com.myapp.api.dto.CreateProductRequest;
import com.myapp.api.dto.ProductResponse;
import com.myapp.application.port.ProductRepository;
import com.myapp.application.service.impl.ProductServiceImpl;
import com.myapp.domain.exception.ProductNotFoundException;
import com.myapp.domain.model.Product;

/**
 * Unit tests for {@link ProductServiceImpl}.
 */
@ExtendWith(MockitoExtension.class)
class ProductServiceTest {

    @Mock
    private ProductRepository productRepository;

    @InjectMocks
    private ProductServiceImpl sut;

    // --- findById ---

    @Test
    void shouldReturnProduct_whenIdExists() {
        // Arrange
        var product = new Product();
        product.setId("prod-123");
        product.setName("Widget");
        product.setPrice(BigDecimal.valueOf(29.99));
        product.setCreatedAt(Instant.now());
        when(productRepository.findById("prod-123")).thenReturn(Optional.of(product));

        // Act
        var result = sut.findById("prod-123");

        // Assert
        assertThat(result).isNotNull();
        assertThat(result.id()).isEqualTo("prod-123");
        assertThat(result.name()).isEqualTo("Widget");
        assertThat(result.price()).isEqualByComparingTo(BigDecimal.valueOf(29.99));
    }

    @Test
    void shouldThrowNotFoundException_whenIdDoesNotExist() {
        // Arrange
        when(productRepository.findById("missing")).thenReturn(Optional.empty());

        // Act & Assert
        assertThatThrownBy(() -> sut.findById("missing"))
                .isInstanceOf(ProductNotFoundException.class)
                .hasMessageContaining("missing");
    }

    @ParameterizedTest
    @NullAndEmptySource
    void shouldThrowException_whenIdIsInvalid(String id) {
        // Act & Assert
        assertThatThrownBy(() -> sut.findById(id))
                .isInstanceOf(NullPointerException.class);
    }

    // --- findAll ---

    @Test
    void shouldReturnAllProducts() {
        // Arrange
        var products = List.of(
                createProduct("1", "A", BigDecimal.TEN),
                createProduct("2", "B", BigDecimal.valueOf(20)));
        when(productRepository.findAll()).thenReturn(products);

        // Act
        var result = sut.findAll();

        // Assert
        assertThat(result).hasSize(2);
        assertThat(result).extracting(ProductResponse::name)
                .containsExactly("A", "B");
    }

    @Test
    void shouldReturnEmptyList_whenNoProducts() {
        // Arrange
        when(productRepository.findAll()).thenReturn(List.of());

        // Act
        var result = sut.findAll();

        // Assert
        assertThat(result).isEmpty();
    }

    // --- create ---

    @Test
    void shouldCreateProduct_whenValidRequest() {
        // Arrange
        var request = new CreateProductRequest("Widget", "A useful widget", BigDecimal.valueOf(29.99));
        when(productRepository.save(any(Product.class))).thenAnswer(invocation -> {
            var product = invocation.getArgument(0, Product.class);
            product.setId("new-id");
            product.setCreatedAt(Instant.now());
            return product;
        });

        // Act
        var result = sut.create(request);

        // Assert
        assertThat(result).isNotNull();
        assertThat(result.id()).isEqualTo("new-id");
        assertThat(result.name()).isEqualTo("Widget");
        verify(productRepository).save(any(Product.class));
    }

    // --- delete ---

    @Test
    void shouldDeleteProduct_whenExists() {
        // Arrange
        when(productRepository.existsById("prod-123")).thenReturn(true);

        // Act
        sut.delete("prod-123");

        // Assert
        verify(productRepository).deleteById("prod-123");
    }

    @Test
    void shouldThrowNotFoundException_whenDeletingNonExistent() {
        // Arrange
        when(productRepository.existsById("missing")).thenReturn(false);

        // Act & Assert
        assertThatThrownBy(() -> sut.delete("missing"))
                .isInstanceOf(ProductNotFoundException.class);
    }

    // --- Helpers ---

    private Product createProduct(String id, String name, BigDecimal price) {
        var product = new Product();
        product.setId(id);
        product.setName(name);
        product.setPrice(price);
        product.setCreatedAt(Instant.now());
        return product;
    }
}
