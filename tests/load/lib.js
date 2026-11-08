import http from 'k6/http';
import { check, fail } from 'k6';
import { Counter } from 'k6/metrics';

/** Successful check-ins; smoke tests assert it is > 0 so a silently skipped flow cannot pass. */
export const checkIns = new Counter('parking_checkins');

export const BASE_URL = __ENV.BASE_URL || 'https://localhost';
const API = `${BASE_URL}/api/v1`;

export function login(username = __ENV.ADMIN_USERNAME, password = __ENV.ADMIN_PASSWORD) {
  const res = http.post(`${API}/auth/login`, JSON.stringify({ username, password }), {
    headers: { 'Content-Type': 'application/json' },
    tags: { name: 'POST /auth/login' },
  });
  if (!check(res, { 'login 200': (r) => r.status === 200 })) fail(`login failed: ${res.status} ${res.body}`);
  return res.json('accessToken');
}

export function client(token) {
  const headers = { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' };
  return {
    get: (path, name) => http.get(`${API}${path}`, { headers, tags: { name: name || `GET ${path.split('?')[0]}` } }),
    post: (path, body, name, extra = {}, expected = undefined) =>
      http.post(`${API}${path}`, body === undefined ? null : JSON.stringify(body), {
        headers: { ...headers, ...extra },
        tags: { name: name || `POST ${path}` },
        responseCallback: expected,
      }),
  };
}

/** Test fixtures: one brand and a pool of spaces dedicated to load testing (section "LT"). */
export function ensureFixtures(api, spaces = 40) {
  const brands = api.get('/catalog/brands');
  if (brands.status !== 200) fail(`fixtures: cannot list brands (${brands.status})`);
  let brand = brands.json().find((b) => b.name === 'Carga k6');
  if (!brand) {
    const created = api.post('/catalog/brands', { name: 'Carga k6' });
    if (created.status !== 201) fail(`fixtures: cannot create brand (${created.status} ${created.body})`);
    brand = created.json();
  }
  const bulk = api.post('/spaces/bulk', { sections: ['LT'], fromNumber: 1, toNumber: spaces });
  if (bulk.status !== 200) fail(`fixtures: cannot create load-test spaces (${bulk.status} ${bulk.body})`);
  return { brandId: brand.id };
}

export function randomNationalId() {
  const digits = (n) => Array.from({ length: n }, () => Math.floor(Math.random() * 10)).join('');
  return `${digits(9)}LT${digits(3)}`;
}

export function randomPlate() {
  return `LT-${Math.floor(Math.random() * 1e6)}-${String.fromCharCode(65 + Math.floor(Math.random() * 26))}`;
}

export function checkInOut(api, brandId) {
  const board = api.get('/spaces?search=LT&status=Free', 'GET /spaces (free)');
  const free = board.json('items') || [];
  if (free.length === 0) return;
  const space = free[Math.floor(Math.random() * free.length)];
  const opened = api.post(
    '/tickets',
    {
      spaceId: space.id,
      customer: { fullName: 'Cliente Carga', nationalId: randomNationalId(), birthDate: '1990-01-01' },
      vehicle: { plate: randomPlate(), brandId, modelId: null, color: '#336699' },
    },
    'POST /tickets',
    { 'Idempotency-Key': `${__VU}-${__ITER}-${Date.now()}` },
    http.expectedStatuses(201, 409),
  );
  // 409 = another VU took the same space first: an expected business outcome under contention.
  check(opened, { 'check-in 201/409': (r) => r.status === 201 || r.status === 409 });
  if (opened.status !== 201) return;
  checkIns.add(1);
  const closed = api.post(`/tickets/${opened.json('id')}/close`, undefined, 'POST /tickets/{id}/close');
  check(closed, { 'check-out 200': (r) => r.status === 200 });
}
