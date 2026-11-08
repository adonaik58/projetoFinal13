// Load test (run against staging with RATE_LIMIT_* raised for the test client):
//   docker run --rm -v "$PWD/tests/load:/scripts" -e BASE_URL=https://staging.example -e ADMIN_USERNAME=... \
//     -e ADMIN_PASSWORD=... grafana/k6:0.55.0 run /scripts/load.js
import { check, sleep } from 'k6';
import { login, client, ensureFixtures, checkInOut } from './lib.js';

export const options = {
  scenarios: {
    // Operators watching the board and browsing history.
    browse: {
      executor: 'ramping-vus',
      exec: 'browse',
      startVUs: 0,
      stages: [
        { duration: '1m', target: 50 },
        { duration: '3m', target: 50 },
        { duration: '1m', target: 0 },
      ],
    },
    // Vehicles entering and leaving.
    traffic: {
      executor: 'constant-arrival-rate',
      exec: 'traffic',
      rate: 10,
      timeUnit: '1s',
      duration: '5m',
      preAllocatedVUs: 20,
      maxVUs: 60,
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    'http_req_duration{name:GET /spaces}': ['p(95)<300'],
    'http_req_duration{name:GET /tickets}': ['p(95)<400'],
    'http_req_duration{name:POST /tickets}': ['p(95)<800'],
    'http_req_duration{name:POST /tickets/{id}/close}': ['p(95)<800'],
    checks: ['rate>0.99'],
  },
};

export function setup() {
  const token = login();
  return { token, ...ensureFixtures(client(token), 200) };
}

export function browse(data) {
  const api = client(data.token);
  check(api.get('/spaces'), { 'spaces 200': (r) => r.status === 200 });
  check(api.get(`/tickets?status=Closed&page=${1 + Math.floor(Math.random() * 5)}&pageSize=20`), { 'tickets 200': (r) => r.status === 200 });
  sleep(1 + Math.random() * 2);
}

export function traffic(data) {
  checkInOut(client(data.token), data.brandId);
}
