console.log("notifications.js loaded");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", function (title, message) {
    console.log("Notification received:", title, message);

    const titleElement = document.getElementById("notificationTitle");
    const messageElement = document.getElementById("notificationMessage");
    const toastElement = document.getElementById("notificationToast");

    titleElement.textContent = title;
    messageElement.textContent = message;

    const toast = new bootstrap.Toast(toastElement, {
        delay: 8000
    });

    toast.show();
});

connection.start()
    .then(function () {
        console.log("SignalR connected");
    })
    .catch(function (err) {
        console.error("SignalR error:", err.toString());
    });