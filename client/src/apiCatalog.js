// ---------------------------------------------------------------------------
// Declarative catalog of every endpoint the test client can call.
// The UI is generated from this data, so adding an endpoint = adding an object.
//
// Endpoint shape:
//   { id, label, method, path, desc,
//     pathParams?: [{ name, default }],   // substituted into :name in the path
//     query?:      [{ name, default }],   // appended as ?name=value
//     headers?:    [{ name, default }],   // sent as request headers
//     body?:        <object>,             // JSON body (POST/PUT/PATCH)
//     auth?:        true,                 // attach "Authorization: Bearer <token>"
//     capture?:    'token' }              // on success, save body.token as the JWT
// ---------------------------------------------------------------------------

const productBody = {
  name: 'Clean Code',
  price: 32.99,
  contactEmail: 'shop@example.com',
  zipCode: '12345',
  stock: 10,
  isPhysical: true,
  weight: 1.2,
};

export const catalog = [
  {
    id: 'auth',
    title: '1 · JWT Authentication',
    topic: 'Authentication',
    blurb:
      'Log in to get a JWT. The token is stored globally and auto-attached to every "🔒 auth" request below. Try demo users: admin/admin123, manager/manager123, user/user123.',
    endpoints: [
      {
        id: 'login',
        label: 'Login (capture token)',
        method: 'POST',
        path: '/api/v1/auth/login',
        desc: 'Authenticates and returns a JWT. On success the token is saved for all protected calls.',
        body: { username: 'admin', password: 'admin123' },
        capture: 'token',
      },
      {
        id: 'me',
        label: 'Current user profile',
        method: 'GET',
        path: '/api/v1/auth/me',
        desc: 'Reads the identity from the JWT. Returns 401 if no/invalid token.',
        auth: true,
      },
      {
        id: 'info',
        label: 'Auth system info',
        method: 'GET',
        path: '/api/v1/auth/info',
        desc: 'Public endpoint describing the auth setup.',
      },
      {
        id: 'register',
        label: 'Register new user',
        method: 'POST',
        path: '/api/v1/auth/register',
        desc: 'Creates a user and returns a token.',
        body: {
          username: 'newuser',
          password: 'Passw0rd!',
          email: 'new@example.com',
          firstName: 'New',
          lastName: 'User',
        },
      },
      {
        id: 'logout',
        label: 'Logout',
        method: 'POST',
        path: '/api/v1/auth/logout',
        desc: 'Server-side logout hook (JWT is stateless, so the client also drops its token).',
        auth: true,
      },
    ],
  },
  {
    id: 'lifetimes',
    title: '2 · DI Lifetimes',
    topic: 'Dependency Injection',
    blurb:
      'Call this TWICE and compare the GUIDs. Singleton = same across calls. Scoped = same within a call, differs between calls. Transient = differs even within one call.',
    endpoints: [
      {
        id: 'lifetimes',
        label: 'Singleton vs Scoped vs Transient',
        method: 'GET',
        path: '/api/lifetimes',
        desc: 'Each lifetime stamps itself with a GUID at creation time.',
      },
    ],
  },
  {
    id: 'status',
    title: '3 · HTTP Status Codes',
    topic: 'Status Codes',
    blurb:
      '2xx success · 3xx redirect · 4xx client error · 5xx server error. (Browsers auto-follow 3xx, so those show the final 200.)',
    endpoints: [
      { id: 's200', label: '200 OK', method: 'GET', path: '/api/status-codes/200', desc: 'Standard success.' },
      { id: 's201', label: '201 Created', method: 'POST', path: '/api/status-codes/201', desc: 'Resource created (Location header).' },
      { id: 's204', label: '204 No Content', method: 'DELETE', path: '/api/status-codes/204', desc: 'Success, empty body.' },
      { id: 's400', label: '400 Bad Request', method: 'GET', path: '/api/status-codes/400', desc: 'Invalid input.' },
      { id: 's401', label: '401 Unauthorized', method: 'GET', path: '/api/status-codes/401', desc: 'Not authenticated.' },
      { id: 's403', label: '403 Forbidden', method: 'GET', path: '/api/status-codes/403', desc: 'Authenticated but not allowed.' },
      { id: 's404', label: '404 Not Found', method: 'GET', path: '/api/status-codes/404', desc: 'Resource missing.' },
      { id: 's500', label: '500 Server Error', method: 'GET', path: '/api/status-codes/500', desc: 'Server crashed.' },
    ],
  },
  {
    id: 'binding',
    title: '4 · Model Binding',
    topic: 'Model Binding',
    blurb: 'Where does the data come from? Route, query, header, or body — each has its own [From*] attribute.',
    endpoints: [
      {
        id: 'route',
        label: '[FromRoute]',
        method: 'GET',
        path: '/api/binding/route/:id',
        desc: 'Value comes from the URL path segment.',
        pathParams: [{ name: 'id', default: '5' }],
      },
      {
        id: 'query',
        label: '[FromQuery]',
        method: 'GET',
        path: '/api/binding/query',
        desc: 'Values come from the query string.',
        query: [{ name: 'page', default: '2' }, { name: 'size', default: '10' }],
      },
      {
        id: 'header',
        label: '[FromHeader]',
        method: 'GET',
        path: '/api/binding/header',
        desc: 'Value comes from an HTTP header.',
        headers: [{ name: 'X-Client-Id', default: 'abc-123' }],
      },
      {
        id: 'body',
        label: '[FromBody]',
        method: 'POST',
        path: '/api/binding/body',
        desc: 'Complex object bound from the JSON body.',
        body: productBody,
      },
      {
        id: 'combined',
        label: 'Route + Body combined',
        method: 'PUT',
        path: '/api/binding/combined/:id',
        desc: 'id from the route, fields from the body.',
        pathParams: [{ name: 'id', default: '7' }],
        body: productBody,
      },
    ],
  },
  {
    id: 'methods',
    title: '5 · HTTP Methods (CRUD)',
    topic: 'HTTP Methods',
    blurb: 'GET=read · POST=create · PUT=replace · PATCH=partial · DELETE=remove · OPTIONS=which methods?',
    endpoints: [
      { id: 'getAll', label: 'GET all', method: 'GET', path: '/api/http-methods', desc: 'Read all items.' },
      { id: 'getOne', label: 'GET by id', method: 'GET', path: '/api/http-methods/:id', desc: 'Read one.', pathParams: [{ name: 'id', default: '1' }] },
      { id: 'create', label: 'POST create', method: 'POST', path: '/api/http-methods', desc: 'Create (not idempotent).', body: productBody },
      { id: 'replace', label: 'PUT replace', method: 'PUT', path: '/api/http-methods/:id', desc: 'Replace whole resource.', pathParams: [{ name: 'id', default: '1' }], body: productBody },
      { id: 'patch', label: 'PATCH partial', method: 'PATCH', path: '/api/http-methods/:id', desc: 'Update only sent fields (custom DTO).', pathParams: [{ name: 'id', default: '1' }], body: { name: 'Patched Name', price: 19.99 } },
      {
        id: 'jsonpatch',
        label: 'PATCH (JSON Patch · RFC 6902)',
        method: 'PATCH',
        path: '/api/http-methods/json-patch/:id',
        desc: 'Standard JSON Patch document: an array of ops (add/remove/replace/move/copy/test). Sent as application/json-patch+json.',
        pathParams: [{ name: 'id', default: '1' }],
        contentType: 'application/json-patch+json',
        body: [
          { op: 'replace', path: '/name', value: 'Updated via JSON Patch' },
          { op: 'replace', path: '/price', value: 49.99 },
        ],
      },
      { id: 'delete', label: 'DELETE', method: 'DELETE', path: '/api/http-methods/:id', desc: 'Remove (idempotent).', pathParams: [{ name: 'id', default: '1' }] },
      { id: 'options', label: 'OPTIONS', method: 'OPTIONS', path: '/api/http-methods', desc: 'Lists supported methods (Allow header).' },
    ],
  },
  {
    id: 'pagination',
    title: '5b · Pagination',
    topic: 'Pagination',
    blurb:
      'OFFSET = page/pageSize (simple, "jump to page N", but slow & unstable on deep pages). CURSOR/keyset = pass the opaque "nextCursor" from each response to get the next slice (fast, stable). Try cursor with no cursor, then paste the returned nextCursor.',
    endpoints: [
      {
        id: 'offset',
        label: 'Offset pagination',
        method: 'GET',
        path: '/api/pagination/offset',
        desc: 'Classic page/pageSize with total counts.',
        query: [{ name: 'page', default: '2' }, { name: 'pageSize', default: '5' }],
      },
      {
        id: 'cursor',
        label: 'Cursor (keyset) pagination',
        method: 'GET',
        path: '/api/pagination/cursor',
        desc: 'Leave cursor empty for page 1, then paste the nextCursor from the response here for the next page.',
        query: [{ name: 'cursor', default: '' }, { name: 'limit', default: '5' }],
      },
    ],
  },
  {
    id: 'validation',
    title: '6 · Validation',
    topic: 'Validation',
    blurb:
      'Data Annotations auto-return a 400 (handled by [ApiController]). FluentValidation runs explicitly and supports conditional/custom rules. Try the invalid bodies to see the errors.',
    endpoints: [
      {
        id: 'annotations',
        label: 'Data Annotations (invalid body)',
        method: 'POST',
        path: '/api/validation/data-annotations',
        desc: 'Auto 400 from [ApiController]. Edit the body to make it valid.',
        body: { name: 'ab', price: -1, contactEmail: 'not-an-email', zipCode: '1' },
      },
      {
        id: 'fluent',
        label: 'FluentValidation (invalid body)',
        method: 'POST',
        path: '/api/validation/fluent',
        desc: 'Shows conditional (weight) + custom (no "test") rules.',
        body: { name: 'test', price: -1, contactEmail: 'x', zipCode: '1', isPhysical: true, weight: 0 },
      },
    ],
  },
  {
    id: 'caching',
    title: '7 · Caching',
    topic: 'Caching',
    blurb:
      'Call "memory" twice within 30s — generatedAt stays frozen (cache hit). "response" adds a Cache-Control header.',
    endpoints: [
      { id: 'memory', label: 'In-memory cache', method: 'GET', path: '/api/caching/memory', desc: 'First call computes, next 30s are cached.' },
      { id: 'clear', label: 'Clear cache', method: 'DELETE', path: '/api/caching/memory', desc: 'Evicts the cached value.' },
      { id: 'response', label: 'Response cache', method: 'GET', path: '/api/caching/response', desc: 'Adds Cache-Control: public, max-age=30.' },
      { id: 'nocache', label: 'No cache', method: 'GET', path: '/api/caching/no-cache', desc: 'Explicitly never cached.' },
    ],
  },
  {
    id: 'logging',
    title: '8 · Logging',
    topic: 'Logging',
    blurb: 'These write to the API server console. Watch the dotnet terminal after calling them.',
    endpoints: [
      { id: 'levels', label: 'All log levels', method: 'GET', path: '/api/logging/all-levels', desc: 'Trace → Critical.' },
      { id: 'exception', label: 'Log an exception', method: 'GET', path: '/api/logging/exception', desc: 'Logs with stack trace.' },
      { id: 'scope', label: 'Logging scope', method: 'GET', path: '/api/logging/scope', desc: 'Groups logs under an OrderId.' },
    ],
  },
  {
    id: 'filters',
    title: '9 · Filters Pipeline',
    topic: 'Filters',
    blurb:
      'Watch the server console for filter order. "pipeline" also adds an X-Demo-Result-Filter response header (see the Headers tab).',
    endpoints: [
      { id: 'pipeline', label: 'Resource/Action/Result filters', method: 'GET', path: '/api/filters/pipeline', desc: 'Filters fire around the action.' },
      { id: 'exception', label: 'Exception filter', method: 'GET', path: '/api/filters/exception', desc: 'Action throws; filter returns a clean 500.' },
    ],
  },
  {
    id: 'security',
    title: '10 · Security & Crypto',
    topic: 'Security',
    blurb: 'Password hashing (PBKDF2), AES symmetric encryption, and HMAC integrity.',
    endpoints: [
      { id: 'hash', label: 'Hash password', method: 'POST', path: '/api/security/hash', desc: 'Salted one-way hash; verifies right vs wrong.', body: { password: 'secret123' } },
      { id: 'encrypt', label: 'Encrypt / decrypt', method: 'POST', path: '/api/security/encrypt', desc: 'AES round-trip.', body: { text: 'sensitive data' } },
      { id: 'hmac', label: 'HMAC signature', method: 'POST', path: '/api/security/hmac', desc: 'Detects tampering.', body: { text: 'message to sign' } },
    ],
  },
  {
    id: 'authorization',
    title: '11 · Authorization',
    topic: 'Authorization',
    blurb:
      '🔒 Requires a token (login first). Role/Claims/Policy based. Different demo users get 200 vs 403 — log in as admin vs user to compare.',
    endpoints: [
      { id: 'role', label: 'Role: admin-only', method: 'GET', path: '/api/v1/role-demo/admin-only', desc: 'Requires Admin role.', auth: true },
      { id: 'roleMgr', label: 'Role: manager-or-admin', method: 'GET', path: '/api/v1/role-demo/manager-or-admin', desc: 'Requires Manager or Admin.', auth: true },
      { id: 'claimsMine', label: 'Claims: my-claims', method: 'GET', path: '/api/v1/claims-demo/my-claims', desc: 'Lists the JWT claims.', auth: true },
      { id: 'claimsSec', label: 'Claims: high-security', method: 'GET', path: '/api/v1/claims-demo/high-security', desc: 'Requires security_level 3+.', auth: true },
      { id: 'claimsIt', label: 'Claims: IT department', method: 'GET', path: '/api/v1/claims-demo/it-department-only', desc: 'Requires department=IT claim.', auth: true },
      { id: 'policy', label: 'Policy: security-level-2', method: 'GET', path: '/api/v1/policy-demo/security-level-2', desc: 'Custom requirement + handler.', auth: true },
    ],
  },
  {
    id: 'versioning',
    title: '12 · API Versioning',
    topic: 'API Versioning',
    blurb: 'Same resource, different versions: URL path (v1/v2) and query string both work.',
    endpoints: [
      { id: 'v1', label: 'URL path v1', method: 'GET', path: '/api/v1/books', desc: 'Version 1 shape.' },
      { id: 'v2', label: 'URL path v2', method: 'GET', path: '/api/v2/books', desc: 'Version 2 shape (richer).' },
      { id: 'q', label: 'Query string version', method: 'GET', path: '/api/books', desc: 'Pick version via ?version=.', query: [{ name: 'version', default: '2.0' }] },
      { id: 'vinfo', label: 'Version info', method: 'GET', path: '/api/v1/books/version-info', desc: 'Echoes the resolved version.' },
    ],
  },
  {
    id: 'ratelimit',
    title: '12b · Rate Limiting',
    topic: 'Rate Limiting (sliding window)',
    blurb:
      'Sliding-window limiter: 5 requests per 15s (3 × 5s segments), scoped to this controller only. Click "Burst ×8" to fire 8 at once — 5 return 200, the rest 429 with a Retry-After header. Wait ~5s and a segment frees up.',
    endpoints: [
      {
        id: 'rl',
        label: 'Rate-limited endpoint',
        method: 'GET',
        path: '/api/rate-limit',
        desc: 'Allowed until the window limit is hit, then 429 Too Many Requests.',
        burst: 8,
      },
      {
        id: 'rlu',
        label: 'Unlimited (DisableRateLimiting)',
        method: 'GET',
        path: '/api/rate-limit/unlimited',
        desc: 'Same controller, limiter disabled — burst never throttles.',
        burst: 8,
      },
    ],
  },
  {
    id: 'books',
    title: '13 · Bookstore CRUD',
    topic: 'Real-world CRUD',
    blurb:
      'The "real" domain API, using the explicit v1 routes. (The non-versioned /api/books path is ambiguous between the versioned and non-versioned controllers — a known issue — so we call /api/v1/books directly.)',
    endpoints: [
      { id: 'list', label: 'List all', method: 'GET', path: '/api/v1/books', desc: 'All books (v1 shape).' },
      { id: 'get', label: 'Get by id', method: 'GET', path: '/api/v1/books/:id', desc: 'Single book.', pathParams: [{ name: 'id', default: '1' }] },
      { id: 'search', label: 'Search', method: 'GET', path: '/api/v1/books/search', desc: 'Search by title/author.', query: [{ name: 'query', default: 'code' }] },
      {
        id: 'create',
        label: 'Create',
        method: 'POST',
        path: '/api/v1/books',
        desc: 'Creates a book (v1 request shape).',
        body: {
          title: 'The Pragmatic Programmer',
          author: 'Hunt & Thomas',
          price: 39.99,
          year: 2019,
        },
      },
    ],
  },
];
