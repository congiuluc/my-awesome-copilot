# Java Backend Testing Reference

## Frameworks

- **JUnit 5** as the test framework.
- **Mockito** for mocking dependencies.
- **AssertJ** for fluent, readable assertions.
- **Spring Boot Test** (`@SpringBootTest`, `@WebMvcTest`) for integration tests.
- **Testcontainers** for database integration tests with real databases.
- **MockMvc** for controller layer testing without starting a full server.

## Test Structure

```
src/test/java/
  com/myapp/
    domain/                          # Domain model unit tests
    application/service/             # Service unit tests
    infrastructure/persistence/      # Repository adapter tests (@DataJpaTest)
    api/controller/                  # Controller integration tests (@WebMvcTest)
```

## Naming Conventions

- Test class: `{ClassUnderTest}Test` (e.g., `ProductServiceTest`).
- Test method: `shouldDoX_whenCondition` (e.g., `shouldReturnProduct_whenIdExists`).
- Test file sits in the same package as the source class.

## Unit Test Pattern (Arrange-Act-Assert)

```java
@ExtendWith(MockitoExtension.class)
class ProductServiceTest {

    @Mock
    private ProductRepository productRepository;

    @InjectMocks
    private ProductServiceImpl sut;

    @Test
    void shouldReturnProduct_whenIdExists() {
        // Arrange
        var product = new Product();
        product.setId("123");
        product.setName("Widget");
        when(productRepository.findById("123")).thenReturn(Optional.of(product));

        // Act
        var result = sut.findById("123");

        // Assert
        assertThat(result).isNotNull();
        assertThat(result.id()).isEqualTo("123");
        assertThat(result.name()).isEqualTo("Widget");
    }

    @Test
    void shouldThrowNotFoundException_whenIdDoesNotExist() {
        // Arrange
        when(productRepository.findById("999")).thenReturn(Optional.empty());

        // Act & Assert
        assertThatThrownBy(() -> sut.findById("999"))
                .isInstanceOf(ProductNotFoundException.class)
                .hasMessageContaining("999");
    }

    @ParameterizedTest
    @NullAndEmptySource
    @ValueSource(strings = {"   "})
    void shouldThrowException_whenIdIsInvalid(String id) {
        assertThatThrownBy(() -> sut.findById(id))
                .isInstanceOf(NullPointerException.class)
                .isInstanceOf(IllegalArgumentException.class);
    }
}
```

## Integration Test Pattern (Controller)

```java
@WebMvcTest(ProductController.class)
class ProductControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @MockBean
    private ProductService productService;

    @Autowired
    private ObjectMapper objectMapper;

    @Test
    void shouldReturnProducts_whenGetAll() throws Exception {
        // Arrange
        var products = List.of(
                new ProductResponse("1", "Widget", null, BigDecimal.TEN, Instant.now()));
        when(productService.findAll()).thenReturn(products);

        // Act & Assert
        mockMvc.perform(get("/api/products"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.success").value(true))
                .andExpect(jsonPath("$.data").isArray())
                .andExpect(jsonPath("$.data[0].name").value("Widget"));
    }

    @Test
    void shouldReturn404_whenProductNotFound() throws Exception {
        when(productService.findById("999"))
                .thenThrow(new ProductNotFoundException("999"));

        mockMvc.perform(get("/api/products/999"))
                .andExpect(status().isNotFound())
                .andExpect(jsonPath("$.success").value(false))
                .andExpect(jsonPath("$.error").isNotEmpty());
    }

    @Test
    void shouldReturn400_whenInvalidRequest() throws Exception {
        var request = new CreateProductRequest("", null, BigDecimal.ZERO);

        mockMvc.perform(post("/api/products")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isBadRequest());
    }

    @Test
    void shouldReturn201_whenProductCreated() throws Exception {
        var request = new CreateProductRequest("Widget", "A widget", BigDecimal.TEN);
        var response = new ProductResponse("new-id", "Widget", "A widget",
                BigDecimal.TEN, Instant.now());
        when(productService.create(any())).thenReturn(response);

        mockMvc.perform(post("/api/products")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(request)))
                .andExpect(status().isCreated())
                .andExpect(jsonPath("$.success").value(true))
                .andExpect(jsonPath("$.data.id").value("new-id"));
    }
}
```

## Repository Test Pattern

```java
@DataJpaTest
class JpaProductRepositoryTest {

    @Autowired
    private ProductJpaRepository repository;

    @Test
    void shouldSaveAndFindProduct() {
        var product = new Product();
        product.setName("Widget");
        product.setPrice(BigDecimal.TEN);

        var saved = repository.save(product);

        assertThat(saved.getId()).isNotNull();
        var found = repository.findById(saved.getId());
        assertThat(found).isPresent();
        assertThat(found.get().getName()).isEqualTo("Widget");
    }
}
```

## What to Test

- All service / business logic methods (happy path + edge cases).
- Repository adapters with `@DataJpaTest` or Testcontainers.
- Controller endpoints: status codes, response envelope, validation errors.
- Edge cases: null inputs, empty collections, concurrent access.
- Error handling: exceptions translated to correct `ApiResponse` errors.
- Validation rules: all boundary conditions.

## Rules

- Tests must be **deterministic** — no reliance on external services, time, or randomness.
- Use `@ExtendWith(MockitoExtension.class)` for unit tests — not `@SpringBootTest`.
- Mock external dependencies at boundaries, not internal logic.
- Keep tests fast: unit < 100ms, integration < 2s.
- One assertion concept per test (AssertJ chains count as one concept).
- Use `sut` (system under test) for the class being tested.
- Use `@DisplayName` for human-readable test descriptions when method names are complex.
