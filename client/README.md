# ⚡ WebCoreAPI · React Test Client

A small **React + Vite** single-page app that calls **every endpoint** of the `WebCoreAPI` ASP.NET Core server, so you can _see_ each Web API concept work from a real browser front-end.

It logs in, captures the **JWT**, auto-attaches it to protected calls, and shows the **status code, timing, response body, and response headers** for each request.

---

## 🚀 Run it (two terminals)

**Terminal 1 — start the API:**

```bash
cd ..                       # repo root
dotnet run --project WebCoreAPI
# API listening on http://localhost:5274
```

**Terminal 2 — start the client:**

```bash
cd client
npm install                 # first time only
npm run dev
# opens http://localhost:5173
```

Then in the browser:

1. Open **section 1 · JWT Authentication** → click **Send** on _Login_ (defaults to `admin / admin123`).
2. The top bar flips to **🔓 admin** — the token is now attached to every **🔒** request.
3. Explore the other topics in the sidebar and click **Send**.

> The API has CORS `AllowAll` enabled, so the browser at `:5173` can call the API at `:5274` cross-origin. If your API runs elsewhere, change the **API base URL** box in the top bar.

---

## 🧠 How it works (the interesting parts)

```
┌──────────────────────────────────────────────────────────────┐
│  App.jsx               sidebar nav + topbar (base URL, auth)   │
│   └─ ApiProvider (api.jsx)   global state: baseUrl, token, call()│
│        └─ Endpoint.jsx        one "try it" card per endpoint     │
│             reads ← apiCatalog.js  (declarative list of routes)  │
└──────────────────────────────────────────────────────────────┘
```

| File | Responsibility |
|------|----------------|
| [`src/apiCatalog.js`](src/apiCatalog.js) | **Declarative** list of every endpoint grouped by topic. Add an object → a new tester appears. No JSX needed. |
| [`src/api.jsx`](src/api.jsx) | React **context** holding `baseUrl`, the JWT `token` (persisted in `localStorage`), and the single `call()` fetch wrapper that attaches `Authorization: Bearer <token>`. |
| [`src/components/Endpoint.jsx`](src/components/Endpoint.jsx) | Generic UI for one endpoint: editable path params / query / headers / JSON body, a **Send** button, and a response panel (status badge, timing, body, headers). Captures the token on login. |
| [`src/App.jsx`](src/App.jsx) | Layout: top bar (base URL + login chip) and sidebar of topics. |
| [`src/styles.css`](src/styles.css) | Dark theme matching the study guide. |

**Why this design?** It mirrors good front-end practice and is easy to explain in an interview:

- **Single source of truth** for the API surface (`apiCatalog.js`) — data-driven UI.
- **One HTTP wrapper** (`call()`) — the React equivalent of a typed `HttpClient` with an auth handler; cross-cutting concerns (base URL, bearer token, JSON parsing, error shaping) live in one place.
- **Context for auth state** so the token set by the login card is instantly available to every other card.
- **Errors are data, not exceptions** — a 404/500 is rendered, never thrown, so you always see what the server returned.

---

## 🗂️ Topics covered

JWT Auth · DI Lifetimes · Status Codes · Model Binding · HTTP Methods (CRUD) · Validation (Data Annotations + FluentValidation) · Caching · Logging · Filters · Security (hash/AES/HMAC) · Authorization (role/claims/policy) · API Versioning · Bookstore CRUD.

For the **theory** behind each, read [`../LEARNING-GUIDE.md`](../LEARNING-GUIDE.md).
For **how client + server fit together**, read [`../doc/FULLSTACK-WALKTHROUGH.md`](../doc/FULLSTACK-WALKTHROUGH.md).

---

## 🛠️ Tech

React 18 · Vite 5 · plain JavaScript (no TypeScript, no UI library) · `fetch` · `localStorage`. Zero backend coupling — it only speaks HTTP/JSON.
