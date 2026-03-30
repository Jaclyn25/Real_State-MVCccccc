namespace RealState_Platform.Controllers
{
    public class InquiryController : Controller
    {
        private readonly IGenericRepository<Inquiry> _inquiryRepo;
        private readonly IGenericRepository<Property> _propertyRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public InquiryController(
            IGenericRepository<Inquiry> inquiryRepo,
            IGenericRepository<Property> propertyRepo,
            UserManager<ApplicationUser> userManager)
        {
            _inquiryRepo = inquiryRepo;
            _propertyRepo = propertyRepo;
            _userManager = userManager;
        }
        // SendInquiry (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> SendInquiry(int propertyId, string customerName, string customerEmail, string customerPhone, string message)
        {
            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerEmail) || 
                string.IsNullOrWhiteSpace(customerPhone) || string.IsNullOrWhiteSpace(message) || propertyId <= 0)
            {
                return Json(new { success = false, message = "All fields are required" });
            }
            var property = await _propertyRepo.GetByIdAsync(propertyId);
            if (property == null)
            {
                return Json(new { success = false, message = "Property not found" });
            }
            if (!property.IsApproved || !string.Equals(property.Status, "Available", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "This property is not available." });
            }
            ApplicationUser currentUser = null;
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                currentUser = await _userManager.FindByIdAsync(userId);
            }
            var inquiry = new Inquiry
            {
                Message = message,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,
                Status = "New",
                PropertyId = propertyId,
                UserId = currentUser?.Id,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _inquiryRepo.AddAsync(inquiry);
            await _inquiryRepo.SaveChangesAsync();

            return Json(new { success = true, message = "Inquiry sent successfully!" });
        }
        // Inbox (GET) → Show received inquiries for agent's properties
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Inbox(int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            int pageSize = 10;
            var inquiries = await _inquiryRepo.GetAllAsync(i => i.Property);

            var userInquiries = inquiries
                .Where(i => i.Property.AgentId == currentUser.Id && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .ToList();

            int totalItems = userInquiries.Count();

            var pagedInquiries = userInquiries
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InquiryInboxViewModel
                {
                    Id = i.Id,
                    CustomerName = i.CustomerName,
                    CustomerEmail = i.CustomerEmail,
                    CustomerPhone = i.CustomerPhone,
                    Message = i.Message,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt,
                    PropertyTitle = i.Property?.Title ?? "Unknown",
                    PropertyId = i.PropertyId,
                    UserEmail = i.User?.Email
                })
                .ToList();

            var model = new InboxViewModel
            {
                Inquiries = pagedInquiries,
                PageNumber = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalInquiries = totalItems
            };

            return View(model);
        }

        // MarkAsRead (POST) → Update inquiry status
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var inquiry = await _inquiryRepo.GetByIdAsync(id);
            if (inquiry == null)
                return Json(new { success = false, message = "Inquiry not found" });

            var currentUser = await _userManager.GetUserAsync(User);
            var property = await _propertyRepo.GetByIdAsync(inquiry.PropertyId);
            if (property?.AgentId != currentUser?.Id)
                return Json(new { success = false, message = "Unauthorized" });

            inquiry.Status = "Read";
            _inquiryRepo.Update(inquiry);
            await _inquiryRepo.SaveChangesAsync();

            return Json(new { success = true, message = "Inquiry marked as read" });
        }
    }

    // Helper ViewModel for Inbox listing
    public class InboxViewModel
    {
        public List<InquiryInboxViewModel> Inquiries { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalInquiries { get; set; }
    }
}
