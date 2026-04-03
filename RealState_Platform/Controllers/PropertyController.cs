namespace RealState_Platform.Controllers
{
    public class PropertyController : Controller
    {
        private readonly IGenericRepository<Property> _propertyRepo;
        private readonly IGenericRepository<PropertyImage> _imageRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PropertyController(
            IGenericRepository<Property> propertyRepo,
            IGenericRepository<PropertyImage> imageRepo,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _propertyRepo = propertyRepo;
            _imageRepo = imageRepo;
            _userManager = userManager;
            _env = env;
        }
        // Index (GET) → Search + Pagination
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(string search, string city, int page = 1)
        {
            int pageSize = 6;
            var properties = await _propertyRepo.GetAllAsync(p => p.Images);

            // Public catalog: show approved properties (both Available and Sold)
            properties = properties.Where(p => p.IsApproved);

            if (!string.IsNullOrEmpty(search))
                properties = properties.Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(city))
                properties = properties.Where(p => p.City == city);

            // Sort: Available first, then Sold at bottom
            properties = properties
                .OrderBy(p => p.IsSold ? 1 : 0)
                .ThenByDescending(p => p.CreatedAt);

            int totalItems = properties.Count();

            var data = properties
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new PropertyListViewModel
            {
                Properties = data,
                Search = search,
                City = city,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            return View(vm);
        }
        // Details (GET)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id, p => p.Images, p => p.Agent);
            if (property == null)
                return NotFound();

            // Hide unapproved listings from public users
            if (!property.IsApproved && !(User.IsInRole("Admin") || (User.IsInRole("Agent") && property.AgentId == _userManager.GetUserId(User))))
                return NotFound();

            // If sold, only Admin / Agent owner / Buyer can view details
            if (string.Equals(property.Status, "Sold", StringComparison.OrdinalIgnoreCase))
            {
                var currentUserId = _userManager.GetUserId(User);
                var canSeeSold =
                    User.IsInRole("Admin") ||
                    (User.IsInRole("Agent") && property.AgentId == currentUserId) ||
                    (!string.IsNullOrEmpty(property.BuyerId) && property.BuyerId == currentUserId);

                if (!canSeeSold)
                    return NotFound();
            }

            return View(property);
        }

        // Buy (POST) → Customer buys property, it disappears from public catalog
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Buy(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null)
                return NotFound();

            if (!property.IsApproved)
                return Forbid();

            if (!string.Equals(property.Status, "Available", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "This property is no longer available.";
                return RedirectToAction(nameof(Details), new { id });
            }

            property.Status = "Sold";
            property.IsSold = true;
            property.BuyerId = currentUser.Id;
            property.SoldAt = DateTime.UtcNow;

            _propertyRepo.Update(property);
            await _propertyRepo.SaveChangesAsync();

            TempData["Success"] = "Purchase completed. This property is now marked as Sold.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // BuyApi (POST) → API endpoint for buying properties with JSON response
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [Route("api/property/buy/{id}")]
        public async Task<IActionResult> BuyApi(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var property = await _propertyRepo.GetByIdAsync(id);
                if (property == null)
                    return NotFound(new { success = false, message = "Property not found" });

                if (!property.IsApproved)
                    return BadRequest(new { success = false, message = "This property has not been approved yet" });

                // Prevent buying already sold property
                if (property.IsSold || !string.Equals(property.Status, "Available", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { success = false, message = "This property is no longer available for purchase" });

                // Update property
                property.Status = "Sold";
                property.IsSold = true;
                property.BuyerId = currentUser.Id;
                property.SoldAt = DateTime.UtcNow;

                _propertyRepo.Update(property);
                await _propertyRepo.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Purchase completed successfully!",
                    propertyId = property.Id,
                    propertyTitle = property.Title,
                    price = property.Price,
                    soldAt = property.SoldAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        // Create (GET)
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public IActionResult Create()
        {
            return View();
        }
        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> Create(PropertyCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "There are errors in your input. Please fix them.";
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var property = new Property
            {
                Title = vm.Title,
                Description = vm.Description,
                Price = vm.Price,
                City = vm.City,
                State = vm.State,
                ZipCode = vm.ZipCode,
                Address = vm.Address,
                PropertyType = vm.PropertyType,
                ListingType = vm.ListingType,
                Status = "Available",
                Area = vm.Area ?? 0,
                Bedrooms = vm.Bedrooms ?? 0,
                Bathrooms = vm.Bathrooms ?? 0,
                AgentId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _propertyRepo.AddAsync(property);
            await _propertyRepo.SaveChangesAsync();

            // Handle Images
            if (vm.Images != null && vm.Images.Any())
            {
                foreach (var file in vm.Images)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string path = Path.Combine(_env.WebRootPath, "images", fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var image = new PropertyImage
                    {
                        ImageUrl = "/images/" + fileName,
                        PropertyId = property.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _imageRepo.AddAsync(image);
                }
                await _imageRepo.SaveChangesAsync();
            }

            TempData["Success"] = "Property created successfully!";
            return RedirectToAction("MyListings");
        }
        // Edit (GET)
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (property.AgentId != user.Id)
                return Forbid();

            var vm = new PropertyEditViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                City = property.City,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                PropertyType = property.PropertyType,
                ListingType = property.ListingType,
                Address = property.Address,
                State = property.State,
                ZipCode = property.ZipCode,
                Area = property.Area,
                HasGarage = property.HasGarage,
                HasGarden = property.HasGarden,
                HasPool = property.HasPool,
                IsFurnished = property.IsFurnished
            };

            return View(vm);
        }
        // Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> Edit(PropertyEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var property = await _propertyRepo.GetByIdAsync(vm.Id);
            if (property == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (property.AgentId != user.Id)
                return Forbid();

            property.Title = vm.Title;
            property.Description = vm.Description;
            property.Price = vm.Price;
            property.City = vm.City;
            property.Bedrooms = vm.Bedrooms ?? property.Bedrooms;
            property.Bathrooms = vm.Bathrooms ?? property.Bathrooms;
            property.HasGarage = vm.HasGarage;
            property.HasGarden = vm.HasGarden;
            property.HasPool = vm.HasPool;
            property.IsFurnished = vm.IsFurnished;

            await _propertyRepo.SaveChangesAsync();

            if (vm.Images != null && vm.Images.Any())
            {
                foreach (var file in vm.Images)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string path = Path.Combine(_env.WebRootPath, "images", fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var image = new PropertyImage
                    {
                        ImageUrl = "/images/" + fileName,
                        PropertyId = property.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _imageRepo.AddAsync(image);
                }

                await _imageRepo.SaveChangesAsync();
            }

            TempData["Success"] = "Property updated successfully!";
            return RedirectToAction(nameof(MyListings));
        }
        // Delete (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (property.AgentId != user.Id)
                return Forbid();

            _propertyRepo.Delete(property);
            await _propertyRepo.SaveChangesAsync();

            TempData["Success"] = "Property deleted successfully!";
            return RedirectToAction(nameof(MyListings));
        }
        // MyListings (GET)
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> MyListings(string search, string city, int page = 1)
        {
            int pageSize = 6;
            var user = await _userManager.GetUserAsync(User);

            var allMine = await _propertyRepo.GetAllAsync(p => p.Images);
            var properties = allMine.Where(p => p.AgentId == user.Id);

            if (!string.IsNullOrEmpty(search))
                properties = properties.Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(city))
                properties = properties.Where(p => p.City == city);

            int totalItems = properties.Count();

            var data = properties
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new PropertyListViewModel
            {
                Properties = data,
                Search = search,
                City = city,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            return View(vm);
        }

        // MyPurchases (GET) → Show customer's bought properties
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyPurchases(int page = 1)
        {
            int pageSize = 6;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var all = await _propertyRepo.GetAllAsync(p => p.Images);
            var mine = all
                .Where(p => p.BuyerId == user.Id)
                .OrderByDescending(p => p.SoldAt ?? p.UpdatedAt ?? p.CreatedAt)
                .ToList();

            int totalItems = mine.Count;
            var data = mine
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new PropertyListViewModel
            {
                Properties = data,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            return View(vm);
        }
    }
}