self.addEventListener('install', event => {
  event.waitUntil(caches.open('familyledger-v1').then(c => c.addAll(['/', '/styles.css', '/app.js', '/manifest.webmanifest'])));
});

self.addEventListener('fetch', event => {
  event.respondWith(caches.match(event.request).then(r => r || fetch(event.request)));
});
