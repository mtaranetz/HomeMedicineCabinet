const CACHE_NAME = "home-medicine-cabinet-v1";

const STATIC_ASSETS = [
    "/",
    "/manifest.json",
    "/css/site.css",
    "/js/notifications.js"
];

self.addEventListener("install", event => {
    console.log("[ServiceWorker] Installed");

    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(STATIC_ASSETS))
    );

    self.skipWaiting();
});

self.addEventListener("activate", event => {
    console.log("[ServiceWorker] Activated");

    event.waitUntil(
        caches.keys().then(cacheNames =>
            Promise.all(
                cacheNames
                    .filter(cacheName => cacheName !== CACHE_NAME)
                    .map(cacheName => caches.delete(cacheName))
            )
        )
    );

    self.clients.claim();
});

self.addEventListener("fetch", event => {
    if (event.request.method !== "GET") {
        return;
    }

    event.respondWith(
        fetch(event.request).catch(() => caches.match(event.request))
    );
});