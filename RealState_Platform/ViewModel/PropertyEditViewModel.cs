using System.ComponentModel.DataAnnotations;

namespace RealState_Platform.ViewModel
{
    public class PropertyEditViewModel
    {
        public int Id { get; set; }

        // Basic Info
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        // Read-only / Required (hidden)
        public string PropertyType { get; set; }
        public string ListingType { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public double Area { get; set; }

        // Features
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public bool HasGarage { get; set; }
        public bool HasGarden { get; set; }
        public bool HasPool { get; set; }
        public bool IsFurnished { get; set; }

        // Images (optional)
        public IFormFileCollection Images { get; set; }
    }
}
