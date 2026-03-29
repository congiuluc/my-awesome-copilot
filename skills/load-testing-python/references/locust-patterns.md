# Locust Patterns for Python Load Testing

## Basic HttpUser

```python
from locust import HttpUser, task, between


class ProductUser(HttpUser):
    """Simulates a user browsing and managing products."""

    wait_time = between(1, 3)

    @task(3)
    def list_products(self):
        self.client.get("/api/products", name="GET /api/products")

    @task(1)
    def get_product(self):
        self.client.get("/api/products/1", name="GET /api/products/{id}")
```

## CRUD Flow with Sequential Tasks

```python
from locust import HttpUser, task, between, SequentialTaskSet
import json
import random


class ProductCrudTasks(SequentialTaskSet):
    """Runs create → read → update → delete in order."""

    product_id = None

    @task
    def create_product(self):
        payload = {
            "name": f"Product-{random.randint(1, 100000)}",
            "price": round(random.uniform(5.0, 100.0), 2),
        }
        with self.client.post(
            "/api/products",
            json=payload,
            name="POST /api/products",
            catch_response=True,
        ) as response:
            if response.status_code == 201:
                self.product_id = response.json().get("id")
                response.success()
            else:
                response.failure(f"Expected 201, got {response.status_code}")

    @task
    def read_product(self):
        if self.product_id:
            self.client.get(
                f"/api/products/{self.product_id}",
                name="GET /api/products/{id}",
            )

    @task
    def update_product(self):
        if self.product_id:
            payload = {"name": "Updated Product", "price": 29.99}
            self.client.put(
                f"/api/products/{self.product_id}",
                json=payload,
                name="PUT /api/products/{id}",
            )

    @task
    def delete_product(self):
        if self.product_id:
            self.client.delete(
                f"/api/products/{self.product_id}",
                name="DELETE /api/products/{id}",
            )
            self.product_id = None


class CrudUser(HttpUser):
    wait_time = between(1, 2)
    tasks = [ProductCrudTasks]
```

## Authentication Flow

```python
from locust import HttpUser, task, between


class AuthenticatedUser(HttpUser):
    """Logs in once and uses the token for subsequent requests."""

    wait_time = between(1, 3)
    token = None

    def on_start(self):
        """Called once per user at the start of the test."""
        response = self.client.post(
            "/api/auth/login",
            json={"email": "test@example.com", "password": "Test123!"},
            name="POST /api/auth/login",
        )
        if response.status_code == 200:
            self.token = response.json().get("token")

    @task
    def get_protected_resource(self):
        if self.token:
            self.client.get(
                "/api/products",
                headers={"Authorization": f"Bearer {self.token}"},
                name="GET /api/products (auth)",
            )
```

## CSV Data Feeder

```python
import csv
from locust import HttpUser, task, between


class DataDrivenUser(HttpUser):
    wait_time = between(1, 2)

    def on_start(self):
        with open("tests/load/data/products.csv") as f:
            reader = csv.DictReader(f)
            self.products = list(reader)
        self.index = 0

    @task
    def create_product_from_csv(self):
        product = self.products[self.index % len(self.products)]
        self.index += 1
        self.client.post(
            "/api/products",
            json={"name": product["name"], "price": float(product["price"])},
            name="POST /api/products (csv)",
        )
```

## Custom Response Validation

```python
from locust import HttpUser, task, between


class ValidatingUser(HttpUser):
    wait_time = between(1, 2)

    @task
    def get_products_with_validation(self):
        with self.client.get(
            "/api/products",
            name="GET /api/products (validated)",
            catch_response=True,
        ) as response:
            if response.status_code != 200:
                response.failure(f"Status {response.status_code}")
            elif response.elapsed.total_seconds() > 0.5:
                response.failure(
                    f"Too slow: {response.elapsed.total_seconds():.2f}s"
                )
            elif not response.json():
                response.failure("Empty response body")
            else:
                response.success()
```

## Event Hooks (Custom Metrics / Thresholds)

```python
from locust import events
import logging

logger = logging.getLogger(__name__)

# Track failures for CI/CD exit code
failure_count = 0
total_count = 0


@events.request.add_listener
def on_request(request_type, name, response_time, response_length, exception, **kwargs):
    global failure_count, total_count
    total_count += 1
    if exception:
        failure_count += 1


@events.quitting.add_listener
def on_quitting(environment, **kwargs):
    error_rate = (failure_count / total_count * 100) if total_count > 0 else 0
    p95 = environment.stats.total.get_response_time_percentile(0.95) or 0

    logger.info(f"Error rate: {error_rate:.2f}%  |  p95: {p95:.0f}ms")

    if error_rate > 1.0:
        logger.error(f"FAIL: Error rate {error_rate:.2f}% exceeds 1% threshold")
        environment.process_exit_code = 1
    if p95 > 500:
        logger.error(f"FAIL: p95 {p95:.0f}ms exceeds 500ms threshold")
        environment.process_exit_code = 1
```

## Custom Load Shapes

### Step Load Shape

```python
from locust import LoadTestShape


class StepLoadShape(LoadTestShape):
    """Increases users in steps: 10 → 20 → 50 → 100."""

    stages = [
        {"duration": 60, "users": 10, "spawn_rate": 5},
        {"duration": 120, "users": 20, "spawn_rate": 5},
        {"duration": 180, "users": 50, "spawn_rate": 10},
        {"duration": 300, "users": 100, "spawn_rate": 10},
    ]

    def tick(self):
        run_time = self.get_run_time()
        for stage in self.stages:
            if run_time < stage["duration"]:
                return stage["users"], stage["spawn_rate"]
        return None  # Stop the test
```

### Spike Load Shape

```python
from locust import LoadTestShape


class SpikeLoadShape(LoadTestShape):
    """Simulates a sudden traffic spike and recovery."""

    stages = [
        {"duration": 30, "users": 10, "spawn_rate": 5},    # Warm-up
        {"duration": 60, "users": 200, "spawn_rate": 100},  # Spike
        {"duration": 90, "users": 10, "spawn_rate": 50},    # Recovery
        {"duration": 150, "users": 10, "spawn_rate": 5},    # Steady
    ]

    def tick(self):
        run_time = self.get_run_time()
        for stage in self.stages:
            if run_time < stage["duration"]:
                return stage["users"], stage["spawn_rate"]
        return None
```

## Distributed Mode Configuration

```python
# No code changes needed — Locust handles distribution automatically.
# Master distributes users across workers; workers report stats back.
#
#   Master: locust --master -f locustfile.py
#   Worker: locust --worker --master-host=<master-ip> -f locustfile.py
#
# Each worker runs a subset of users. The Web UI shows aggregated stats.
# For CI/CD, add --headless --expect-workers N to the master command.
```
