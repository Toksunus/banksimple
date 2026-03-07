import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { validerKyc } from '../api.js';

export default function KycPending() {
  const navigate = useNavigate();
  const location = useLocation();
  const { clientId, nom } = location.state ?? {};
  const [statut, setStatut] = useState('pending'); // 'pending' | 'valide' | 'erreur'
  const [loading, setLoading] = useState(false);

  if (!clientId) {
    navigate('/inscription');
    return null;
  }

  async function simulerValidation() {
    setLoading(true);
    try {
      await validerKyc(clientId);
      setStatut('valide');
      setTimeout(() => navigate('/login'), 2000);
    } catch {
      setStatut('erreur');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={styles.card}>
      <h2 style={styles.titre}>Vérification KYC</h2>

      {statut === 'pending' && (
        <>
          <p style={styles.texte}>
            Bonjour <strong>{nom}</strong>, votre dossier est en cours d'examen.
          </p>
          <div style={styles.badge}>En attente de validation</div>
          <button style={styles.btn} onClick={simulerValidation} disabled={loading}>
            {loading ? 'Validation...' : 'Simuler la validation KYC'}
          </button>
        </>
      )}

      {statut === 'valide' && (
        <>
          <div style={{ ...styles.badge, background: '#e8f8ef', color: '#27ae60', borderColor: '#27ae60' }}>
            KYC validé — Compte activé
          </div>
          <p style={styles.texte}>Redirection vers la connexion...</p>
        </>
      )}

      {statut === 'erreur' && (
        <p style={{ color: '#c0392b' }}>Erreur lors de la validation. Réessayez.</p>
      )}
    </div>
  );
}

const styles = {
  card: { background: '#fff', padding: '40px 32px', borderRadius: '8px', boxShadow: '0 2px 8px rgba(0,0,0,0.1)', maxWidth: '460px', margin: '0 auto', textAlign: 'center' },
  titre: { marginBottom: '16px', color: '#1a1a2e' },
  texte: { fontSize: '0.95rem', color: '#333', marginBottom: '16px' },
  badge: { display: 'inline-block', padding: '6px 16px', background: '#fff8e1', color: '#e67e22', border: '1px solid #e67e22', borderRadius: '20px', fontSize: '0.85rem', fontWeight: '600', marginBottom: '16px' },
  btn: { width: '100%', padding: '10px', background: '#1a1a2e', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '1rem' },
};
