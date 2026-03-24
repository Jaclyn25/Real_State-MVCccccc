using System.ComponentModel.DataAnnotations;

namespace RealState_Platform.ViewModel
{
    public class PropertyCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Area must be positive")]
        public double? Area { get; set; } // nullable

        [Range(0, int.MaxValue, ErrorMessage = "Bedrooms must be positive")]
        public int? Bedrooms { get; set; } // nullable

        [Range(0, int.MaxValue, ErrorMessage = "Bathrooms must be positive")]
        public int? Bathrooms { get; set; } // nullable
        [Required]
        public string ListingType { get; set; }

        [Required]
        public string PropertyType { get; set; }
        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }
        public string ZipCode { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}
