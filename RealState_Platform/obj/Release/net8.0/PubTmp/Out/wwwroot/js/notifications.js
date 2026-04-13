// SignalR Notification Connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Counter for notification IDs
let notificationId = 0;

// Start the connection
connection.start().catch(err => console.error("SignalR Connection Error:", err));

// Listen for notifications
connection.on("ReceiveNotification", function (notification) {
    showNotification(notification.title, notification.message, notification.type);
});

// Listen for update notifications
connection.on("ReceiveUpdateNotification", function (notification) {
    showNotification(notification.updateType, notification.message, "info");
});

// Show Toast Notification
function showNotification(title, message, type = "info") {
    const uniqueId = `notification-${notificationId++}`;
    
    // Determine toast colors based on type
    let bgClass = "bg-info";
    let iconClass = "bi-info-circle";
    
    switch(type) {
        case "success":
            bgClass = "bg-success";
            iconClass = "bi-check-circle";
            break;
        case "warning":
            bgClass = "bg-warning";
            iconClass = "bi-exclamation-triangle";
            break;
        case "error":
            bgClass = "bg-danger";
            iconClass = "bi-x-circle";
            break;
        case "info":
        default:
            bgClass = "bg-info";
            iconClass = "bi-info-circle";
    }
    
    // Create toast HTML
    const toastHTML = `
        <div id="${uniqueId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true" style="min-width: 300px;">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong>
                    <br>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    // Add to container
    const container = document.getElementById("notificationContainer");
    const toastElement = document.createElement("div");
    toastElement.innerHTML = toastHTML;
    container.appendChild(toastElement);
    
    // Show toast
    const toastEl = document.getElementById(uniqueId);
    const bsToast = new bootstrap.Toast(toastEl, { delay: 5000 });
    bsToast.show();
    
    // Remove from DOM after hiding
    toastEl.addEventListener("hidden.bs.toast", function() {
        toastElement.remove();
    });
}

// Handle connection state changes
connection.onreconnected(connectionId => {
    console.log("Reconnected to NotificationHub");
});

connection.onreconnecting(error => {
    console.log("Attempting to reconnect...", error);
});

connection.onclose(error => {
    console.log("Disconnected from NotificationHub", error);
});
