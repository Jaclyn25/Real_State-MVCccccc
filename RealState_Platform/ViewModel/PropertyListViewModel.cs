namespace RealState_Platform.ViewModel
{
    public class PropertyListViewModel
    {
       public IEnumerable<Property> Properties { get; set; } = new List<Property>();

        public string? Search { get; set; }
        public string? City { get; set; }

        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}
