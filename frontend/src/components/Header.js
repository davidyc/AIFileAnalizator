import { Link } from 'react-router-dom';
import './Header.css';

function Header() {
  return (
    <header className="app-header">
      <div className="app-logo">🧠 AI Анализ</div>
      <nav className="app-nav">
        <Link to="/" className="nav-link">
          Chat
        </Link>
        <Link to="/rag" className="nav-link">
          Rag
        </Link>
      </nav>
    </header>
  );
}

export default Header;
