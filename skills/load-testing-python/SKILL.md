---
name: load-testing-python
description: >-
  Load test Python FastAPI/Flask APIs using Locust (Python-native) and k6. Covers
  HttpUser design, task sets, custom load shapes, distributed mode, event hooks,
  pip/Poetry setup, and CI/CD pipelines. Use when: writing load tests in Python with
  Locust, testing FastAPI or Flask endpoints under load, running distributed load
  tests, or comparing Locust vs k6 for a Python project.
argument-hint: 'Describe the Python API endpoint or scenario to load test.'
---

# Load Testing for Python (Locust + k6)

## When to Use

- Writing load tests co-located with Python projects
- Testing FastAPI or Flask REST API endpoints under realistic load
- Need a pure-Python test framework that developers already know
- Need distributed load generation across multiple workers
- Need custom load shapes (ramp, spike, step) defined in Python
- Already using k6 but need supplementary Python-native tests

## Official Documentation

- [Locust Documentation](https://docs.locust.io/en/stable/)
- [Locust Quickstart](https://docs.locust.io/en/stable/quickstart.html)
- [Locust Configuration](https://docs.locust.io/en/stable/configuration.html)
- [Locust Distributed](https://docs.locust.io/en/stable/running-distributed.html)
- [k6 Documentation](https://grafana.com/docs/k6/latest/) (see `load-testing` skill)

## When to Use Locust vs k6

| Criteria | Locust | k6 |
|----------|--------|-----|
| Language | Python | JavaScript |
| Best for | Python teams, custom shapes | Cross-team, CI/CD gates |
| Protocol | HTTP (native), custom via plugins | HTTP, WebSocket, gRPC |
| Distributed | Built-in master/worker | k6 Cloud or xk6-distributed |
| Reports | Web UI (real-time), CSV, JSON | Console, JSON, Prometheus |
| Load shapes | Python classes (full control) | Scenarios + executors |
| Data feeds | Python generators, CSV, DB | SharedArray, JSON |

**Recommendation**: Use Locust for Python teams that want pure-Python test code,
real-time Web UI, and custom load shapes. Use k6 for quick CI/CD pipeline smoke tests.

## Procedure

1. Install Locust: `pip install locust`
2. Follow [Locust patterns](./references/locust-patterns.md)
3. Review [product load test sample](./samples/locustfile.py)
4. Design users with tasks, waits, and event hooks
5. Run locally: `locust -f locustfile.py --host http://localhost:8000`
6. Open Web UI at `http://localhost:8089` to configure and start test
7. For headless CI/CD mode: `locust --headless -u 50 -r 10 -t 2m`
8. For CI/CD pipeline integration, use k6 (see `load-testing` skill) or headless Locust
9. Export results as CSV or JSON for analysis

## Installation

### pip

```bash
pip install locust
```

### Poetry

```bash
poetry add --group dev locust
```

### Requirements File

```
# requirements-loadtest.txt
locust>=2.29
```

## Project Structure

```
tests/
  load/
    locustfile.py               # Main Locust test file
    users/
      product_user.py           # Product API user
      auth_user.py              # Authenticated user flow
    shapes/
      step_load.py              # Custom step load shape
      spike_load.py             # Custom spike load shape
    data/
      products.csv              # Test data feeder
    locust.conf                 # Default Locust configuration
```

## Locust Configuration File

```ini
# locust.conf
locustfile = tests/load/locustfile.py
host = http://localhost:8000
users = 50
spawn-rate = 10
run-time = 2m
headless = false
csv = reports/results
html = reports/report.html
```

## Headless Mode (CI/CD)

```bash
locust \
  --headless \
  --users 50 \
  --spawn-rate 10 \
  --run-time 2m \
  --host http://localhost:8000 \
  --csv reports/results \
  --html reports/report.html \
  --exit-code-on-error 1
```

## Distributed Mode

```bash
# Start master
locust --master

# Start workers (on same or different machines)
locust --worker --master-host=<master-ip>
```

## CI/CD Integration (GitHub Actions)

```yaml
- name: Install Locust
  run: pip install locust

- name: Run load tests (headless)
  run: |
    locust \
      --headless \
      --users 20 \
      --spawn-rate 5 \
      --run-time 1m \
      --host ${{ env.API_URL }} \
      --csv reports/results \
      --html reports/report.html \
      --exit-code-on-error 1

- name: Upload Locust report
  uses: actions/upload-artifact@v4
  if: always()
  with:
    name: locust-report
    path: reports/
```
