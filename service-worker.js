// MrTamal Service Worker v1
const CACHE_NAME = 'mrtamal-v1';

// Archivos críticos a cachear para funcionamiento offline básico
const PRECACHE = [
  '/mrtamal/',
  '/mrtamal/index.html',
  '/mrtamal/css/bootstrap/bootstrap.min.css',
  '/mrtamal/css/app.css',
  '/mrtamal/icon-192.png',
  '/mrtamal/favicon.png'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => cache.addAll(PRECACHE))
  );
  self.skipWaiting();
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys =>
      Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
    )
  );
  self.clients.claim();
});

self.addEventListener('fetch', event => {
  const url = new URL(event.request.url);

  // No cachear llamadas al API
  if (url.hostname === 'mrtamal.onrender.com') return;

  // Para navegación: network first, fallback a index.html (SPA)
  if (event.request.mode === 'navigate') {
    event.respondWith(
      fetch(event.request).catch(() =>
        caches.match('/mrtamal/index.html')
      )
    );
    return;
  }

  // Para assets: cache first
  event.respondWith(
    caches.match(event.request).then(cached => {
      if (cached) return cached;
      return fetch(event.request).then(response => {
        if (response && response.status === 200 && response.type === 'basic') {
          const clone = response.clone();
          caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
        }
        return response;
      }).catch(() => cached);
    })
  );
});
