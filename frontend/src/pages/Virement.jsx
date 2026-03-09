import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getComptes, virement } from '../api.js';

export default function Virement() {
  const clientId = localStorage.getItem('clientId');
  const navigate = useNavigate();
  const [comptes, setComptes] = useState([]);
  const [type, setType] = useState('interne');
  const [form, setForm] = useState({ compteSourceId: '', compteDestinataireId: '', montant: '' });
  const [erreur, setErreur] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    getComptes(clientId).then(data => {
      setComptes(data);
      if (data.length > 0) setForm(f => ({ ...f, compteSourceId: data[0].compteId }));
    }).catch(() => {});
  }, [clientId]);

  function change(e) {
    setForm({ ...form, [e.target.name]: e.target.value });
    setErreur(null);
  }

  function changeType(t) {
    setType(t);
    setForm(f => ({ ...f, compteDestinataireId: '' }));
    setErreur(null);
  }

  async function soumettre(e) {
    e.preventDefault();
    setErreur(null);
    setLoading(true);
    try {
      const res = await virement(form.compteSourceId, form.compteDestinataireId, parseFloat(form.montant));
      navigate('/dashboard', { state: { virementResultat: res } });
    } catch (err) {
      setErreur(err.message);
    } finally {
      setLoading(false);
    }
  }

  const comptesDestinataires = comptes.filter(c => c.compteId !== form.compteSourceId);

  return (
    <div>
      <h2>Virement bancaire</h2>
      <div style={styles.card}>

        <div style={styles.toggle}>
          <button
            style={{ ...styles.toggleBtn, ...(type === 'interne' ? styles.toggleActive : {}) }}
            onClick={() => changeType('interne')}
            type="button"
          >
            Entre mes comptes
          </button>
          <button
            style={{ ...styles.toggleBtn, ...(type === 'externe' ? styles.toggleActive : {}) }}
            onClick={() => changeType('externe')}
            type="button"
          >
            Vers un autre client
          </button>
        </div>

        <form onSubmit={soumettre}>
          <div style={styles.field}>
            <label style={styles.label}>Compte source</label>
            <select style={styles.input} name="compteSourceId" value={form.compteSourceId} onChange={change} required>
              {comptes.map(c => (
                <option key={c.compteId} value={c.compteId}>
                  {c.type} ({Number(c.solde).toFixed(2)} $)
                </option>
              ))}
            </select>
          </div>

          <div style={styles.field}>
            <label style={styles.label}>Compte destinataire</label>
            {type === 'interne' ? (
              <select style={styles.input} name="compteDestinataireId" value={form.compteDestinataireId} onChange={change} required>
                <option value="">— Choisir un compte —</option>
                {comptesDestinataires.map(c => (
                  <option key={c.compteId} value={c.compteId}>
                    {c.type} ({Number(c.solde).toFixed(2)} $)
                  </option>
                ))}
              </select>
            ) : (
              <input
                style={styles.input}
                name="compteDestinataireId"
                value={form.compteDestinataireId}
                onChange={change}
                placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                required
              />
            )}
          </div>

          <div style={styles.field}>
            <label style={styles.label}>Montant ($)</label>
            <input style={styles.input} name="montant" type="text" inputMode="decimal"
              value={form.montant}
              onChange={e => {
                const v = e.target.value;
                const montantValide = /^\d*\.?\d{0,2}$/.test(v);
                if (montantValide) setForm({ ...form, montant: v });
              }}
              placeholder="Ex: 100.00" required />
          </div>

          {erreur && <p style={styles.erreur}>{erreur}</p>}

          <button style={styles.btn} type="submit" disabled={loading}>
            {loading ? 'Traitement...' : 'Effectuer le virement'}
          </button>
        </form>

      </div>
    </div>
  );
}

function statutColor(statut) {
  if (statut === 'Effectué') return '#27ae60';
  if (statut === 'Suspect') return '#e67e22';
  return '#c0392b';
}

const styles = {
  card: { background: '#fff', padding: '28px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' },
  toggle: { display: 'flex', marginBottom: '24px', borderRadius: '6px', overflow: 'hidden', border: '1px solid #ccc' },
  toggleBtn: { flex: 1, padding: '10px', background: '#f5f5f5', border: 'none', cursor: 'pointer', fontSize: '0.9rem' },
  toggleActive: { background: '#1a1a2e', color: '#fff', fontWeight: '600' },
  field: { marginBottom: '16px' },
  label: { display: 'block', marginBottom: '4px', fontWeight: '500', fontSize: '0.9rem' },
  input: { width: '100%', padding: '8px 10px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '0.95rem', boxSizing: 'border-box' },
  btn: { width: '100%', padding: '10px', background: '#1a1a2e', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '1rem', marginTop: '8px' },
  erreur: { color: '#c0392b', fontSize: '0.9rem' },
  resultatBox: { marginTop: '20px', padding: '16px', border: '2px solid', borderRadius: '6px', background: '#f9f9f9' },
};
