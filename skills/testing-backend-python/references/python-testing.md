# Python Backend Testing Reference

## Frameworks

- **pytest** as the test framework.
- **pytest-asyncio** for async test support.
- **pytest-mock** (wraps `unittest.mock`) for mocking dependencies.
- **httpx** (`AsyncClient`) for FastAPI integration tests.
- **factory_boy** for generating realistic test data.
- **pytest-cov** for code coverage reporting.

## Test Structure

```
tests/
  unit/
    domain/                    # Domain model unit tests
    application/               # Service unit tests
    infrastructure/            # Repository unit tests
  integration/                 # API endpoint integration tests
  conftest.py                  # Shared fixtures (app, client, db session)
```

## Naming Conventions

- Test file: `test_{module_name}.py` (e.g., `test_product_service.py`).
- Test class: `Test{ClassUnderTest}` (e.g., `TestProductService`).
- Test function: `test_should_do_x_when_condition` (e.g., `test_should_return_product_when_id_exists`).
- Fixture: descriptive `snake_case` (e.g., `product_service`, `mock_repository`).

## Unit Test Pattern (Arrange-Act-Assert)

```python
import pytest
from unittest.mock import AsyncMock

from app.application.services.product_service import ProductServiceImpl
from app.domain.exceptions import ProductNotFoundError
from app.domain.models.product import Product


class TestProductService:
    """Unit tests for ProductServiceImpl."""

    @pytest.fixture
    def mock_repository(self) -> AsyncMock:
        return AsyncMock()

    @pytest.fixture
    def sut(self, mock_repository: AsyncMock) -> ProductServiceImpl:
        return ProductServiceImpl(repository=mock_repository)

    @pytest.mark.asyncio
    async def test_should_return_product_when_id_exists(
        self, sut: ProductServiceImpl, mock_repository: AsyncMock
    ) -> None:
        # Arrange
        product = Product(id="123", name="Widget", price=Decimal("29.99"))
        mock_repository.get_by_id.return_value = product

        # Act
        result = await sut.find_by_id("123")

        # Assert
        assert result is not None
        assert result.id == "123"
        assert result.name == "Widget"

    @pytest.mark.asyncio
    async def test_should_raise_not_found_when_id_missing(
        self, sut: ProductServiceImpl, mock_repository: AsyncMock
    ) -> None:
        # Arrange
        mock_repository.get_by_id.return_value = None

        # Act & Assert
        with pytest.raises(ProductNotFoundError):
            await sut.find_by_id("999")

    @pytest.mark.asyncio
    async def test_should_raise_value_error_when_id_empty(
        self, sut: ProductServiceImpl
    ) -> None:
        with pytest.raises(ValueError):
            await sut.find_by_id("")
```

## Integration Test Pattern (FastAPI Endpoints)

```python
import pytest
from httpx import ASGITransport, AsyncClient

from app.main import app


@pytest.fixture
async def client() -> AsyncClient:
    transport = ASGITransport(app=app)
    async with AsyncClient(transport=transport, base_url="http://test") as ac:
        yield ac


class TestProductEndpoints:
    """Integration tests for product API endpoints."""

    @pytest.mark.asyncio
    async def test_should_return_products(self, client: AsyncClient) -> None:
        response = await client.get("/api/products")

        assert response.status_code == 200
        body = response.json()
        assert body["success"] is True
        assert isinstance(body["data"], list)

    @pytest.mark.asyncio
    async def test_should_return_404_when_not_found(
        self, client: AsyncClient
    ) -> None:
        response = await client.get("/api/products/nonexistent")

        assert response.status_code == 404
        body = response.json()
        assert body["success"] is False
        assert body["error"] is not None

    @pytest.mark.asyncio
    async def test_should_return_422_when_invalid_request(
        self, client: AsyncClient
    ) -> None:
        response = await client.post(
            "/api/products",
            json={"name": "", "price": -1},
        )

        assert response.status_code == 422

    @pytest.mark.asyncio
    async def test_should_create_product(self, client: AsyncClient) -> None:
        response = await client.post(
            "/api/products",
            json={"name": "Widget", "description": "A widget", "price": "29.99"},
        )

        assert response.status_code == 201
        body = response.json()
        assert body["success"] is True
        assert body["data"]["name"] == "Widget"
```

## Dependency Override Pattern

```python
from app.api.deps import get_product_service
from unittest.mock import AsyncMock


@pytest.fixture
def mock_service() -> AsyncMock:
    return AsyncMock()


@pytest.fixture
def client_with_mock(mock_service: AsyncMock) -> AsyncClient:
    app.dependency_overrides[get_product_service] = lambda: mock_service
    transport = ASGITransport(app=app)
    client = AsyncClient(transport=transport, base_url="http://test")
    yield client
    app.dependency_overrides.clear()
```

## Parametrized Tests

```python
@pytest.mark.parametrize(
    "product_id",
    [None, "", "   "],
    ids=["none", "empty", "whitespace"],
)
@pytest.mark.asyncio
async def test_should_raise_when_id_invalid(
    sut: ProductServiceImpl, product_id: str | None
) -> None:
    with pytest.raises((ValueError, TypeError)):
        await sut.find_by_id(product_id)
```

## What to Test

- All service / business logic methods (happy path + edge cases).
- Repository implementations with test databases or mocks.
- API endpoints: status codes, response envelope, validation errors.
- Edge cases: None inputs, empty collections, concurrent access.
- Error handling: exceptions translated to correct ApiResponse errors.
- Validation rules: all Pydantic boundary conditions.
- Async behavior: ensure no blocking calls in async context.

## Rules

- Tests must be **deterministic** — no reliance on external services, time, or randomness.
- Mock external dependencies at boundaries, not internal logic.
- Use `@pytest.mark.asyncio` for all async test functions.
- Keep tests fast: unit < 100ms, integration < 2s.
- One assertion concept per test (related assertions on one result are fine).
- Use `sut` (system under test) for the class being tested.
- Use fixtures for test setup — avoid `setUp` methods.
- Never use `sleep()` in tests — use mocks or events.
