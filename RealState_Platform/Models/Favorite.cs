namespace RealState_Platform.Models
{
    public class Favorite : BaseEntity
    {
        public string UserId { get; set; }
        public int PropertyId { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual Property Property { get; set; }
    }
}
