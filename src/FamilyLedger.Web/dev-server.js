const http = require('http');
const fs = require('fs');
const path = require('path');

const host = process.env.HOST || '0.0.0.0';
const port = Number(process.env.PORT || 8081);
const apiHost = process.env.API_HOST || '127.0.0.1';
const apiPort = Number(process.env.API_PORT || 5000);
const root = __dirname;

const types = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.webmanifest': 'application/manifest+json; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png'
};

function send(res, status, body, headers = {}) {
  res.writeHead(status, headers);
  res.end(body);
}

function serveFile(req, res) {
  let pathname = new URL(req.url, `http://${req.headers.host}`).pathname;
  if (pathname === '/') pathname = '/index.html';
  const filePath = path.join(root, pathname);
  if (!filePath.startsWith(root)) return send(res, 403, 'Forbidden');

  fs.readFile(filePath, (err, data) => {
    if (err) return send(res, 404, 'Not found');
    send(res, 200, data, { 'Content-Type': types[path.extname(filePath)] || 'application/octet-stream' });
  });
}

function proxyApi(req, res) {
  const upstream = http.request({
    hostname: apiHost,
    port: apiPort,
    path: req.url,
    method: req.method,
    headers: req.headers
  }, upstreamRes => {
    res.writeHead(upstreamRes.statusCode || 502, upstreamRes.headers);
    upstreamRes.pipe(res);
  });

  upstream.on('error', err => {
    send(res, 502, JSON.stringify({
      title: 'API unavailable',
      detail: `Could not reach FamilyLedger API at http://${apiHost}:${apiPort}: ${err.message}`
    }), { 'Content-Type': 'application/json; charset=utf-8' });
  });

  req.pipe(upstream);
}

http.createServer((req, res) => {
  if ((req.url || '').startsWith('/api/')) return proxyApi(req, res);
  serveFile(req, res);
}).listen(port, host, () => {
  console.log(`FamilyLedger web on http://127.0.0.1:${port}`);
  console.log(`Proxying /api to http://${apiHost}:${apiPort}`);
});
