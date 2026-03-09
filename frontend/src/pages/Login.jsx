import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { login, verifyOtp } from '../api.js';

export default function Login() {
  const navigate = useNavigate();
  const [etape, setEtape] = useState(1); // 1 = NAS+mdp, 2 = OTP
  const [form, setForm] = useState({ nasSimule: '', motDePasse: '' });
  const [otpAffiche, setOtpAffiche] = useState('');
  const [otpSaisi, setOtpSaisi] = useState('');
  const [clientId, setClientId] = useState('');
  const [erreur, setErreur] = useState(null);
  const [loading, setLoading] = useState(false);

  function change(e) {
    setForm({ ...form, [e.target.name]: e.target.value });
  }

  async function soumettre(e) {
    e.preventDefault();
    setErreur(null);
    setLoading(true);
    try {
      const data = await login(form.nasSimule, form.motDePasse);
      setClientId(data.clientId);
      setOtpAffiche(data.otpCode);
      setEtape(2);
    } catch (err) {
      setErreur(err.message);
    } finally {
      setLoading(false);
    }
  }

  async function verifier(e) {
    e.preventDefault();
    setErreur(null);
    setLoading(true);
    try {
      const data = await verifyOtp(clientId, otpSaisi);
      localStorage.setItem('token', data.token);
      localStorage.setItem('clientId', data.clientId);
      navigate('/dashboard');
    } catch (err) {
      setErreur(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={styles.card}>
      <h2>Connexion</h2>

      {etape === 1 && (
        <form onSubmit={soumettre}>
          <div style={{ marginBottom: '14px' }}>
            <label style={styles.label}>NAS</label>
            <input style={styles.input} name="nasSimule" value={form.nasSimule} onChange={change} required />
          </div>
          <div style={{ marginBottom: '14px' }}>
            <label style={styles.label}>Mot de passe</label>
            <input style={styles.input} name="motDePasse" type="password" value={form.motDePasse} onChange={change} required />
          </div>
          {erreur && <p style={styles.erreur}>{erreur}</p>}
          <button style={styles.btn} type="submit" disabled={loading}>
            {loading ? 'Vérification...' : 'Continuer'}
          </button>
        </form>
      )}

      {etape === 2 && (
        <form onSubmit={verifier}>
          <p style={styles.mfaInfo}>
            Vérification MFA — entrez le code généré par le système.
          </p>
          <div style={styles.otpBox}>
            <span style={styles.otpLabel}>Code MFA (simulation)</span>
            <span style={styles.otpCode}>{otpAffiche}</span>
          </div>
          <div style={{ marginBottom: '14px' }}>
            <label style={styles.label}>Entrez le code</label>
            <input
              style={{ ...styles.input, letterSpacing: '0.2em', textAlign: 'center' }}
              value={otpSaisi}
              onChange={e => setOtpSaisi(e.target.value)}
              maxLength={6}
              placeholder="______"
              required
            />
          </div>
          {erreur && <p style={styles.erreur}>{erreur}</p>}
          <button style={styles.btn} type="submit" disabled={loading}>
            {loading ? 'Validation...' : 'Valider le code'}
          </button>
          <button style={styles.btnRetour} type="button" onClick={() => { setEtape(1); setErreur(null); }}>
            Retour
          </button>
        </form>
      )}

      {etape === 1 && (
        <p style={styles.lien}>Pas de compte ? <Link to="/inscription">S'inscrire</Link></p>
      )}
    </div>
  );
}

const styles = {
  card: { background: '#fff', padding: '32px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)', maxWidth: '400px', margin: '0 auto' },
  label: { display: 'block', marginBottom: '4px', fontWeight: '500', fontSize: '0.9rem' },
  input: { width: '100%', padding: '8px 10px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '0.95rem', boxSizing: 'border-box' },
  btn: { width: '100%', padding: '10px', background: '#1a1a2e', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '1rem', marginTop: '8px' },
  btnRetour: { width: '100%', padding: '8px', background: 'transparent', color: '#666', border: '1px solid #ccc', borderRadius: '4px', cursor: 'pointer', fontSize: '0.9rem', marginTop: '8px' },
  erreur: { color: '#c0392b', fontSize: '0.9rem' },
  lien: { marginTop: '16px', textAlign: 'center', fontSize: '0.9rem' },
  mfaInfo: { fontSize: '0.9rem', color: '#555', marginBottom: '16px' },
  otpBox: { background: '#f0f4ff', border: '1px solid #a0c4ff', borderRadius: '6px', padding: '14px 16px', marginBottom: '16px', display: 'flex', flexDirection: 'column', gap: '4px' },
  otpLabel: { fontSize: '0.75rem', color: '#666', textTransform: 'uppercase', letterSpacing: '0.05em' },
  otpCode: { fontSize: '1.8rem', fontWeight: 'bold', color: '#1a1a2e', letterSpacing: '0.25em' },
};
