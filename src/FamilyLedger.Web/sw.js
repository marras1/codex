const cacheName = 'familyledger-v2';

self.addEventListener('install', event => {
  event.waitUntil(caches.open(cacheName).then(c => c.addAll(['/', '/styles.css?v=20260409', '/app.js?v=20260409', '/manifest.webmanifest?v=20260409'])));
  self.skipWaiting();
});

self.addEventListener('activate', event => {
  event.waitUntil(caches.keys().then(keys => Promise.all(keys.filter(key => key !== cacheName).map(key => caches.delete(key)))));
  self.clients.claim();
});

self.addEventListener('fetch', event => {
  event.respondWith(caches.match(event.request).then(r => r || fetch(event.request)));
});
