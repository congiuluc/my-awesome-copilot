// Sample: k6 smoke test for API endpoints
// Run: k6 run tests/load/smoke.js
// CI: runs on every PR to catch regressions early

import http from 'k6/http';
import { check, group, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export const options = {
  vus: 3,
  duration: '1m',
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.05'],
  },
};

export default function () {
  group('Health check', () => {
    const res = http.get(`${BASE_URL}/health`);
    check(res, {
      'health status 200': (r) => r.status === 200,
    });
  });

  group('List products', () => {
    const res = http.get(`${BASE_URL}/api/products`);
    check(res, {
      'products status 200': (r) => r.status === 200,
      'products response time OK': (r) => r.timings.duration < 500,
      'products has data': (r) => {
        const body = JSON.parse(r.body);
        return body.success === true && body.data !== undefined;
      },
    });
  });

  group('Get single product', () => {
    const res = http.get(`${BASE_URL}/api/products/1`);
    check(res, {
      'product status 200': (r) => r.status === 200,
      'product response time OK': (r) => r.timings.duration < 300,
    });
  });

  sleep(1);
}
