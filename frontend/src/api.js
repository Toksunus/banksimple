const BASE = '/api/v1';

function authHeaders() {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function handleResponse(res) {
  const text = await res.text();
  let data;
  try { data = JSON.parse(text); } catch { data = text; }
  if (!res.ok) throw new Error(data?.error || data || `Erreur ${res.status}`);
  return data;
}

export async function inscrireClient(nom, adresse, nasSimule, motDePasse) {
  const res = await fetch(`${BASE}/clients/inscription`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ nom, adresse, nasSimule, motDePasse }),
  });
  return handleResponse(res);
}

export async function validerKyc(clientId) {
  const res = await fetch(`${BASE}/clients/${clientId}/valider-kyc`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', 'Content-Length': '0' },
  });
  return handleResponse(res);
}

export async function login(nasSimule, motDePasse) {
  const res = await fetch(`${BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ login: nasSimule, motDePasse }),
  });
  return handleResponse(res);
}

export async function verifyOtp(clientId, otpCode) {
  const res = await fetch(`${BASE}/auth/verify-otp`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ clientId, otpCode }),
  });
  return handleResponse(res);
}

export async function getClient(clientId) {
  const res = await fetch(`${BASE}/clients/${clientId}`, {
    headers: authHeaders(),
  });
  return handleResponse(res);
}

export async function getComptes(clientId) {
  const res = await fetch(`${BASE}/clients/${clientId}/comptes`, {
    headers: authHeaders(),
  });
  return handleResponse(res);
}

export async function creerCompte(clientId, type) {
  const res = await fetch(`${BASE}/clients/${clientId}/comptes`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify({ type }),
  });
  return handleResponse(res);
}

export async function depot(clientId, compteId, montant) {
  const res = await fetch(`${BASE}/clients/${clientId}/comptes/${compteId}/depot`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify(montant),
  });
  return handleResponse(res);
}

export async function fermerCompte(clientId, compteId) {
  const res = await fetch(`${BASE}/clients/${clientId}/comptes/${compteId}`, {
    method: 'DELETE',
    headers: authHeaders(),
  });
  return handleResponse(res);
}

export async function getVirements(compteId) {
  const res = await fetch(`${BASE}/virements/compte/${compteId}`, {
    headers: authHeaders(),
  });
  return handleResponse(res);
}

export async function virement(compteSourceId, compteDestinataireId, montant) {
  const res = await fetch(`${BASE}/virements`, {
    method: 'POST',
    headers: authHeaders(),
    body: JSON.stringify({ compteSourceId, compteDestinataireId, montant }),
  });
  return handleResponse(res);
}
