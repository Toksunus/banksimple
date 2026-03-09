import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

const BASE_URL    = __ENV.BASE_URL || 'http://localhost:8080';
const tauxErreurs = new Rate('taux_erreurs');
const latenceP95  = new Trend('latence_p95', true);

export const options = {
  scenarios: {
    spike: {
      executor: 'ramping-arrival-rate',
      startRate: 5,
      preAllocatedVUs: 50,
      maxVUs: 100,
      stages: [
        { duration: '20s', target: 5   },
        { duration: '10s', target: 60  },
        { duration: '30s', target: 60  },
        { duration: '10s', target: 5   },
        { duration: '20s', target: 5   },
      ],
    },
  },

  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed:   ['rate<0.05'],
    taux_erreurs:      ['rate<0.05'],
  },
};

export function setup() {
  const headers = { 'Content-Type': 'application/json' };
  const ts   = Math.floor(Math.random() * 900000000 + 100000000);
  const nas1 = `30${ts}`;
  const nas2 = `40${ts}`;

  const r1 = http.post(`${BASE_URL}/api/v1/clients/inscription`, JSON.stringify({
    nom: 'Samy Mekkati', email: 'samy.mekkati@gmail.com', nasSimule: nas1, motDePasse: 'StressTest1',
  }), { headers });

  const client1 = r1.json();
  http.post(`${BASE_URL}/api/v1/clients/${client1.clientId}/valider-kyc`, null, { headers });

  const r2 = http.post(`${BASE_URL}/api/v1/clients/inscription`, JSON.stringify({
    nom: 'Mehdi Battou', email: 'mehdi.battou@gmail.com', nasSimule: nas2, motDePasse: 'StressTest2',
  }), { headers });

  const client2 = r2.json();
  http.post(`${BASE_URL}/api/v1/clients/${client2.clientId}/valider-kyc`, null, { headers });

  const login1 = http.post(`${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ login: nas1, motDePasse: 'StressTest1' }),
    { headers }).json();

  const token1 = http.post(`${BASE_URL}/api/v1/auth/verify-otp`,
    JSON.stringify({ clientId: login1.clientId, otpCode: login1.otpCode }),
    { headers }).json('token');

  const login2 = http.post(`${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ login: nas2, motDePasse: 'StressTest2' }),
    { headers }).json();

  const token2 = http.post(`${BASE_URL}/api/v1/auth/verify-otp`,
    JSON.stringify({ clientId: login2.clientId, otpCode: login2.otpCode }), { headers }).json('token');

  const authHeaders1 = { 'Content-Type': 'application/json', Authorization: `Bearer ${token1}` };

  const authHeaders2 = { 'Content-Type': 'application/json', Authorization: `Bearer ${token2}` };

  const compte1 = http.post(`${BASE_URL}/api/v1/clients/${client1.clientId}/comptes`,
    JSON.stringify({ type: 'Cheque' }),
    { headers: authHeaders1 }).json();

  const compte2 = http.post(`${BASE_URL}/api/v1/clients/${client2.clientId}/comptes`,
    JSON.stringify({ type: 'Cheque' }),
    { headers: authHeaders2 }).json();

  http.post(`${BASE_URL}/api/v1/clients/${client1.clientId}/comptes/${compte1.compteId}/depot`,
    JSON.stringify(999999),
    { headers: authHeaders1 });

  http.post(`${BASE_URL}/api/v1/clients/${client2.clientId}/comptes/${compte2.compteId}/depot`,
    JSON.stringify(999999),
    { headers: authHeaders2 });

  return {
    client1Id: client1.clientId,
    client2Id: client2.clientId,
    compte1Id: compte1.compteId,
    compte2Id: compte2.compteId,
    token1,
    token2,
  };
}

export default function (data) {
  if (!data) {
    console.error('Données setup manquantes, skip.');
    return;
  }

  const { client1Id, compte1Id, compte2Id, token1 } = data;
  const authHeaders1 = { 'Content-Type': 'application/json', Authorization: `Bearer ${token1}` };

  const rand = Math.random();

  if (rand < 0.5) {
    const res = http.get(`${BASE_URL}/api/v1/clients/${client1Id}/comptes`, { headers: authHeaders1 });
    latenceP95.add(res.timings.duration);
    const ok = check(res, { 'GET comptes → 200': (r) => r.status === 200 });
    tauxErreurs.add(!ok ? 1 : 0);

  } else if (rand < 0.8) {
    const res = http.post(`${BASE_URL}/api/v1/clients/${client1Id}/comptes/${compte1Id}/depot`,
      JSON.stringify(10), { headers: authHeaders1 });
    latenceP95.add(res.timings.duration);
    const ok = check(res, { 'POST dépôt → 200': (r) => r.status === 200 });
    tauxErreurs.add(!ok ? 1 : 0);

  } else {
    const res = http.post(`${BASE_URL}/api/v1/virements`, JSON.stringify({
      compteSourceId: compte1Id, compteDestinataireId: compte2Id, montant: 1,
    }), { headers: authHeaders1 });
    latenceP95.add(res.timings.duration);
    const ok = check(res, { 'POST virement → 200': (r) => r.status === 200 });
    tauxErreurs.add(!ok ? 1 : 0);
  }

  sleep(0.2);
}

export function teardown(data) {
  if (data) {
    console.log(`[teardown] Test terminé.`);
    console.log(`  → Client source : ${data.client1Id}`);
    console.log(`  → Client dest   : ${data.client2Id}`);
  }
}
