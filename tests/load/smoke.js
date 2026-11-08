// Smoke test: a few iterations of every critical path (used in CI after each deploy/build).
import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, login, client, ensureFixtures, checkInOut } from './lib.js';

export const options = {
  vus: 2,
  iterations: 10,
  thresholds: {
    checks: ['rate>0.99'],
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<800'],
    parking_checkins: ['count>0'],
  },
};

export function setup() {
  const ready = http.get(`${BASE_URL}/health/ready`);
  check(ready, { 'ready 200': (r) => r.status === 200 });
  const token = login();
  return { token, ...ensureFixtures(client(token), 10) };
}

export default function (data) {
  const api = client(data.token);
  check(api.get('/spaces'), { 'spaces 200': (r) => r.status === 200 });
  check(api.get('/tickets?status=Closed&pageSize=20'), { 'tickets 200': (r) => r.status === 200 });
  check(api.get('/dashboard/summary?period=today'), { 'dashboard 200': (r) => r.status === 200 });
  checkInOut(api, data.brandId);
}
