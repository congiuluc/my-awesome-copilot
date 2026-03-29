# Locust Load Testing Patterns

## Basic HttpUser

```python
from locust import HttpUser, task, between


class ApiUser(HttpUser):
    """Simulates a user browsing API endpoints."""

    wait_time = between(1, 3)

    @task(3)
    def list_products(self):
        self.client.get("/api/products", name="GET /api/products")

    @task(1)
    def get_product(self):
        self.client.get("/api/products/1", name="GET /api/products/{id}")
```

## CRUD Flow (Sequential Tasks)

```python
from locust import HttpUser, task, between, SequentialTaskSet
import random


class ProductCrudTasks(SequentialTaskSet):
    """Runs create → read → update → delete in strict order."""

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
        ) as resp:
            if resp.status_code == 201:
                self.product_id = resp.json().get("id")
                resp.success()
            else:
                resp.failure(f"Expected 201, got {resp.status_code}")

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
            self.client.put(
                f"/api/products/{self.product_id}",
                json={"name": "Updated Product", "price": 29.99},
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
    """Logs in once and uses the token for all subsequent requests."""

    wait_time = between(1, 3)
    token = None

    def on_start(self):
        resp = self.client.post(
            "/api/auth/login",
            json={"email": "test@example.com", "password": "Test123!"},
            name="POST /api/auth/login",
        )
        if resp.status_code == 200:
            self.token = resp.json().get("token")

    @task
    def get_protected_resource(self):
        if self.token:
            self.client.get(
                "/api/products",
                headers={"Authorization": f"Bearer {self.token}"},
                name="GET /api/products (auth)",
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
        ) as resp:
            if resp.status_code != 200:
                resp.failure(f"Status {resp.status_code}")
            elif resp.elapsed.total_seconds() > 0.5:
                resp.failure(f"Too slow: {resp.elapsed.total_seconds():.2f}s")
            elif not resp.json():
                resp.failure("Empty response body")
            else:
                resp.success()
```

## Event Hooks (Threshold Enforcement for CI/CD)

```python
from locust import events
import logging

logger = logging.getLogger(__name__)

failure_count = 0
total_count = 0


@events.request.add_listener
def on_request(request_type, name, response_time, response_length,
               exception, **kwargs):
    global failure_count, total_count
    total_count += 1
    if exception:
        failure_count += 1


@events.quitting.add_listener
def on_quitting(environment, **kwargs):
    error_rate = (failure_count / total_count * 100) if total_count > 0 else 0
    p95 = environment.stats.total.get_response_time_percentile(0.95) or 0

    logger.info("Results  |  error rate: %.2f%%  |  p95: %.0fms",
                error_rate, p95)

    if error_rate > 1.0:
        logger.error("FAIL: Error rate %.2f%% exceeds 1%% threshold",
                      error_rate)
        environment.process_exit_code = 1
    if p95 > 500:
        logger.error("FAIL: p95 %.0fms exceeds 500ms threshold", p95)
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
        return None
```

### Spike Load Shape

```python
from locust import LoadTestShape


class SpikeLoadShape(LoadTestShape):
    """Simulates a sudden traffic spike and recovery."""

    stages = [
        {"duration": 30, "users": 10, "spawn_rate": 5},
        {"duration": 60, "users": 200, "spawn_rate": 100},
        {"duration": 90, "users": 10, "spawn_rate": 50},
        {"duration": 150, "users": 10, "spawn_rate": 5},
    ]

    def tick(self):
        run_time = self.get_run_time()
        for stage in self.stages:
            if run_time < stage["duration"]:
                return stage["users"], stage["spawn_rate"]
        return None
```

## Distributed Mode

```bash
# Start master (coordinates workers, hosts Web UI)
locust --master -f locustfile.py

# Start workers (can run on different machines)
locust --worker --master-host=<master-ip> -f locustfile.py

# Headless distributed (CI/CD)
locust --master --headless --expect-workers 4 -u 200 -r 20 -t 5m
```

## Configuration File (locust.conf)

```ini
locustfile = tests/load/locust/locustfile.py
host = http://localhost:5000
users = 50
spawn-rate = 10
run-time = 2m
headless = false
csv = reports/results
html = reports/report.html
```
