const cacheName = 'gym26-v1';
const assetsToCache = [
    '/',
    '/index.html',
    '/css/gym-styles.css',
    '/css/site.css',// Asegúrate de que coincida con tu ruta
    '/js/site.js'
];

self.addEventListener('install', event => {
    event.waitUntil(caches.open(cacheName).then(cache => cache.addAll(assetsToCache)));
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => response || fetch(event.request))
    );
});