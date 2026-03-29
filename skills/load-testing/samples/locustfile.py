"""
Smoke test — Locust

Run interactively:
    locust -f locustfile.py --host http://localhost:5000
    Open http://localhost:8089 to configure and start.

Run headless (CI/CD):
    locust -f locustfile.py --headless \
        --users 5 --spawn-rate 5 --run-time 1m \
        --host http://localhost:5000 \
        --csv reports/results --html reports/report.html \
        --exit-code-on-error 1
"""

import logging

from locust import HttpUser, between, events, task

logger = logging.getLogger(__name__)

# ---------------------------------------------------------------------------
# Threshold enforcement (CI/CD pass/fail via exit code)
# ---------------------------------------------------------------------------

_failure_count = 0
_total_count = 0


@events.request.add_listener
def _on_request(request_type, name, response_time, response_length,
                exception, **kwargs):
    global _failure_count, _total_count
    _total_count += 1
    if exception:
        _failure_count += 1


@events.quitting.add_listener
def _on_quitting(environment, **kwargs):
    error_rate = (_failure_count / _total_count * 100) if _total_count > 0 else 0
    p95 = environment.stats.total.get_response_time_percentile(0.95) or 0

    logger.info("Results  |  error rate: %.2f%%  |  p95: %.0fms",
                error_rate, p95)

    if error_rate > 5.0:
        logger.error("FAIL: Error rate %.2f%% exceeds 5%% threshold",
                      error_rate)
        environment.process_exit_code = 1
    if p95 > 500:
        logger.error("FAIL: p95 %.0fms exceeds 500ms threshold", p95)
        environment.process_exit_code = 1


# ---------------------------------------------------------------------------
# Users
# ---------------------------------------------------------------------------


class SmokeUser(HttpUser):
    """Smoke test user — hits health check and core API endpoints."""

    wait_time = between(1, 2)

    @task
    def health_check(self):
        with self.client.get(
            "/health",
            name="GET /health",
            catch_response=True,
        ) as resp:
            if resp.status_code != 200:
                resp.failure(f"Health check failed: {resp.status_code}")
            else:
                resp.success()

    @task(3)
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

    @task
    def get_single_product(self):
        with self.client.get(
            "/api/products/1",
            name="GET /api/products/{id}",
            catch_response=True,
        ) as resp:
            if resp.status_code != 200:
                resp.failure(f"Status {resp.status_code}")
            elif resp.elapsed.total_seconds() > 0.3:
                resp.failure(f"Slow: {resp.elapsed.total_seconds():.2f}s")
            else:
                resp.success()
