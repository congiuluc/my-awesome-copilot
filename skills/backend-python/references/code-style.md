# Python Code Style

## Naming Conventions

| Element | Style | Example |
|---------|-------|---------|
| Module / Package | snake_case | `product_service.py` |
| Class | PascalCase | `ProductService` |
| Function / Method | snake_case | `find_by_id` |
| Variable | snake_case | `product_dto` |
| Private attribute | _snake_case | `_repository` |
| Constant | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT` |
| Type alias | PascalCase | `ProductId = str` |
| Pydantic model | PascalCase | `CreateProductRequest` |

## File Organization

```python
"""Product router module.

Defines FastAPI endpoints for product management.
"""

from __future__ import annotations

# 1. Standard library imports
from typing import Annotated

# 2. Third-party imports
from fastapi import APIRouter, Depends, HTTPException, status

# 3. Local imports
from app.api.deps import get_product_service
from app.api.schemas.product import (
    CreateProductRequest,
    ProductResponse,
)
from app.application.interfaces.product_service import ProductServiceProtocol


# 4. Module-level constants
router = APIRouter(prefix="/api/products", tags=["Product"])


# 5. Functions / Classes
```

## Rules

- Google-style docstrings on all public classes, functions, and non-trivial methods.
- Max line length: 120 characters.
- Use **type hints** on all function signatures, parameters, and return types.
- Use `from __future__ import annotations` for forward references.
- Use `Annotated` for FastAPI dependency injection types.
- Use **Pydantic models** for all request/response schemas — never raw dicts.
- Use **Protocol** classes for interface definitions — prefer over ABC where possible.
- Use `snake_case` everywhere except classes and type aliases.
- One class per file for major components (models, services, repositories).
- Use f-strings for string formatting — never `%` or `.format()`.
- Use `pathlib.Path` instead of `os.path` for file operations.
- Use `datetime.UTC` (Python 3.11+) instead of `datetime.timezone.utc`.
- Prefer `list[str]` over `List[str]` (Python 3.9+ built-in generics).

## Function Style

```python
async def find_by_id(self, product_id: str) -> Product:
    """Retrieve a product by its unique identifier.

    Args:
        product_id: The product identifier.

    Returns:
        The product domain model.

    Raises:
        ProductNotFoundError: If no product exists with the given id.
    """
    if not product_id:
        raise ValueError("product_id must not be empty")
    product = await self._repository.get_by_id(product_id)
    if product is None:
        raise ProductNotFoundError(product_id)
    return product
```

## Pydantic Model / DTO Style

```python
from pydantic import BaseModel, Field, ConfigDict


class CreateProductRequest(BaseModel):
    """Request to create a new product."""

    name: str = Field(..., min_length=1, max_length=100, description="Product name")
    description: str | None = Field(None, max_length=500, description="Optional description")
    price: Decimal = Field(..., gt=0, description="Product price")

    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "name": "Widget",
                "description": "A useful widget",
                "price": "29.99",
            }
        }
    )


class ProductResponse(BaseModel):
    """Product data returned to API consumers."""

    id: str
    name: str
    description: str | None
    price: Decimal
    created_at: datetime

    model_config = ConfigDict(from_attributes=True)
```

## Exception Handling Style

```python
from fastapi import Request
from fastapi.responses import JSONResponse


class AppException(Exception):
    """Base exception for application errors."""

    def __init__(self, message: str, status_code: int = 500) -> None:
        self.message = message
        self.status_code = status_code
        super().__init__(message)


class ProductNotFoundError(AppException):
    """Raised when a product is not found."""

    def __init__(self, product_id: str) -> None:
        super().__init__(
            message=f"Product with id '{product_id}' not found",
            status_code=404,
        )


async def app_exception_handler(request: Request, exc: AppException) -> JSONResponse:
    """Global exception handler for application exceptions."""
    return JSONResponse(
        status_code=exc.status_code,
        content={"success": False, "data": None, "error": exc.message},
    )
```

## Import Ordering

Follow isort conventions (compatible with ruff):

1. Standard library (`import os`, `from typing import ...`)
2. Third-party (`import fastapi`, `import pydantic`)
3. Local application (`from app.domain import ...`)

Separate each group with a blank line.
