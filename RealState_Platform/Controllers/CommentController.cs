namespace RealState_Platform.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly IGenericRepository<Comment> _commentRepository;
        private readonly IGenericRepository<Property> _propertyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<CommentHub> _commentHubContext;

        public CommentController(
            IGenericRepository<Comment> commentRepository,
            IGenericRepository<Property> propertyRepository,
            UserManager<ApplicationUser> userManager,
            IHubContext<CommentHub> commentHubContext)
        {
            _commentRepository = commentRepository;
            _propertyRepository = propertyRepository;
            _userManager = userManager;
            _commentHubContext = commentHubContext;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<JsonResult> GetComments(int propertyId, int page = 1, int pageSize = 10)
        {
            try
            {
                var allComments = await _commentRepository.GetAllAsync();
                var comments = allComments.ToList();
                
                var propertyComments = comments
                    .Where(c => c.PropertyId == propertyId && c.ParentCommentId == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var nextPageExists = comments
                    .Where(c => c.PropertyId == propertyId && c.ParentCommentId == null)
                    .Skip(page * pageSize)
                    .Any();

                return Json(new
                {
                    comments = propertyComments.Select(c => new
                    {
                        id = c.Id,
                        content = c.Content,
                        userName = c.IsAnonymous ? "Anonymous" : (c.User?.UserName ?? "Anonymous"),
                        userEmail = c.IsAnonymous ? "" : (c.User?.Email ?? ""),
                        createdAt = c.CreatedAt,
                        updatedAt = c.UpdatedAt,
                        isAnonymous = c.IsAnonymous,
                        isOwner = c.UserId == User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        replies = GetReplies(c.Id, comments)
                    }),
                    currentPage = page,
                    hasNextPage = nextPageExists
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = ex.Message });
            }
        }

        private object[] GetReplies(int parentCommentId, List<Comment> allComments)
        {
            return allComments
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    id = c.Id,
                    content = c.Content,
                    userName = c.IsAnonymous ? "Anonymous" : (c.User?.UserName ?? "Anonymous"),
                    userEmail = c.IsAnonymous ? "" : (c.User?.Email ?? ""),
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt,
                    isAnonymous = c.IsAnonymous,
                    isOwner = c.UserId == User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                })
                .ToArray();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddComment(int propertyId, string content, bool isAnonymous = false, int? parentCommentId = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AddComment called with propertyId={propertyId}, content length={content?.Length ?? 0}, isAnonymous={isAnonymous}");
                if (propertyId <= 0)
                    return Json(new { success = false, message = "Invalid property ID" });

                if (string.IsNullOrWhiteSpace(content))
                    return Json(new { success = false, message = "Comment cannot be empty" });

                if (content.Length > 1000)
                    return Json(new { success = false, message = "Comment is too long (max 1000 characters)" });

                var property = await _propertyRepository.GetByIdAsync(propertyId);
                if (property == null)
                    return Json(new { success = false, message = "Property not found" });
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "User not authenticated" });

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });
                var comment = new Comment
                {
                    PropertyId = propertyId,
                    UserId = userId,
                    Content = content.Trim(),
                    ParentCommentId = parentCommentId,
                    IsAnonymous = isAnonymous,
                    CreatedAt = DateTime.UtcNow
                };

                await _commentRepository.AddAsync(comment);
                await _commentRepository.SaveChangesAsync();
                var createdComment = await _commentRepository.GetByIdAsync(comment.Id);
                
                // Determine display name
                var displayName = isAnonymous ? "Anonymous" : user.UserName;
                var displayEmail = isAnonymous ? "" : user.Email;

                try
                {
                    await _commentHubContext.Clients
                        .Group($"property-comments-{propertyId}")
                        .SendAsync("NewCommentAdded", new
                        {
                            commentId = createdComment.Id,
                            propertyId = propertyId,
                            userName = displayName,
                            userEmail = displayEmail,
                            content = content,
                            createdAt = createdComment.CreatedAt,
                            isAnonymous = isAnonymous,
                            isReply = parentCommentId != null
                        });
                }
                catch (Exception signalrError)
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR error: {signalrError.Message}");
                }

                return Json(new
                {
                    success = true,
                    message = "Comment added successfully",
                    comment = new
                    {
                        id = createdComment.Id,
                        content = content,
                        userName = displayName,
                        userEmail = displayEmail,
                        createdAt = createdComment.CreatedAt,
                        isAnonymous = isAnonymous,
                        isOwner = true
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddComment error: {ex.Message}\n{ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null)
                    return Json(new { success = false, message = "Comment not found" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (comment.UserId != userId && !User.IsInRole("Admin"))
                    return Json(new { success = false, message = "You can only delete your own comments" });
                comment.IsDeleted = true;
                comment.UpdatedAt = DateTime.UtcNow;
                _commentRepository.Update(comment);
                await _commentRepository.SaveChangesAsync();
                await _commentHubContext.Clients
                    .Group($"property-comments-{comment.PropertyId}")
                    .SendAsync("CommentDeleted", new
                    {
                        propertyId = comment.PropertyId,
                        commentId = commentId
                    });

                return Json(new { success = true, message = "Comment deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, string newContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newContent))
                    return Json(new { success = false, message = "Comment cannot be empty" });

                if (newContent.Length > 1000)
                    return Json(new { success = false, message = "Comment is too long (max 1000 characters)" });

                var comment = await _commentRepository.GetByIdAsync(commentId);
                if (comment == null)
                    return Json(new { success = false, message = "Comment not found" });

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (comment.UserId != userId && !User.IsInRole("Admin"))
                    return Json(new { success = false, message = "You can only edit your own comments" });

                comment.Content = newContent;
                comment.UpdatedAt = DateTime.UtcNow;
                _commentRepository.Update(comment);
                await _commentRepository.SaveChangesAsync();

                await _commentHubContext.Clients
                    .Group($"property-comments-{comment.PropertyId}")
                    .SendAsync("CommentEdited", new
                    {
                        propertyId = comment.PropertyId,
                        commentId = commentId,
                        newContent = newContent
                    });

                return Json(new { success = true, message = "Comment edited successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
