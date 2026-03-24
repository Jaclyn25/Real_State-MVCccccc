namespace RealState_Platform.Models
{
    public class PropertyImage : BaseEntity
    {
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }

        // Foreign Keys
        public int PropertyId { get; set; }

        // Navigation Properties
        public virtual Property Property { get; set; }
    }
}
