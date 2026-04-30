async function enablePushNotifications() {
    try {
        if (!("serviceWorker" in navigator)) {
            alert("Service Worker не поддерживается");
            return;
        }

        if (!("PushManager" in window)) {
            alert("Push-уведомления не поддерживаются");
            return;
        }

        const permission = await Notification.requestPermission();

        if (permission !== "granted") {
            alert("Вы не разрешили уведомления");
            return;
        }

        const registration = await navigator.serviceWorker.getRegistration();

        if (!registration) {
            alert("Service Worker не найден");
            return;
        }

        const keyResponse = await fetch("/PushSubscriptions/PublicKey");

        if (!keyResponse.ok) {
            console.error("Failed to fetch public key");
            return;
        }

        const keyData = await keyResponse.json();

        let subscription = await registration.pushManager.getSubscription();

        if (!subscription) {
            subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(keyData.publicKey)
            });
        }

        const response = await fetch("/PushSubscriptions/Subscribe", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(subscription)
        });

        if (!response.ok) {
            console.error("Failed to save subscription");
            return;
        }

        alert("Push-уведомления включены");
    } catch (error) {
        console.error("Push error:", error);
        alert("Ошибка при включении push-уведомлений");
    }
}

function urlBase64ToUint8Array(base64String) {
    const padding = "=".repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/-/g, "+")
        .replace(/_/g, "/");

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }

    return outputArray;
}

document.addEventListener("DOMContentLoaded", async () => {
    if (!("serviceWorker" in navigator)) return;

    const reg = await navigator.serviceWorker.getRegistration();

    if (!reg) return;

    const sub = await reg.pushManager.getSubscription();

    if (sub) {
        const button = document.querySelector('[onclick="enablePushNotifications()"]');
        if (button) {
            button.textContent = "Push включен";
            button.classList.remove("btn-outline-primary");
            button.classList.add("btn-success");
        }
    }
});