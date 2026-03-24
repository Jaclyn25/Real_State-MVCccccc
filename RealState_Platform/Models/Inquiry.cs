namespace RealState_Platform.Models
{
    public class Inquiry : BaseEntity
    {
        public string Message { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string Status { get; set; } // New, Read, Replied

        // Foreign Keys
        public int PropertyId { get; set; }
        public string? UserId { get; set; } // Null if not registered

        // Navigation Properties
        public virtual Property Property { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
