"""
Product API Load Test — Locust

Run interactively:
    locust -f locustfile.py --host http://localhost:8000
    Open http://localhost:8089 to configure and start.

Run headless (CI/CD):
    locust -f locustfile.py --headless \
        --users 50 --spawn-rate 10 --run-time 2m \
        --host http://localhost:8000 \
        --csv reports/results --html reports/report.html \
        --exit-code-on-error 1
"""

import logging
import random

from locust import HttpUser, LoadTestShape, SequentialTaskSet, between, events, task

logger = logging.getLogger(__name__)

# ---------------------------------------------------------------------------
# Threshold tracking (for CI/CD pass/fail)
# ---------------------------------------------------------------------------

_failure_count = 0
_total_count = 0


@events.request.add_listener
def _on_request(request_type, name, response_time, response_length, exception, **kwargs):
    global _failure_count, _total_count
    _total_count += 1
    if exception:
        _failure_count += 1


@events.quitting.add_listener
def _on_quitting(environment, **kwargs):
    error_rate = (_failure_count / _total_count * 100) if _total_count > 0 else 0
    p95 = environment.stats.total.get_response_time_percentile(0.95) or 0

    logger.info("Results  |  error rate: %.2f%%  |  p95: %.0fms", error_rate, p95)

    if error_rate > 1.0:
        logger.error("FAIL: Error rate %.2f%% exceeds 1%% threshold", error_rate)
        environment.process_exit_code = 1
    if p95 > 500:
        logger.error("FAIL: p95 %.0fms exceeds 500ms threshold", p95)
        environment.process_exit_code = 1


# ---------------------------------------------------------------------------
# Product CRUD tasks (sequential)
# ---------------------------------------------------------------------------


class ProductCrudTasks(SequentialTaskSet):
    """Runs create → read → update → delete in strict order."""

    product_id = None

    @task
    def create_product(self):
        payload = {
            "name": f"Product-{random.randint(1, 100_000)}",
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


# ---------------------------------------------------------------------------
# User classes
# ---------------------------------------------------------------------------


class BrowseUser(HttpUser):
    """Simulates a read-only user browsing products."""

    weight = 3
    wait_time = between(1, 3)

    @task(5)
    def list_products(self):
        with self.client.get(
            "/api/products",
            name="GET /api/products",
            catch_response=True,
        ) as resp:
            if resp.status_code != 200:
                resp.failure(f"Status {resp.status_code}")
            elif resp.elapsed.total_seconds() > 0.5:
                resp.failure(f"Slow: {resp.elapsed.total_seconds():.2f}s")
            else:
                resp.success()

    @task(1)
    def get_single_product(self):
        product_id = random.randint(1, 100)
        self.client.get(
            f"/api/products/{product_id}",
            name="GET /api/products/{id}",
        )


class CrudUser(HttpUser):
    """Simulates a user performing full CRUD operations."""

    weight = 1
    wait_time = between(1, 2)
    tasks = [ProductCrudTasks]


# ---------------------------------------------------------------------------
# Custom load shape (optional — comment out for default Locust ramp)
# ---------------------------------------------------------------------------


class ProductLoadShape(LoadTestShape):
    """
    Step load shape for the product API:
      0-30s  → ramp to 10 users
      30-90s → ramp to 30 users
      90-180s → ramp to 50 users
      180s   → stop
    """

    stages = [
        {"duration": 30, "users": 10, "spawn_rate": 5},
        {"duration": 90, "users": 30, "spawn_rate": 5},
        {"duration": 180, "users": 50, "spawn_rate": 10},
    ]

    def tick(self):
        run_time = self.get_run_time()
        for stage in self.stages:
            if run_time < stage["duration"]:
                return stage["users"], stage["spawn_rate"]
        return None
