# Sample: Python Backend Unit Tests with pytest, pytest-mock, and pytest-asyncio
# Shows Arrange-Act-Assert pattern, mocking, parametrized tests, and naming conventions.

"""Unit tests for ProductServiceImpl."""

from __future__ import annotations

from decimal import Decimal
from datetime import datetime, UTC
from unittest.mock import AsyncMock

import pytest

from app.api.schemas.product import CreateProductRequest, ProductResponse
from app.application.services.product_service import ProductServiceImpl
from app.domain.exceptions import ProductNotFoundError
from app.domain.models.product import Product


class TestProductService:
    """Unit tests for ProductServiceImpl."""

    @pytest.fixture
    def mock_repository(self) -> AsyncMock:
        """Create a mock repository."""
        return AsyncMock()

    @pytest.fixture
    def sut(self, mock_repository: AsyncMock) -> ProductServiceImpl:
        """Create the system under test with mocked dependencies."""
        return ProductServiceImpl(repository=mock_repository)

    # --- find_by_id ---

    @pytest.mark.asyncio
    async def test_should_return_product_when_id_exists(
        self,
        sut: ProductServiceImpl,
        mock_repository: AsyncMock,
    ) -> None:
        # Arrange
        product = Product(
            id="prod-123",
            name="Widget",
            description=None,
            price=Decimal("29.99"),
            created_at=datetime.now(UTC),
        )
        mock_repository.get_by_id.return_value = product

        # Act
        result = await sut.find_by_id("prod-123")

        # Assert
        assert result is not None
        assert result.id == "prod-123"
        assert result.name == "Widget"
        assert result.price == Decimal("29.99")
        mock_repository.get_by_id.assert_awaited_once_with("prod-123")

    @pytest.mark.asyncio
    async def test_should_raise_not_found_when_id_missing(
        self,
        sut: ProductServiceImpl,
        mock_repository: AsyncMock,
    ) -> None:
        # Arrange
        mock_repository.get_by_id.return_value = None

        # Act & Assert
        with pytest.raises(ProductNotFoundError, match="missing"):
            await sut.find_by_id("missing")

    @pytest.mark.asyncio
    async def test_should_raise_value_error_when_id_empty(
        self,
        sut: ProductServiceImpl,
    ) -> None:
        # Act & Assert
        with pytest.raises(ValueError):
            await sut.find_by_id("")

    @pytest.mark.parametrize(
        "product_id",
        [None, "", "   "],
        ids=["none", "empty", "whitespace"],
    )
    @pytest.mark.asyncio
    async def test_should_raise_when_id_invalid(
        self,
        sut: ProductServiceImpl,
        product_id: str | None,
    ) -> None:
        with pytest.raises((ValueError, TypeError)):
            await sut.find_by_id(product_id)

    # --- find_all ---

    @pytest.mark.asyncio
    async def test_should_return_all_products(
        self,
        sut: ProductServiceImpl,
        mock_repository: AsyncMock,
    ) -> None:
        # Arrange
        products = [
            Product(id="1", name="A", price=Decimal("10"), created_at=datetime.now(UTC)),
            Product(id="2", name="B", price=Decimal("20"), created_at=datetime.now(UTC)),
        ]
        mock_repository.get_all.return_value = products

        # Act
        result = await sut.find_all()

        # Assert
        assert len(result) == 2
        assert result[0].name == "A"
        assert result[1].name == "B"

    @pytest.mark.asyncio
    async def test_should_return_empty_list_when_no_products(
        self,
        sut: ProductServiceImpl,
        mock_repository: AsyncMock,
    ) -> None:
        # Arrange
        mock_repository.get_all.return_value = []

        # Act
        result = await sut.find_all()

        # Assert
        assert result == []

    # --- create ---

    @pytest.mark.asyncio
    async def test_should_create_product_when_valid_request(
        self,
        sut: ProductServiceImpl,
        mock_repository: AsyncMock,
    ) -> None:
        # Arrange
        request = CreateProductRequest(
            name="Widget",
            description="A useful widget",
            price=Decimal("29.99"),
        )
        saved_product = Product(
            id="new-id",
            name="Widget",
            description="A useful widget",
            price=Decimal("29.99"),
            created_at=datetime.now(UTC),
        )
        mock_repository.save.return_value = saved_product

        # Act
        result = await sut.create(request)

        # Assert
        assert result is not None
        assert result.id == "new-id"
        assert result.name == "Widget"
        mock_repository.save.assert_awaited_once()

    # --- delete ---

    @pytest.mark.asyncio
    async def test_should_delete_product_when_exists(
        self,
        sut: ProductServiceImpl,
        mock_repository: AsyncMock,
    ) -> None:
        # Arrange
        product = Product(
            id="prod-123",
            name="Widget",
            price=Decimal("10"),
            created_at=datetime.now(UTC),
        )
        mock_repository.get_by_id.return_value = product

        # Act
        await sut.delete("prod-123")

        # Assert
        mock_repository.delete.assert_awaited_once_with("prod-123")

    @pytest.mark.asyncio
    async def test_should_raise_not_found_when_deleting_nonexistent(
        self,
        sut: ProductServiceImpl,
        mock_repository: AsyncMock,
    ) -> None:
        # Arrange
        mock_repository.get_by_id.return_value = None

        # Act & Assert
        with pytest.raises(ProductNotFoundError):
            await sut.delete("missing")
