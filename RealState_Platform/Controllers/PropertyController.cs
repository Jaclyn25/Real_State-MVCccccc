using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealState_Platform.Interfaces;
using RealState_Platform.Models;
using Microsoft.EntityFrameworkCore;
using RealState_Platform.Data;
using RealState_Platform.ViewModel;

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

        // =========================================
        // Index (GET) → Search + Pagination
        // =========================================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(string search, string city, int page = 1)
        {
            int pageSize = 6;
            var properties = await _propertyRepo.GetAllAsync(p => p.Images);

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

        // =========================================
        // Details (GET)
        // =========================================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id, p => p.Images, p => p.Agent);
            if (property == null)
                return NotFound();

            return View(property);
        }

        // =========================================
        // Create (GET)
        // =========================================
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================
        // Create (POST)
        // =========================================
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

        // =========================================
        // Edit (GET)
        // =========================================
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

        // =========================================
        // Edit (POST)
        // =========================================
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

        // =========================================
        // Delete (POST)
        // =========================================
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

        // =========================================
        // MyListings (GET)
        // =========================================
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> MyListings(string search, string city, int page = 1)
        {
            int pageSize = 6;
            var user = await _userManager.GetUserAsync(User);

            var properties = await _propertyRepo.FindAsync(p => p.AgentId == user.Id);

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
    }
}