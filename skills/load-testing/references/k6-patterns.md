# k6 Load Testing Patterns

## Basic Script Structure

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

// Configuration
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export const options = {
  stages: [
    { duration: '1m', target: 50 },   // Ramp up
    { duration: '3m', target: 50 },   // Steady state
    { duration: '1m', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<200'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const res = http.get(`${BASE_URL}/api/products`);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 200ms': (r) => r.timings.duration < 200,
    'has data': (r) => JSON.parse(r.body).data !== undefined,
  });

  sleep(1); // Think time between requests
}
```

## Authentication Pattern

```javascript
// helpers/auth.js
import http from 'k6/http';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export function getAuthToken(username, password) {
  const res = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
    username,
    password,
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  return JSON.parse(res.body).data.token;
}

export function authHeaders(token) {
  return {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  };
}
```

```javascript
// In test script
import { getAuthToken, authHeaders } from './helpers/auth.js';

export function setup() {
  const token = getAuthToken('loadtest@example.com', 'TestPassword123!');
  return { token };
}

export default function (data) {
  const res = http.get(`${BASE_URL}/api/products`, authHeaders(data.token));
  // ...checks
}
```

## CRUD Scenario Pattern

```javascript
import http from 'k6/http';
import { check, group, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  let productId;

  group('Create product', () => {
    const res = http.post(`${BASE_URL}/api/products`, JSON.stringify({
      name: `Load Test Product ${Date.now()}`,
      price: 29.99,
    }), { headers: { 'Content-Type': 'application/json' } });

    check(res, { 'created': (r) => r.status === 201 });
    productId = JSON.parse(res.body).data.id;
  });

  sleep(1);

  group('Read product', () => {
    const res = http.get(`${BASE_URL}/api/products/${productId}`);
    check(res, { 'found': (r) => r.status === 200 });
  });

  sleep(1);

  group('Update product', () => {
    const res = http.put(`${BASE_URL}/api/products/${productId}`, JSON.stringify({
      name: 'Updated Product',
      price: 39.99,
    }), { headers: { 'Content-Type': 'application/json' } });

    check(res, { 'updated': (r) => r.status === 200 });
  });

  sleep(1);

  group('Delete product', () => {
    const res = http.del(`${BASE_URL}/api/products/${productId}`);
    check(res, { 'deleted': (r) => r.status === 204 || r.status === 200 });
  });
}
```

## Environment-Aware Config

```javascript
// helpers/config.js
export const config = {
  baseUrl: __ENV.BASE_URL || 'http://localhost:5000',
  thresholds: {
    p95: parseInt(__ENV.THRESHOLD_P95 || '200'),
    errorRate: parseFloat(__ENV.THRESHOLD_ERROR_RATE || '0.01'),
  },
};
```

## Smoke Test (CI on every PR)

```javascript
export const options = {
  vus: 3,
  duration: '1m',
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.05'],
  },
};
```

## Stress Test (Find Breaking Point)

```javascript
export const options = {
  stages: [
    { duration: '2m', target: 100 },
    { duration: '2m', target: 200 },
    { duration: '2m', target: 300 },
    { duration: '2m', target: 500 },
    { duration: '2m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000'],  // Relaxed for stress
    http_req_failed: ['rate<0.10'],
  },
};
```

## Spike Test

```javascript
export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '10s', target: 500 },  // Sudden spike
    { duration: '2m', target: 500 },
    { duration: '10s', target: 10 },   // Drop back
    { duration: '1m', target: 10 },
  ],
};
```

## Custom Metrics

```javascript
import { Trend, Counter, Rate } from 'k6/metrics';

const productListDuration = new Trend('product_list_duration');
const productCreated = new Counter('products_created');
const productCreateSuccess = new Rate('product_create_success');

export default function () {
  const res = http.get(`${BASE_URL}/api/products`);
  productListDuration.add(res.timings.duration);

  const createRes = http.post(`${BASE_URL}/api/products`, /*...*/);
  productCreated.add(1);
  productCreateSuccess.add(createRes.status === 201);
}
```

## JSON Output for CI

```bash
# Run with JSON output for automated analysis
k6 run --out json=results.json load-test.js

# Run with summary export
k6 run --summary-export=summary.json load-test.js
```
