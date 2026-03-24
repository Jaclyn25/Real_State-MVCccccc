namespace RealState_Platform.ViewModel
{
    public class FavoriteViewModel
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public double? Area { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public string PropertyType { get; set; }
        public string ListingType { get; set; }
        public string ImageUrl { get; set; }
        public int FavoriteId { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
