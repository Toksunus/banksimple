import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { inscrireClient } from '../api.js';

export default function Inscription() {
  const navigate = useNavigate();
  const [form, setForm] = useState({ nom: '', adresse: '', nasSimule: '', motDePasse: '' });
  const [msg, setMsg] = useState(null);
  const [erreur, setErreur] = useState(null);
  const [loading, setLoading] = useState(false);

  function change(e) {
    setForm({ ...form, [e.target.name]: e.target.value });
  }

  async function soumettre(e) {
    e.preventDefault();
    setErreur(null);
    setMsg(null);
    setLoading(true);
    try {
      const client = await inscrireClient(form.nom, form.adresse, form.nasSimule, form.motDePasse);
      navigate('/kyc-pending', { state: { clientId: client.clientId, nom: client.nom } });
    } catch (err) {
      setErreur(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={styles.card}>
      <h2>Inscription</h2>
      <form onSubmit={soumettre}>
        <Field label="Nom complet" name="nom" value={form.nom} onChange={change} required />
        <Field label="Adresse" name="adresse" value={form.adresse} onChange={change} required />
        <Field label="NAS (11 chiffres)" name="nasSimule" value={form.nasSimule} onChange={change} required />
        <Field label="Mot de passe" name="motDePasse" type="password" value={form.motDePasse} onChange={change} required />
        {erreur && <p style={styles.erreur}>{erreur}</p>}
        {msg && <p style={styles.succes}>{msg}</p>}
        <button style={styles.btn} type="submit" disabled={loading}>
          {loading ? 'Création...' : "S'inscrire"}
        </button>
      </form>
      <p style={styles.lien}>Déjà un compte ? <Link to="/login">Se connecter</Link></p>
    </div>
  );
}

function Field({ label, name, value, onChange, type = 'text', required }) {
  return (
    <div style={{ marginBottom: '14px' }}>
      <label style={styles.label}>{label}</label>
      <input style={styles.input} name={name} type={type} value={value} onChange={onChange} required={required} />
    </div>
  );
}

const styles = {
  card: { background: '#fff', padding: '32px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)', maxWidth: '440px', margin: '0 auto' },
  label: { display: 'block', marginBottom: '4px', fontWeight: '500', fontSize: '0.9rem' },
  input: { width: '100%', padding: '8px 10px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '0.95rem', boxSizing: 'border-box' },
  btn: { width: '100%', padding: '10px', background: '#1a1a2e', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '1rem', marginTop: '8px' },
  erreur: { color: '#c0392b', fontSize: '0.9rem' },
  succes: { color: '#27ae60', fontSize: '0.9rem' },
  lien: { marginTop: '16px', textAlign: 'center', fontSize: '0.9rem' },
};
