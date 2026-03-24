namespace RealState_Platform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;


        public virtual ICollection<Property> Properties { get; set; } // For Agent
        public virtual ICollection<Inquiry> Inquiries { get; set; } // For Customer
        public virtual ICollection<Favorite> Favorites { get; set; } // For Customer


        public string FullName => $"{FirstName} {LastName}";
    }
}
