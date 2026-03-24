using RealState_Platform.Models;
using System.Collections.Generic;
namespace RealState_Platform.ViewModel
{
    public class PropertyIndexViewModel
    {
        public string Search { get; set; }
        public string City { get; set; }

        public List<Property> Properties { get; set; } = new List<Property>();

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;
    }
}
