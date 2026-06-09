import { createContext, useContext, useState, useCallback, useMemo } from 'react';

const DEFAULT_BASE_URL = 'http://localhost:5274';

const ApiContext = createContext(null);

export function useApi() {
  return useContext(ApiContext);
}

/**
 * Provides global API state to the whole app:
 *  - baseUrl   : where the ASP.NET Core API is running
 *  - token     : the JWT captured after login (persisted in localStorage)
 *  - user      : the logged-in user object returned by /auth/login
 *  - call()    : the single fetch wrapper every Endpoint uses
 *
 * This is the React equivalent of an "HttpClient + auth handler" on the server:
 * one place that knows the base address and automatically attaches the bearer token.
 */
export function ApiProvider({ children }) {
  const [baseUrl, setBaseUrl] = useState(
    () => localStorage.getItem('baseUrl') || DEFAULT_BASE_URL
  );
  const [token, setTokenState] = useState(() => localStorage.getItem('token') || '');
  const [user, setUser] = useState(() => {
    const raw = localStorage.getItem('user');
    return raw ? JSON.parse(raw) : null;
  });

  const persistBaseUrl = useCallback((url) => {
    setBaseUrl(url);
    localStorage.setItem('baseUrl', url);
  }, []);

  const setToken = useCallback((t) => {
    setTokenState(t || '');
    if (t) localStorage.setItem('token', t);
    else localStorage.removeItem('token');
  }, []);

  const setLoggedInUser = useCallback((u) => {
    setUser(u || null);
    if (u) localStorage.setItem('user', JSON.stringify(u));
    else localStorage.removeItem('user');
  }, []);

  const logout = useCallback(() => {
    setToken('');
    setLoggedInUser(null);
  }, [setToken, setLoggedInUser]);

  /**
   * The core request function. Returns a normalized result object:
   *   { ok, status, statusText, durationMs, body, headers, error }
   * Never throws for HTTP errors — a 404/500 is data we want to display.
   */
  const call = useCallback(
    async ({ method = 'GET', path, query, headers = {}, body, auth = false, contentType }) => {
      // Build the URL with query string.
      const url = new URL(path.replace(/^\//, ''), baseUrl.replace(/\/?$/, '/'));
      if (query) {
        Object.entries(query).forEach(([k, v]) => {
          if (v !== '' && v !== undefined && v !== null) url.searchParams.set(k, v);
        });
      }

      const finalHeaders = { ...headers };
      let payload;
      if (body !== undefined && body !== null && body !== '') {
        // Allow a custom content type (e.g. application/json-patch+json for JSON Patch).
        finalHeaders['Content-Type'] = contentType || 'application/json';
        payload = typeof body === 'string' ? body : JSON.stringify(body);
      }
      if (auth && token) {
        finalHeaders['Authorization'] = `Bearer ${token}`;
      }

      const started = performance.now();
      try {
        const res = await fetch(url.toString(), { method, headers: finalHeaders, body: payload });
        const durationMs = Math.round(performance.now() - started);

        // Parse body as JSON when possible, otherwise text.
        const text = await res.text();
        let parsed;
        try {
          parsed = text ? JSON.parse(text) : null;
        } catch {
          parsed = text;
        }

        const headerObj = {};
        res.headers.forEach((value, key) => {
          headerObj[key] = value;
        });

        return {
          ok: res.ok,
          status: res.status,
          statusText: res.statusText,
          durationMs,
          body: parsed,
          headers: headerObj,
          finalUrl: url.toString(),
          method,
        };
      } catch (err) {
        const durationMs = Math.round(performance.now() - started);
        return {
          ok: false,
          status: 0,
          statusText: 'Network error',
          durationMs,
          body: null,
          headers: {},
          finalUrl: url.toString(),
          method,
          error:
            'Request failed. Is the API running at ' +
            baseUrl +
            '? (' +
            err.message +
            ')',
        };
      }
    },
    [baseUrl, token]
  );

  const value = useMemo(
    () => ({
      baseUrl,
      setBaseUrl: persistBaseUrl,
      token,
      setToken,
      user,
      setLoggedInUser,
      logout,
      call,
    }),
    [baseUrl, persistBaseUrl, token, setToken, user, setLoggedInUser, logout, call]
  );

  return <ApiContext.Provider value={value}>{children}</ApiContext.Provider>;
}
