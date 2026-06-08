import { useState } from 'react';
import { ApiProvider, useApi } from './api.jsx';
import { catalog } from './apiCatalog.js';
import Endpoint from './components/Endpoint.jsx';

function TopBar() {
  const { baseUrl, setBaseUrl, token, user, logout } = useApi();
  return (
    <header className="topbar">
      <div className="brand">
        <span className="logo">⚡</span>
        <div>
          <h1>WebCoreAPI · Test Client</h1>
          <p>React front-end for the ASP.NET Core Web API demo</p>
        </div>
      </div>
      <div className="topbar-right">
        <label className="baseurl">
          API base URL
          <input value={baseUrl} onChange={(e) => setBaseUrl(e.target.value)} />
        </label>
        <div className={`authchip ${token ? 'on' : ''}`}>
          {token ? (
            <>
              <span>🔓 {user?.username || 'logged in'}</span>
              <button onClick={logout}>Logout</button>
            </>
          ) : (
            <span>🔒 not logged in</span>
          )}
        </div>
      </div>
    </header>
  );
}

function Shell() {
  const [active, setActive] = useState(catalog[0].id);
  const section = catalog.find((s) => s.id === active);

  return (
    <div className="layout">
      <nav className="sidebar">
        <div className="nav-title">Topics</div>
        {catalog.map((s) => (
          <button
            key={s.id}
            className={`nav-item ${active === s.id ? 'on' : ''}`}
            onClick={() => setActive(s.id)}
          >
            {s.title}
          </button>
        ))}
        <div className="nav-foot">
          Start the API with <code>dotnet run --project WebCoreAPI</code>, then explore
          each topic. See <code>LEARNING-GUIDE.md</code> for the theory.
        </div>
      </nav>

      <main className="content">
        <section className="section">
          <h2>{section.title}</h2>
          <span className="topic-badge">{section.topic}</span>
          <p className="blurb">{section.blurb}</p>
          <div className="endpoints">
            {section.endpoints.map((def) => (
              <Endpoint key={def.id} def={def} />
            ))}
          </div>
        </section>
      </main>
    </div>
  );
}

export default function App() {
  return (
    <ApiProvider>
      <TopBar />
      <Shell />
    </ApiProvider>
  );
}
