namespace RealState_Platform.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to a group based on userId
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }

            await base.OnConnectedAsync();
        }
        public async Task SendNotificationToUser(string userId, string title, string message, string type = "info")
        {
            await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", new
            {
                title = title,
                message = message,
                type = type, // info, success, warning, error
                timestamp = DateTime.UtcNow
            });
        }
        public async Task BroadcastNotification(string title, string message, string type = "info")
        {
            await Clients.All.SendAsync("ReceiveNotification", new
            {
                title = title,
                message = message,
                type = type,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task SendAdminNotification(string title, string message, string type = "info")
        {
            await Clients.Group("admins").SendAsync("ReceiveNotification", new
            {
                title = title,
                message = message,
                type = type,
                isAdminOnly = true,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task RegisterAsAdmin()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }
        public async Task SendUpdateNotification(string propertyId, string updateType, string message)
        {
            await Clients.All.SendAsync("ReceiveUpdateNotification", new
            {
                propertyId = propertyId,
                updateType = updateType, // new_property, price_update, status_change
                message = message,
                timestamp = DateTime.UtcNow
            });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
