import { useState } from 'react';
import { useApi } from '../api.jsx';

const METHOD_COLORS = {
  GET: 'm-get',
  POST: 'm-post',
  PUT: 'm-put',
  PATCH: 'm-patch',
  DELETE: 'm-delete',
  OPTIONS: 'm-options',
  HEAD: 'm-head',
};

function statusClass(status) {
  if (status === 0) return 's-err';
  if (status < 300) return 's-2xx';
  if (status < 400) return 's-3xx';
  if (status < 500) return 's-4xx';
  return 's-5xx';
}

// A self-contained "try it" card for a single endpoint, generated from a catalog entry.
export default function Endpoint({ def }) {
  const { call, setToken, setLoggedInUser, token } = useApi();

  // Local, editable copies of every input.
  const [pathParams, setPathParams] = useState(
    () => Object.fromEntries((def.pathParams || []).map((p) => [p.name, p.default]))
  );
  const [query, setQuery] = useState(
    () => Object.fromEntries((def.query || []).map((p) => [p.name, p.default]))
  );
  const [headers, setHeaders] = useState(
    () => Object.fromEntries((def.headers || []).map((p) => [p.name, p.default]))
  );
  const [bodyText, setBodyText] = useState(
    () => (def.body ? JSON.stringify(def.body, null, 2) : '')
  );
  const [res, setRes] = useState(null);
  const [loading, setLoading] = useState(false);
  const [tab, setTab] = useState('body');

  const buildPath = () => {
    let p = def.path;
    Object.entries(pathParams).forEach(([k, v]) => {
      p = p.replace(`:${k}`, encodeURIComponent(v));
    });
    return p;
  };

  const send = async () => {
    setLoading(true);
    let parsedBody;
    if (bodyText.trim()) {
      try {
        parsedBody = JSON.parse(bodyText);
      } catch {
        setRes({ status: 0, statusText: 'Invalid JSON body', body: null, headers: {}, durationMs: 0 });
        setLoading(false);
        return;
      }
    }

    const result = await call({
      method: def.method,
      path: buildPath(),
      query,
      headers,
      body: parsedBody,
      auth: def.auth,
    });

    // Side effects: capture token on login, drop it on logout.
    if (def.capture === 'token' && result.ok && result.body?.token) {
      setToken(result.body.token);
      if (result.body.user) setLoggedInUser(result.body.user);
    }
    if (def.id === 'logout' && result.ok) {
      setToken('');
      setLoggedInUser(null);
    }

    setRes(result);
    setTab('body');
    setLoading(false);
  };

  const needsAuthButNoToken = def.auth && !token;

  return (
    <div className="endpoint">
      <div className="endpoint-head">
        <span className={`method ${METHOD_COLORS[def.method] || ''}`}>{def.method}</span>
        <code className="path">{def.path}</code>
        {def.auth && <span className="lock" title="Requires JWT">🔒</span>}
        <button className="send" onClick={send} disabled={loading}>
          {loading ? '…' : 'Send'}
        </button>
      </div>

      <p className="endpoint-desc">{def.desc}</p>
      {needsAuthButNoToken && (
        <p className="warn">⚠️ This needs a token — log in first (section 1).</p>
      )}

      {/* Inputs */}
      {(def.pathParams || []).length > 0 && (
        <Inputs label="Path params" values={pathParams} onChange={setPathParams} />
      )}
      {(def.query || []).length > 0 && (
        <Inputs label="Query" values={query} onChange={setQuery} />
      )}
      {(def.headers || []).length > 0 && (
        <Inputs label="Headers" values={headers} onChange={setHeaders} />
      )}
      {def.body !== undefined && (
        <div className="field">
          <label>Body (JSON)</label>
          <textarea
            value={bodyText}
            onChange={(e) => setBodyText(e.target.value)}
            spellCheck={false}
            rows={Math.min(14, bodyText.split('\n').length + 1)}
          />
        </div>
      )}

      {/* Response */}
      {res && (
        <div className="response">
          <div className="response-head">
            <span className={`status ${statusClass(res.status)}`}>
              {res.status || '—'} {res.statusText}
            </span>
            <span className="muted">{res.durationMs} ms</span>
            <span className="muted finalurl">{res.method} {res.finalUrl}</span>
            <span className="tabs">
              <button className={tab === 'body' ? 'on' : ''} onClick={() => setTab('body')}>Body</button>
              <button className={tab === 'headers' ? 'on' : ''} onClick={() => setTab('headers')}>Headers</button>
            </span>
          </div>
          {res.error && <p className="warn">{res.error}</p>}
          {tab === 'body' && (
            <pre className="json">
              {typeof res.body === 'string'
                ? res.body
                : JSON.stringify(res.body, null, 2)}
            </pre>
          )}
          {tab === 'headers' && (
            <pre className="json">{JSON.stringify(res.headers, null, 2)}</pre>
          )}
        </div>
      )}
    </div>
  );
}

function Inputs({ label, values, onChange }) {
  return (
    <div className="field">
      <label>{label}</label>
      <div className="inputs-row">
        {Object.entries(values).map(([k, v]) => (
          <span key={k} className="kv">
            <span className="k">{k}</span>
            <input
              value={v}
              onChange={(e) => onChange({ ...values, [k]: e.target.value })}
            />
          </span>
        ))}
      </div>
    </div>
  );
}
