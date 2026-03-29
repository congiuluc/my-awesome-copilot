# Sample: FastAPI Router with Clean Architecture
# This shows the complete pattern for a feature router.

"""Product router module.

Defines FastAPI endpoints for product CRUD operations.
"""

from __future__ import annotations

from typing import Annotated

from fastapi import APIRouter, Depends, status

from app.api.deps import get_product_service
from app.api.schemas.common import ApiResponse
from app.api.schemas.product import (
    CreateProductRequest,
    ProductResponse,
    UpdateProductRequest,
)
from app.application.interfaces.product_service import ProductServiceProtocol

router = APIRouter(prefix="/api/products", tags=["Product"])

ProductServiceDep = Annotated[ProductServiceProtocol, Depends(get_product_service)]


@router.get(
    "/",
    response_model=ApiResponse[list[ProductResponse]],
    summary="Get all products",
)
async def get_all(service: ProductServiceDep) -> ApiResponse[list[ProductResponse]]:
    """Retrieve all products."""
    products = await service.find_all()
    return ApiResponse.ok(products)


@router.get(
    "/{product_id}",
    response_model=ApiResponse[ProductResponse],
    summary="Get a product by ID",
    responses={404: {"model": ApiResponse[None]}},
)
async def get_by_id(
    product_id: str,
    service: ProductServiceDep,
) -> ApiResponse[ProductResponse]:
    """Retrieve a product by its unique identifier."""
    product = await service.find_by_id(product_id)
    return ApiResponse.ok(product)


@router.post(
    "/",
    response_model=ApiResponse[ProductResponse],
    status_code=status.HTTP_201_CREATED,
    summary="Create a new product",
    responses={400: {"model": ApiResponse[None]}},
)
async def create(
    request: CreateProductRequest,
    service: ProductServiceDep,
) -> ApiResponse[ProductResponse]:
    """Create a new product."""
    product = await service.create(request)
    return ApiResponse.ok(product)


@router.put(
    "/{product_id}",
    response_model=ApiResponse[ProductResponse],
    summary="Update an existing product",
    responses={404: {"model": ApiResponse[None]}},
)
async def update(
    product_id: str,
    request: UpdateProductRequest,
    service: ProductServiceDep,
) -> ApiResponse[ProductResponse]:
    """Update an existing product."""
    product = await service.update(product_id, request)
    return ApiResponse.ok(product)


@router.delete(
    "/{product_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Delete a product",
    responses={404: {"model": ApiResponse[None]}},
)
async def delete(
    product_id: str,
    service: ProductServiceDep,
) -> None:
    """Delete a product by its unique identifier."""
    await service.delete(product_id)


# --- Schemas (in api/schemas/common.py) ---

from pydantic import BaseModel
from typing import Generic, TypeVar

T = TypeVar("T")


class ApiResponse(BaseModel, Generic[T]):
    """Standard API response envelope."""

    success: bool
    data: T | None = None
    error: str | None = None

    @classmethod
    def ok(cls, data: T) -> "ApiResponse[T]":
        return cls(success=True, data=data)

    @classmethod
    def fail(cls, error: str) -> "ApiResponse[None]":
        return cls(success=False, error=error)


# --- Schemas (in api/schemas/product.py) ---

from decimal import Decimal
from datetime import datetime
from pydantic import BaseModel, Field, ConfigDict


class CreateProductRequest(BaseModel):
    """Request to create a new product."""

    name: str = Field(..., min_length=1, max_length=100, description="Product name")
    description: str | None = Field(None, max_length=500, description="Optional description")
    price: Decimal = Field(..., gt=0, description="Product price")


class UpdateProductRequest(BaseModel):
    """Request to update an existing product."""

    name: str = Field(..., min_length=1, max_length=100, description="Product name")
    description: str | None = Field(None, max_length=500, description="Optional description")
    price: Decimal = Field(..., gt=0, description="Product price")


class ProductResponse(BaseModel):
    """Product data returned to API consumers."""

    id: str
    name: str
    description: str | None
    price: Decimal
    created_at: datetime

    model_config = ConfigDict(from_attributes=True)


# --- Service Protocol (in application/interfaces/product_service.py) ---

from typing import Protocol


class ProductServiceProtocol(Protocol):
    """Protocol for product business logic."""

    async def find_all(self) -> list[ProductResponse]: ...
    async def find_by_id(self, product_id: str) -> ProductResponse: ...
    async def create(self, request: CreateProductRequest) -> ProductResponse: ...
    async def update(self, product_id: str, request: UpdateProductRequest) -> ProductResponse: ...
    async def delete(self, product_id: str) -> None: ...


# --- Service Implementation (in application/services/product_service.py) ---

import structlog

from app.application.interfaces.product_repository import ProductRepositoryProtocol
from app.domain.exceptions import ProductNotFoundError
from app.domain.models.product import Product

log = structlog.get_logger()


class ProductServiceImpl:
    """Product service implementation using repository pattern."""

    def __init__(self, repository: ProductRepositoryProtocol) -> None:
        self._repository = repository

    async def find_all(self) -> list[ProductResponse]:
        """Retrieve all products."""
        log.debug("fetching_all_products")
        products = await self._repository.get_all()
        return [ProductResponse.model_validate(p) for p in products]

    async def find_by_id(self, product_id: str) -> ProductResponse:
        """Retrieve a product by its unique identifier.

        Args:
            product_id: The product identifier.

        Returns:
            The product response.

        Raises:
            ProductNotFoundError: If no product exists with the given id.
        """
        if not product_id:
            raise ValueError("product_id must not be empty")
        log.debug("fetching_product", product_id=product_id)
        product = await self._repository.get_by_id(product_id)
        if product is None:
            raise ProductNotFoundError(product_id)
        return ProductResponse.model_validate(product)

    async def create(self, request: CreateProductRequest) -> ProductResponse:
        """Create a new product."""
        log.info("creating_product", name=request.name)
        product = Product(
            name=request.name,
            description=request.description,
            price=request.price,
        )
        saved = await self._repository.save(product)
        return ProductResponse.model_validate(saved)

    async def update(self, product_id: str, request: UpdateProductRequest) -> ProductResponse:
        """Update an existing product."""
        log.info("updating_product", product_id=product_id)
        product = await self._repository.get_by_id(product_id)
        if product is None:
            raise ProductNotFoundError(product_id)
        product.name = request.name
        product.description = request.description
        product.price = request.price
        saved = await self._repository.save(product)
        return ProductResponse.model_validate(saved)

    async def delete(self, product_id: str) -> None:
        """Delete a product by its unique identifier."""
        log.info("deleting_product", product_id=product_id)
        product = await self._repository.get_by_id(product_id)
        if product is None:
            raise ProductNotFoundError(product_id)
        await self._repository.delete(product_id)
