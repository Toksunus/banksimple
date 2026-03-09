import { BrowserRouter, Routes, Route, Navigate, Link, useNavigate } from 'react-router-dom';
import Inscription from './pages/Inscription.jsx';
import KycPending from './pages/KycPending.jsx';
import Login from './pages/Login.jsx';
import Dashboard from './pages/Dashboard.jsx';
import Virement from './pages/Virement.jsx';

function NavBar() {
  const navigate = useNavigate();
  const isLoggedIn = !!localStorage.getItem('token');

  function deconnexion() {
    localStorage.clear();
    navigate('/login');
  }

  return (
    <nav style={styles.nav}>
      <span style={styles.brand}>BankSimple</span>
      <div style={styles.navLinks}>
        {isLoggedIn && <Link style={styles.link} to="/dashboard">Mes comptes</Link>}
        {isLoggedIn && <Link style={styles.link} to="/virement">Virement</Link>}
        {isLoggedIn && <button style={styles.btnLogout} onClick={deconnexion}>Déconnexion</button>}
      </div>
    </nav>
  );
}

function PrivateRoute({ children }) {
  return localStorage.getItem('token') ? children : <Navigate to="/login" replace />;
}

export default function App() {
  return (
    <BrowserRouter>
      <NavBar />
      <main style={styles.main}>
        <Routes>
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="/inscription" element={<Inscription />} />
          <Route path="/kyc-pending" element={<KycPending />} />
          <Route path="/login" element={<Login />} />
          <Route path="/dashboard" element={<PrivateRoute><Dashboard /></PrivateRoute>} />
          <Route path="/virement" element={<PrivateRoute><Virement /></PrivateRoute>} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}

const styles = {
  nav: {
    display: 'flex', alignItems: 'center', justifyContent: 'space-between',
    padding: '12px 24px', background: '#1a1a2e', color: '#fff',
  },
  brand: { fontSize: '1.2rem', fontWeight: 'bold' },
  navLinks: { display: 'flex', gap: '16px', alignItems: 'center' },
  link: { color: '#a0c4ff', textDecoration: 'none', fontSize: '0.95rem' },
  btnLogout: {
    background: 'transparent', border: '1px solid #a0c4ff', color: '#a0c4ff',
    padding: '4px 12px', cursor: 'pointer', borderRadius: '4px', fontSize: '0.9rem',
  },
  main: { maxWidth: '800px', margin: '40px auto', padding: '0 16px' },
};
