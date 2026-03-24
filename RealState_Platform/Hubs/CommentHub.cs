namespace RealState_Platform.Hubs
{
    [Authorize]
    public class CommentHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public async Task JoinPropertyComments(int propertyId)
        {
            var groupName = $"property-comments-{propertyId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            await Clients.Group(groupName).SendAsync("UserJoinedComments", new
            {
                message = "A user joined the comments",
                timestamp = DateTime.UtcNow
            });
        }
        public async Task LeavePropertyComments(int propertyId)
        {
            var groupName = $"property-comments-{propertyId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            await Clients.Group(groupName).SendAsync("UserLeftComments", new
            {
                message = "A user left the comments",
                timestamp = DateTime.UtcNow
            });
        }
        public async Task BroadcastNewComment(int propertyId, int commentId, string userName, string userAvatar, string content, DateTime createdAt)
        {
            var groupName = $"property-comments-{propertyId}";
            
            await Clients.Group(groupName).SendAsync("NewCommentAdded", new
            {
                commentId = commentId,
                propertyId = propertyId,
                userName = userName,
                userAvatar = userAvatar,
                content = content,
                createdAt = createdAt,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task BroadcastCommentDeleted(int propertyId, int commentId)
        {
            var groupName = $"property-comments-{propertyId}";
            
            await Clients.Group(groupName).SendAsync("CommentDeleted", new
            {
                propertyId = propertyId,
                commentId = commentId,
                timestamp = DateTime.UtcNow
            });
        }
        public async Task BroadcastCommentEdited(int propertyId, int commentId, string newContent)
        {
            var groupName = $"property-comments-{propertyId}";
            
            await Clients.Group(groupName).SendAsync("CommentEdited", new
            {
                propertyId = propertyId,
                commentId = commentId,
                newContent = newContent,
                timestamp = DateTime.UtcNow
            });
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
