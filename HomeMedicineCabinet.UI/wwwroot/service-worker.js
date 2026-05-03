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

self.addEventListener("push", event => {
    let data = {
        title: "Домашняя аптечка",
        message: "Новое уведомление",
        url: "/Notifications",
        intakeLogId: null
    };

    if (event.data) {
        data = event.data.json();
    }

    const actions = [];

    if (data.intakeLogId) {
        actions.push(
            {
                action: "taken",
                title: "Принял"
            },
            {
                action: "skipped",
                title: "Пропустить"
            }
        );
    }

    const options = {
        body: data.message,
        icon: "/icons/icon-192.png",
        badge: "/icons/icon-192.png",
        data: {
            url: data.url || "/Notifications",
            intakeLogId: data.intakeLogId
        },
        actions: actions
    };

    event.waitUntil(
        self.registration.showNotification(data.title, options)
    );
});

self.addEventListener("notificationclick", event => {
    event.notification.close();

    const data = event.notification.data || {};
    const intakeLogId = data.intakeLogId;

    if (event.action === "taken" && intakeLogId) {
        event.waitUntil(
            updateIntakeStatus(intakeLogId, "Taken")
        );
        return;
    }

    if (event.action === "skipped" && intakeLogId) {
        event.waitUntil(
            updateIntakeStatus(intakeLogId, "Skipped")
        );
        return;
    }

    event.waitUntil(
        clients.openWindow(data.url || "/Notifications")
    );
});

async function updateIntakeStatus(intakeLogId, status) {
    await fetch("/IntakeLogs/SetStatusFromNotification", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: `id=${encodeURIComponent(intakeLogId)}&status=${encodeURIComponent(status)}`
    });
}