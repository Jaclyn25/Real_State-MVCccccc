namespace RealState_Platform.Models
{
    public class Comment : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Content { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment>? Replies { get; set; }
        public bool IsAnonymous { get; set; } = false;
    }
}
