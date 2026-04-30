console.log("notifications.js loaded");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", function (title, message, notificationId, intakeLogId) {
    console.log("Notification received:", title, message, notificationId, intakeLogId);

    const titleElement = document.getElementById("notificationTitle");
    const messageElement = document.getElementById("notificationMessage");
    const toastElement = document.getElementById("notificationToast");

    titleElement.textContent = title;
    messageElement.innerHTML = `
        <div>${message}</div>

        ${intakeLogId ? `
            <div class="mt-2 d-flex gap-2">
                <button class="btn btn-success btn-sm" onclick="setIntakeStatus(${intakeLogId}, 'Taken')">
                    Принял
                </button>
                <button class="btn btn-outline-secondary btn-sm" onclick="setIntakeStatus(${intakeLogId}, 'Skipped')">
                    Пропустить
                </button>
            </div>
        ` : ""}
    `;

    const toast = new bootstrap.Toast(toastElement, { delay: 8000 });
    toast.show();
});

async function setIntakeStatus(intakeLogId, status) {
    const response = await fetch("/IntakeLogs/SetStatusFromNotification", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: `id=${intakeLogId}&status=${status}`
    });

    if (!response.ok) {
        console.error("Failed to update intake status");
        return;
    }

    const toastElement = document.getElementById("notificationToast");
    const toast = bootstrap.Toast.getInstance(toastElement);

    if (toast) {
        toast.hide();
    }
}

connection.start()
    .then(function () {
        console.log("SignalR connected");
    })
    .catch(function (err) {
        console.error("SignalR error:", err.toString());
    });