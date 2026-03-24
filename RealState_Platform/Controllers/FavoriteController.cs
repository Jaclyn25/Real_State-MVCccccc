namespace RealState_Platform.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly IGenericRepository<Favorite> _favoriteRepo;
        private readonly IGenericRepository<Property> _propertyRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteController(
            IGenericRepository<Favorite> favoriteRepo,
            IGenericRepository<Property> propertyRepo,
            UserManager<ApplicationUser> userManager)
        {
            _favoriteRepo = favoriteRepo;
            _propertyRepo = propertyRepo;
            _userManager = userManager;
        }
        // AddToFavorite (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavorite(int propertyId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Json(new { success = false, message = "Please login first" });
            var property = await _propertyRepo.GetByIdAsync(propertyId);
            if (property == null)
                return Json(new { success = false, message = "Property not found" });
            var favorites = await _favoriteRepo.GetAllAsync();
            var existing = favorites.FirstOrDefault(f => 
                f.UserId == currentUser.Id && f.PropertyId == propertyId && !f.IsDeleted);

            if (existing != null)
                return Json(new { success = false, message = "Already in favorites" });
            var favorite = new Favorite
            {
                UserId = currentUser.Id,
                PropertyId = propertyId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _favoriteRepo.AddAsync(favorite);
            await _favoriteRepo.SaveChangesAsync();

            return Json(new { success = true, message = "Added to favorites!" });
        }

        // RemoveFromFavorite (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorite(int favoriteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Json(new { success = false, message = "Unauthorized" });

            var favorite = await _favoriteRepo.GetByIdAsync(favoriteId);
            if (favorite == null || favorite.UserId != currentUser.Id)
                return Json(new { success = false, message = "Favorite not found" });
            favorite.IsDeleted = true;
            _favoriteRepo.Update(favorite);
            await _favoriteRepo.SaveChangesAsync();

            return Json(new { success = true, message = "Removed from favorites" });
        }

        // MyFavorites (GET) → Show user's favorite properties
        [HttpGet]
        public async Task<IActionResult> MyFavorites(int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            int pageSize = 6;
            var allFavorites = await _favoriteRepo.GetAllAsync(f => f.Property, f => f.Property.Images);

            var userFavorites = allFavorites
                .Where(f => f.UserId == currentUser.Id && !f.IsDeleted)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            int totalItems = userFavorites.Count();

            var pagedFavorites = userFavorites
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FavoriteViewModel
                {
                    FavoriteId = f.Id,
                    PropertyId = f.PropertyId,
                    Title = f.Property?.Title ?? "Unknown",
                    Price = f.Property?.Price ?? 0,
                    City = f.Property?.City ?? "",
                    Address = f.Property?.Address ?? "",
                    Area = f.Property?.Area,
                    Bedrooms = f.Property?.Bedrooms,
                    Bathrooms = f.Property?.Bathrooms,
                    PropertyType = f.Property?.PropertyType ?? "",
                    ListingType = f.Property?.ListingType ?? "",
                    ImageUrl = f.Property?.Images?.FirstOrDefault()?.ImageUrl ?? "/images/placeholder.png",
                    AddedAt = f.CreatedAt
                })
                .ToList();

            var model = new MyFavoritesViewModel
            {
                Favorites = pagedFavorites,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalFavorites = totalItems
            };

            return View(model);
        }
    }
}
