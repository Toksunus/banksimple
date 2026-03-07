import { useState, useEffect, useCallback } from 'react';
import { useLocation } from 'react-router-dom';
import { getClient, getComptes, creerCompte, depot, fermerCompte, getVirements } from '../api.js';

export default function Dashboard() {
  const clientId = localStorage.getItem('clientId');
  const location = useLocation();
  const virementResultat = location.state?.virementResultat ?? null;
  const [client, setClient] = useState(null);
  const [comptes, setComptes] = useState([]);
  const [erreur, setErreur] = useState(null);
  const [loading, setLoading] = useState(true);

  // Créer compte
  const [typeCompte, setTypeCompte] = useState('Cheque');
  const [msgCreer, setMsgCreer] = useState(null);

  // Dépôt
  const [depotCompteId, setDepotCompteId] = useState('');
  const [depotMontant, setDepotMontant] = useState('');
  const [msgDepot, setMsgDepot] = useState(null);

  const [copiedId, setCopiedId] = useState(null);

  // Historique
  const [historiqueCompteId, setHistoriqueCompteId] = useState('');
  const [virements, setVirements] = useState([]);
  const [loadingHistorique, setLoadingHistorique] = useState(false);
  const [clientsNoms, setClientsNoms] = useState({});

  async function handleFermerCompte(compteId) {
    if (!window.confirm('Supprimer ce compte définitivement ?')) return;
    try {
      await fermerCompte(clientId, compteId);
      chargerComptes();
    } catch (err) {
      alert(err.message);
    }
  }

  function copierId(id) {
    navigator.clipboard.writeText(id);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  }

  const chargerComptes = useCallback(async () => {
    try {
      const data = await getComptes(clientId);
      setComptes(data);
      if (data.length > 0 && !depotCompteId) setDepotCompteId(data[0].compteId);
    } catch (err) {
      setErreur(err.message);
    } finally {
      setLoading(false);
    }
  }, [clientId, depotCompteId]);

  useEffect(() => {
    chargerComptes();
    getClient(clientId).then(setClient).catch(() => {});
  }, []);

  useEffect(() => {
    const aCheque = comptes.some(c => c.type === 'Cheque');
    const aEpargne = comptes.some(c => c.type === 'Epargne');
    if (aCheque && !aEpargne) setTypeCompte('Epargne');
    else if (!aCheque) setTypeCompte('Cheque');
  }, [comptes]);

  async function handleCreerCompte(e) {
    e.preventDefault();
    setMsgCreer(null);
    try {
      await creerCompte(clientId, typeCompte);
      setMsgCreer('Compte créé avec succès.');
      chargerComptes();
    } catch (err) {
      setMsgCreer(`Erreur : ${err.message}`);
    }
  }

  async function chargerHistorique(compteId) {
    setHistoriqueCompteId(compteId);
    setLoadingHistorique(true);
    try {
      const data = await getVirements(compteId);
      setVirements(data);

      const uniqueClientIds = [...new Set(
        data.map(v => v.titulaireId).filter(Boolean)
      )];
      const nouveaux = uniqueClientIds.filter(id => !clientsNoms[id]);
      if (nouveaux.length > 0) {
        const resultats = await Promise.all(nouveaux.map(id => getClient(id).catch(() => null)));
        const map = {};
        nouveaux.forEach((id, i) => { if (resultats[i]) map[id] = resultats[i].nom; });
        setClientsNoms(prev => ({ ...prev, ...map }));
      }
    } catch {
      setVirements([]);
    } finally {
      setLoadingHistorique(false);
    }
  }

  async function handleDepot(e) {
    e.preventDefault();
    setMsgDepot(null);
    try {
      const res = await depot(clientId, depotCompteId, parseFloat(depotMontant));
      setMsgDepot(`Nouveau solde : ${res.solde} $`);
      setDepotMontant('');
      chargerComptes();
    } catch (err) {
      setMsgDepot(`Erreur : ${err.message}`);
    }
  }

  if (loading) return <p>Chargement...</p>;
  if (erreur) return <p style={{ color: 'red' }}>{erreur}</p>;

  return (
    <div>
      <h2>Mes comptes</h2>

      {/* Informations du client */}
      {client && (
        <div style={styles.clientCard}>
          <div style={styles.clientNom}>{client.nom}</div>
          <div style={styles.clientInfo}>Adresse : {client.adresse}</div>
          <div style={styles.clientInfo}>Statut : {client.statut}</div>
        </div>
      )}

      {/* Liste des comptes */}
      {comptes.length === 0 ? (
        <p style={{ color: '#666' }}>Aucun compte. Créez-en un ci-dessous.</p>
      ) : (
        <div style={styles.grid}>
          {comptes.map(c => (
            <div key={c.compteId} style={styles.compteCard}>
              <div style={styles.compteType}>{c.type}</div>
              <div style={styles.compteSolde}>{Number(c.solde).toFixed(2)} $</div>
              <div style={styles.compteInfo}>Statut : {c.statut}</div>
              <div style={styles.compteId}>ID : {c.compteId.slice(0, 8)}…</div>
              <div style={{ display: 'flex', gap: '6px', marginTop: '8px' }}>
                <button style={styles.btnCopy} onClick={() => copierId(c.compteId)}>
                  {copiedId === c.compteId ? 'Copié !' : 'Copier ID'}
                </button>
                {c.solde === 0 && (
                  <button style={styles.btnFermer} onClick={() => handleFermerCompte(c.compteId)}>
                    Supprimer
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Créer un compte */}
      {(() => {
        const aCheque = comptes.some(c => c.type === 'Cheque');
        const aEpargne = comptes.some(c => c.type === 'Epargne');
        if (aCheque && aEpargne) return null;
        return (
          <div style={styles.section}>
            <h3>Ouvrir un compte</h3>
            <form onSubmit={handleCreerCompte} style={styles.row}>
              <select style={styles.select} value={typeCompte} onChange={e => setTypeCompte(e.target.value)}>
                {!aCheque && <option value="Cheque">Chèque</option>}
                {!aEpargne && <option value="Epargne">Épargne</option>}
              </select>
              <button style={styles.btnSm} type="submit">Créer</button>
            </form>
            {msgCreer && <p style={msgCreer.startsWith('Erreur') ? styles.erreur : styles.succes}>{msgCreer}</p>}
          </div>
        );
      })()}

      {/* Dépôt */}
      {comptes.length > 0 && (
        <div style={styles.section}>
          <h3>Faire un dépôt</h3>
          <form onSubmit={handleDepot} style={styles.row}>
            <select style={styles.select} value={depotCompteId} onChange={e => setDepotCompteId(e.target.value)}>
              {comptes.map(c => (
                <option key={c.compteId} value={c.compteId}>{c.type} ({Number(c.solde).toFixed(2)} $)</option>
              ))}
            </select>
            <input
              style={{ ...styles.input, width: '120px' }}
              type="text" inputMode="decimal"
              placeholder="Montant $"
              value={depotMontant}
              onChange={e => {
                const v = e.target.value;
                if (/^\d*\.?\d{0,2}$/.test(v)) setDepotMontant(v);
              }}
              required
            />
            <button style={styles.btnSm} type="submit">Déposer</button>
          </form>
          {msgDepot && <p style={msgDepot.startsWith('Erreur') ? styles.erreur : styles.succes}>{msgDepot}</p>}
        </div>
      )}

      {/* Historique des virements */}
      {comptes.length > 0 && (
        <div style={styles.section}>
          <h3>Historique des virements</h3>
          <div style={styles.row}>
            <select
              style={styles.select}
              value={historiqueCompteId}
              onChange={e => chargerHistorique(e.target.value)}
            >
              <option value="">— Choisir un compte —</option>
              {comptes.map(c => (
                <option key={c.compteId} value={c.compteId}>{c.type} ({Number(c.solde).toFixed(2)} $)</option>
              ))}
            </select>
          </div>
          {loadingHistorique && <p style={{ fontSize: '0.9rem', color: '#666', marginTop: '12px' }}>Chargement...</p>}
          {!loadingHistorique && historiqueCompteId && virements.length === 0 && (
            <p style={{ fontSize: '0.9rem', color: '#666', marginTop: '12px' }}>Aucun virement pour ce compte.</p>
          )}
          {virements.length > 0 && (
            <table style={styles.table}>
              <thead>
                <tr>
                  <th style={styles.th}>Date</th>
                  <th style={styles.th}>Montant</th>
                  <th style={styles.th}>Direction</th>
                  <th style={styles.th}>Compte contrepartie</th>
                  <th style={styles.th}>Statut</th>
                </tr>
              </thead>
              <tbody>
                {virements.map(v => {
                  const estSource = v.compteSourceId === historiqueCompteId;
                  const d = new Date(v.dateVirement);
                  const date = `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')} ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}`;
                  const nomTitulaire = v.titulaireId
                    ? (clientsNoms[v.titulaireId] ?? v.titulaireId.slice(0, 8) + '…')
                    : '—';
                  return (
                    <tr key={v.virementId}>
                      <td style={styles.td}>{date}</td>
                      <td style={styles.td}>{Number(v.montant).toFixed(2)} $</td>
                      <td style={{ ...styles.td, color: estSource ? '#c0392b' : '#27ae60', fontWeight: '600' }}>
                        {estSource ? 'Envoyé' : 'Reçu'}
                      </td>
                      <td style={styles.td}>{nomTitulaire}</td>
                      <td style={{ ...styles.td, color: statutColor(v.statut) }}>{v.statut}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* Résultat dernier virement */}
      {virementResultat && (
        <div style={{ ...styles.resultatBox, borderColor: statutColor(virementResultat.statut) }}>
          <p><strong>Statut :</strong> <span style={{ color: statutColor(virementResultat.statut) }}>{virementResultat.statut}</span></p>
          <p><strong>Montant :</strong> {virementResultat.montant} $</p>
          <p><strong>ID virement :</strong> {virementResultat.virementId}</p>
          <p><strong>Date :</strong> {(() => { const d = new Date(virementResultat.dateVirement); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}, ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}`; })()}</p>
        </div>
      )}

    </div>
  );
}

const styles = {
  clientCard: { background: '#fff', padding: '16px 20px', borderRadius: '8px', boxShadow: '0 2px 6px rgba(0,0,0,0.1)', marginBottom: '24px' },
  clientNom: { fontWeight: 'bold', fontSize: '1.1rem', color: '#1a1a2e', marginBottom: '4px' },
  clientInfo: { fontSize: '0.85rem', color: '#555' },
  grid: { display: 'flex', gap: '16px', flexWrap: 'wrap', marginBottom: '24px' },
  compteCard: { background: '#fff', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 6px rgba(0,0,0,0.1)', minWidth: '180px' },
  compteType: { fontWeight: 'bold', fontSize: '1rem', marginBottom: '8px', color: '#1a1a2e' },
  compteSolde: { fontSize: '1.5rem', fontWeight: 'bold', color: '#27ae60', marginBottom: '8px' },
  compteInfo: { fontSize: '0.8rem', color: '#666' },
  compteId: { fontSize: '0.75rem', color: '#999', marginTop: '4px' },
  section: { background: '#fff', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 6px rgba(0,0,0,0.1)', marginBottom: '16px' },
  row: { display: 'flex', gap: '10px', alignItems: 'center', flexWrap: 'wrap' },
  select: { padding: '8px 10px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '0.95rem' },
  input: { padding: '8px 10px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '0.95rem' },
  btnSm: { padding: '8px 16px', background: '#1a1a2e', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  erreur: { color: '#c0392b', fontSize: '0.9rem', marginTop: '8px' },
  succes: { color: '#27ae60', fontSize: '0.9rem', marginTop: '8px' },
  btnCopy: { padding: '4px 10px', fontSize: '0.75rem', background: '#eee', border: '1px solid #ccc', borderRadius: '4px', cursor: 'pointer' },
  btnFermer: { padding: '4px 10px', fontSize: '0.75rem', background: '#fff', border: '1px solid #c0392b', color: '#c0392b', borderRadius: '4px', cursor: 'pointer' },
  resultatBox: { marginTop: '16px', padding: '16px', border: '2px solid', borderRadius: '6px', background: '#f9f9f9' },
  table: { width: '100%', borderCollapse: 'collapse', marginTop: '12px', fontSize: '0.88rem' },
  th: { textAlign: 'left', padding: '8px 10px', background: '#f5f5f5', borderBottom: '1px solid #ddd', fontWeight: '600', color: '#444' },
  td: { padding: '8px 10px', borderBottom: '1px solid #eee', color: '#333' },
};

function statutColor(statut) {
  if (statut === 'Effectué') return '#27ae60';
  if (statut === 'Suspect') return '#e67e22';
  return '#c0392b';
}
