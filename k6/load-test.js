import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Counter, Rate } from 'k6/metrics';

const latenceComptes     = new Trend('latence_comptes', true);
const latenceVirement    = new Trend('latence_virement', true);
const erreursTotal       = new Counter('erreurs_total');
const tauxErreurs        = new Rate('taux_erreurs');

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

export const options = {
  scenarios: {
    charge_progressive: {
      executor: 'ramping-vus',
      stages: [
        { duration: '30s', target: 5   },
        { duration: '1m',  target: 5   },
        { duration: '30s', target: 15  },
        { duration: '1m',  target: 15  },
        { duration: '30s', target: 0   },
      ],
    },
  },

  thresholds: {
    http_req_duration:      ['p(95)<500', 'p(99)<1000'],
    http_req_failed:        ['rate<0.01'],
    taux_erreurs:           ['rate<0.01'],
    latence_comptes:        ['p(95)<200'],
    latence_virement:       ['p(95)<500'],
  },
};

export function setup() {
  const headers = { 'Content-Type': 'application/json' };

  const ts = Math.floor(Math.random() * 900000000 + 100000000);
  const nas1 = `10${ts}`;
  const nas2 = `20${ts}`;

  const r1 = http.post(`${BASE_URL}/api/v1/clients/inscription`, JSON.stringify({
    nom: 'Daniel Atik',
    email: 'daniel.atik@gmail.com',
    nasSimule: nas1,
    motDePasse: 'Test1',
  }), { headers });

  if (r1.status !== 201) {
    console.error(`[setup] Échec inscription client 1: ${r1.status} — ${r1.body}`);
    return null;
  }
  const client1 = r1.json();

  http.post(`${BASE_URL}/api/v1/clients/${client1.clientId}/valider-kyc`, null, { headers });

  const r2 = http.post(`${BASE_URL}/api/v1/clients/inscription`, JSON.stringify({
    nom: 'Jad Bizri',
    email: 'jad.bizri@gmail.com',
    nasSimule: nas2,
    motDePasse: 'Test2',
  }), { headers });

  if (r2.status !== 201) {
    console.error(`[setup] Échec inscription client 2: ${r2.status} — ${r2.body}`);
    return null;
  }
  const client2 = r2.json();
  http.post(`${BASE_URL}/api/v1/clients/${client2.clientId}/valider-kyc`, null, { headers });

  const login1 = http.post(`${BASE_URL}/api/v1/auth/login`, JSON.stringify({
    login: nas1, motDePasse: 'Test1',
  }), { headers }).json();

  const token1 = http.post(`${BASE_URL}/api/v1/auth/verify-otp`, JSON.stringify({
    clientId: login1.clientId, otpCode: login1.otpCode,
  }), { headers }).json('token');

  const login2 = http.post(`${BASE_URL}/api/v1/auth/login`, JSON.stringify({
    login: nas2, motDePasse: 'Test2',
  }), { headers }).json();

  const token2 = http.post(`${BASE_URL}/api/v1/auth/verify-otp`, JSON.stringify({
    clientId: login2.clientId, otpCode: login2.otpCode,
  }), { headers }).json('token');

  const authHeaders1 = { 'Content-Type': 'application/json', Authorization: `Bearer ${token1}` };
  const authHeaders2 = { 'Content-Type': 'application/json', Authorization: `Bearer ${token2}` };

  const compteUser1 = http.post(
    `${BASE_URL}/api/v1/clients/${client1.clientId}/comptes`,
    JSON.stringify({ type: 'Cheque' }),
    { headers: authHeaders1 }
  );
  const compte1 = compteUser1.json();

  const compteUser2 = http.post(
    `${BASE_URL}/api/v1/clients/${client2.clientId}/comptes`,
    JSON.stringify({ type: 'Cheque' }),
    { headers: authHeaders2 }
  );
  const compte2 = compteUser2.json();

  http.post(
    `${BASE_URL}/api/v1/clients/${client1.clientId}/comptes/${compte1.compteId}/depot`,
    JSON.stringify(50000),
    { headers: authHeaders1 }
  );
  http.post(
    `${BASE_URL}/api/v1/clients/${client2.clientId}/comptes/${compte2.compteId}/depot`,
    JSON.stringify(50000),
    { headers: authHeaders2 }
  );

  console.log(`[setup] Données prêtes — Client1: ${client1.clientId}, Client2: ${client2.clientId}`);

  return {
    client1Id:  client1.clientId,
    client2Id:  client2.clientId,
    compte1Id:  compte1.compteId,
    compte2Id:  compte2.compteId,
    token1,
    token2,
  };
}

export default function (data) {
  if (!data) {
    console.error('Données setup manquantes, skip.');
    return;
  }

  const { client1Id, client2Id, compte1Id, compte2Id, token1, token2 } = data;
  const authHeaders1 = { 'Content-Type': 'application/json', Authorization: `Bearer ${token1}` };
  const authHeaders2 = { 'Content-Type': 'application/json', Authorization: `Bearer ${token2}` };

  const rand = Math.random();

  if (rand < 0.35) {
    const res = http.get(
      `${BASE_URL}/api/v1/clients/${client1Id}/comptes`,
      { headers: authHeaders1 }
    );
    latenceComptes.add(res.timings.duration);
    const ok = check(res, { 'GET comptes → 200': (r) => r.status === 200 });
    if (!ok) { erreursTotal.add(1); tauxErreurs.add(1); } else { tauxErreurs.add(0); }

  } else if (rand < 0.60) {
    const res = http.get(
      `${BASE_URL}/api/v1/clients/${client2Id}/comptes`,
      { headers: authHeaders2 }
    );
    latenceComptes.add(res.timings.duration);
    const ok = check(res, { 'GET comptes client2 → 200': (r) => r.status === 200 });
    if (!ok) { erreursTotal.add(1); tauxErreurs.add(1); } else { tauxErreurs.add(0); }

  } else if (rand < 0.80) {
    const res = http.post(
      `${BASE_URL}/api/v1/clients/${client1Id}/comptes/${compte1Id}/depot`,
      JSON.stringify(100),
      { headers: authHeaders1 }
    );
    const ok = check(res, { 'POST dépôt → 200': (r) => r.status === 200 });
    if (!ok) { erreursTotal.add(1); tauxErreurs.add(1); } else { tauxErreurs.add(0); }

  } else {
    const res = http.post(
      `${BASE_URL}/api/v1/virements`,
      JSON.stringify({
        compteSourceId:        compte1Id,
        compteDestinataireId:  compte2Id,
        montant:               50,
      }),
      { headers: authHeaders1 }
    );
    latenceVirement.add(res.timings.duration);
    const ok = check(res, { 'POST virement → 200': (r) => r.status === 200 });
    if (!ok) { erreursTotal.add(1); tauxErreurs.add(1); } else { tauxErreurs.add(0); }
  }

  sleep(0.5);
}

export function teardown(data) {
  if (data) {
    console.log(`[teardown] Test terminé.`);
    console.log(`  → Client source : ${data.client1Id}`);
    console.log(`  → Client dest   : ${data.client2Id}`);
  }
}
