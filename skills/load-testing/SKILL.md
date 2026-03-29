---
name: load-testing
description: >-
  Design and run load tests with k6 and Locust for any backend stack. Covers scenario
  design, thresholds, ramp-up strategies, CI/CD pipeline integration, result analysis,
  and SLA validation. Use when: creating load test scripts, defining performance SLAs,
  integrating load tests into CI/CD, analyzing load test results, or capacity planning.
argument-hint: 'Describe the API or scenario to load test and the target SLAs.'
---

# Load Testing with k6 and Locust

## When to Use

- Creating load test scripts for API endpoints
- Defining performance thresholds and SLAs (p95 latency, error rate, throughput)
- Running smoke, load, stress, or soak tests
- Integrating load tests into CI/CD pipelines (GitHub Actions)
- Analyzing load test results and identifying bottlenecks
- Capacity planning and scaling validation
- Testing before production deployments or after major changes

## Official Documentation

### k6
- [k6 Documentation](https://grafana.com/docs/k6/latest/)
- [k6 Scenarios](https://grafana.com/docs/k6/latest/using-k6/scenarios/)
- [k6 Thresholds](https://grafana.com/docs/k6/latest/using-k6/thresholds/)
- [k6 Checks](https://grafana.com/docs/k6/latest/using-k6/checks/)
- [k6 in CI/CD](https://grafana.com/docs/k6/latest/testing-guides/automated-performance-testing/)
- [k6 GitHub Action](https://github.com/grafana/k6-action)

### Locust
- [Locust Documentation](https://docs.locust.io/en/stable/)
- [Locust Quickstart](https://docs.locust.io/en/stable/quickstart.html)
- [Locust Configuration](https://docs.locust.io/en/stable/configuration.html)
- [Locust Distributed Mode](https://docs.locust.io/en/stable/running-distributed.html)
- [Locust Custom Load Shapes](https://docs.locust.io/en/stable/custom-load-shape.html)

## Tool Comparison: k6 vs Locust

| Criteria | k6 | Locust |
|----------|-----|--------|
| Language | JavaScript / TypeScript | Python |
| Best for | CI/CD gates, cross-team | Python teams, custom shapes |
| Protocol | HTTP, WebSocket, gRPC, browser | HTTP (native), custom via plugins |
| Distributed | k6 Cloud or xk6-distributed | Built-in master/worker |
| Reports | Console, JSON, Prometheus | Real-time Web UI, CSV, HTML |
| Load shapes | Scenarios + executors | Python classes (full control) |
| CI/CD | GitHub Action, CLI threshold gates | Headless mode with exit codes |
| Data feeds | SharedArray, JSON | Python generators, CSV, DB |
| Installation | Single binary (no runtime) | `pip install locust` |

**When to choose k6**: CI/CD pipeline gates with threshold-based pass/fail, teams
comfortable with JavaScript, need gRPC/WebSocket/browser protocol support.

**When to choose Locust**: Python teams, need real-time Web UI monitoring, need
custom load shapes defined in code, need built-in distributed mode across workers.

## Why k6

k6 is the recommended cross-platform load testing tool because:
- **JavaScript/TypeScript** test scripts — familiar to all developers
- **CLI-first** — runs anywhere (local, CI/CD, Docker)
- **Built-in metrics** — p95, p99, error rate, throughput, iteration duration
- **Threshold-based pass/fail** — CI/CD pipeline gate
- **Protocol support** — HTTP, WebSocket, gRPC, browser
- **Extensions** — output to Prometheus, Datadog, InfluxDB, JSON

## Why Locust

Locust is the recommended Python-native load testing tool because:
- **Pure Python** test scripts — no new language to learn for Python teams
- **Real-time Web UI** — monitor tests live at `http://localhost:8089`
- **Custom load shapes** — define any ramp pattern as a Python class
- **Built-in distributed mode** — scale across multiple workers natively
- **Lightweight** — each user is a greenlet, thousands of concurrent users per process
- **Extensible** — custom event hooks, listeners, and protocol plugins

For language-native alternatives, see the language-specific load testing skills:
- `load-testing-dotnet` — NBomber for .NET-native load tests
- `load-testing-java` — Gatling for JVM-native load tests
- `load-testing-python` — Locust for Python-native load tests (deep-dive patterns)

## Procedure

### k6 Workflow
1. Identify the endpoints and user flows to test
2. Choose the test type (see [Test Types](#test-types))
3. Define thresholds from SLAs (see [Threshold Patterns](#threshold-patterns))
4. Write k6 scripts following [k6 script patterns](./references/k6-patterns.md)
5. Review the [k6 smoke test sample](./samples/smoke-test.js)
6. Run locally first: `k6 run script.js`
7. Integrate into CI/CD (see [CI/CD Integration](#cicd-integration))
8. Analyze results and iterate

### Locust Workflow
1. Identify the endpoints and user flows to test
2. Choose the test type (see [Test Types](#test-types))
3. Define threshold checks via event hooks
4. Write Locust users following [Locust patterns](./references/locust-patterns.md)
5. Review the [Locust sample](./samples/locustfile.py)
6. Run locally: `locust -f locustfile.py --host http://localhost:5000`
7. Open Web UI at `http://localhost:8089` to configure and start
8. For headless CI/CD: `locust --headless -u 50 -r 10 -t 2m --exit-code-on-error 1`
9. Analyze results (Web UI, CSV, or HTML report) and iterate

## Test Types

| Type | Purpose | VUs | Duration | When |
|------|---------|-----|----------|------|
| **Smoke** | Verify script works | 1–5 | 1 min | Every PR |
| **Load** | Validate SLAs under normal load | 50–200 | 5–15 min | Pre-deploy |
| **Stress** | Find breaking point | Ramp to 500+ | 10–20 min | Release milestones |
| **Soak** | Find memory leaks / degradation | 50–100 | 1–4 hours | Quarterly |
| **Spike** | Test sudden traffic bursts | 0→500→0 | 5 min | Event preparation |

## Threshold Patterns

Define thresholds based on SLA requirements:

```javascript
export const options = {
  thresholds: {
    http_req_duration: ['p(95)<200', 'p(99)<500'],  // 95th percentile < 200ms
    http_req_failed: ['rate<0.01'],                   // Error rate < 1%
    http_reqs: ['rate>100'],                          // Throughput > 100 rps
    iteration_duration: ['p(95)<1000'],               // Full scenario < 1s
  },
};
```

| Metric | Typical SLA | Strict SLA |
|--------|------------|------------|
| `http_req_duration` p95 | < 500ms | < 200ms |
| `http_req_duration` p99 | < 1000ms | < 500ms |
| `http_req_failed` rate | < 5% | < 1% |
| `http_reqs` rate | > 50 rps | > 200 rps |

## CI/CD Integration

### GitHub Actions — k6

```yaml
- name: Run k6 smoke test
  uses: grafana/k6-action@v0.3.1
  with:
    filename: tests/load/smoke.js
    flags: --out json=results.json

- name: Run k6 load test (pre-deploy gate)
  uses: grafana/k6-action@v0.3.1
  with:
    filename: tests/load/load-test.js
    flags: --out json=results.json
  env:
    BASE_URL: ${{ env.STAGING_URL }}
```

### GitHub Actions — Locust

```yaml
- name: Install Locust
  run: pip install locust

- name: Run Locust smoke test
  run: |
    locust \
      -f tests/load/locustfile.py \
      --headless \
      --users 5 \
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

### Pipeline Strategy

| Pipeline Stage | k6 | Locust | Gate |
|---------------|-----|--------|------|
| PR validation | Smoke test (1–5 VUs, 1 min) | Headless (5 users, 1 min) | Must pass |
| Staging deploy | Load test (50–200 VUs, 5 min) | Step shape (10→50 users, 3 min) | Thresholds must pass |
| Pre-production | Stress test (ramp to 500+ VUs) | Spike shape (200 users burst) | Report only |

## Project Structure

### k6

```
tests/
  load/
    k6/
      smoke.js              # Quick validation (CI on every PR)
      load-test.js          # Standard load test (CI pre-deploy)
      stress-test.js        # Stress / breaking point test
      soak-test.js          # Long-running stability test
      helpers/
        auth.js             # Token generation / login helper
        data.js             # Test data generators
        config.js           # Environment-aware base URL, thresholds
```

### Locust

```
tests/
  load/
    locust/
      locustfile.py         # Main entry point (CI + interactive)
      users/
        browse_user.py      # Read-only browsing user
        crud_user.py        # CRUD operation user
        auth_user.py        # Authenticated user flow
      shapes/
        step_load.py        # Step ramp load shape
        spike_load.py       # Spike / burst load shape
      data/
        products.csv        # Test data feeder
      locust.conf           # Default configuration
```

## Result Analysis Checklist

| # | Check | Action if Failed |
|---|-------|-----------------|
| 1 | p95 latency within threshold | Profile slow endpoints, check DB queries |
| 2 | Error rate within threshold | Check error logs, identify failing endpoints |
| 3 | Throughput meets target | Scale horizontally or optimize bottleneck |
| 4 | No memory growth over time | Check for leaks, connection pool exhaustion |
| 5 | Consistent latency under load | Check connection pooling, caching, indexes |
| 6 | Graceful degradation at limits | Verify rate limiting, circuit breakers |
