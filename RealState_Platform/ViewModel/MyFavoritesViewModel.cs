namespace RealState_Platform.ViewModel
{
    public class MyFavoritesViewModel
    {
        public List<FavoriteViewModel> Favorites { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalFavorites { get; set; }
    }
}