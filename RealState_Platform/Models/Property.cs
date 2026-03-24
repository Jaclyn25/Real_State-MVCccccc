namespace RealState_Platform.Models
{
    public class Property : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PropertyType { get; set; } // Apartment, Villa, Commercial
        public string ListingType { get; set; } // Sale, Rent
        public decimal Price { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public double Area { get; set; } // Square Feet
        public string Status { get; set; } // Available, Pending, Sold, Rented

        // Features
        public bool HasGarage { get; set; }
        public bool HasGarden { get; set; }
        public bool HasPool { get; set; }
        public bool IsFurnished { get; set; }

        // Tracking
        public int ViewCount { get; set; }

        // Foreign Keys
        public string AgentId { get; set; } // Identity User Id is string

        // Navigation Properties
        public virtual ApplicationUser Agent { get; set; }
        public virtual ICollection<PropertyImage> Images { get; set; }
        public virtual ICollection<Inquiry> Inquiries { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
    }
}
