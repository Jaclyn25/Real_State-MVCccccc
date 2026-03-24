namespace RealState_Platform.Hubs
{
    [Authorize]
    public class InquiryChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to personal group for receiving inquiry updates
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-inquiries-{userId}");
            }

            await base.OnConnectedAsync();
        }
        public async Task JoinInquiryChat(int inquiryId)
        {
            var groupName = $"inquiry-{inquiryId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            await Clients.Group(groupName).SendAsync("UserJoinedChat", new
            {
                message = $"A user joined the inquiry chat",
                timestamp = DateTime.UtcNow
            });
        }
        public async Task LeaveInquiryChat(int inquiryId)
        {
            var groupName = $"inquiry-{inquiryId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            await Clients.Group(groupName).SendAsync("UserLeftChat", new
            {
                message = $"A user left the inquiry chat",
                timestamp = DateTime.UtcNow
            });
        }
        public async Task SendInquiryMessage(int inquiryId, string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";

            if (string.IsNullOrWhiteSpace(message))
                return;

            var groupName = $"inquiry-{inquiryId}";
            
            await Clients.Group(groupName).SendAsync("ReceiveInquiryMessage", new
            {
                inquiryId = inquiryId,
                userId = userId,
                userName = userName,
                message = message,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task NotifyInquiryStatusChange(int inquiryId, string newStatus)
        {
            var groupName = $"inquiry-{inquiryId}";
            
            await Clients.Group(groupName).SendAsync("InquiryStatusChanged", new
            {
                inquiryId = inquiryId,
                newStatus = newStatus,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task NotifyNewInquiryMessage(string recipientUserId, int inquiryId, string senderName, string preview)
        {
            await Clients.Group($"user-inquiries-{recipientUserId}").SendAsync("NewInquiryMessageNotification", new
            {
                inquiryId = inquiryId,
                senderName = senderName,
                messagePreview = preview,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task MarkInquiryAsRead(int inquiryId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groupName = $"inquiry-{inquiryId}";
            
            await Clients.Group(groupName).SendAsync("InquiryMarkedAsRead", new
            {
                inquiryId = inquiryId,
                readBy = userId,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task SendTypingIndicator(int inquiryId)
        {
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
            var groupName = $"inquiry-{inquiryId}";
            
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("UserTyping", new
            {
                inquiryId = inquiryId,
                userName = userName,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task ClearTypingIndicator(int inquiryId)
        {
            var groupName = $"inquiry-{inquiryId}";
            
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("UserStoppedTyping", new
            {
                inquiryId = inquiryId,
                timestamp = DateTime.UtcNow
            });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
