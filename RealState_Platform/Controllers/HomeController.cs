namespace RealState_Platform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public HomeController(ILogger<HomeController> logger, IHubContext<NotificationHub> notificationHubContext)
        {
            _logger = logger;
            _notificationHubContext = notificationHubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Test Notification Endpoints
        [HttpGet]
        public async Task<IActionResult> TestNotification()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not logged in");
            }

            // Send test notification to current user
            await _notificationHubContext.Clients
                .Group($"user-{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    title = "اختبار الإخطار",
                    message = "هذا اختبار لنظام الإخطارات في الوقت الفعلي",
                    type = "success",
                    timestamp = DateTime.UtcNow
                });

            return Ok("✅ تم إرسال الإخطار! تحقق من واجهة المستخدم");
        }

        [HttpGet]
        public async Task<IActionResult> TestBroadcastNotification()
        {
            // Send to all connected users
            await _notificationHubContext.Clients.All
                .SendAsync("ReceiveNotification", new
                {
                    title = "إخطار عام",
                    message = "هذا إخطار لجميع المستخدمين المتصلين",
                    type = "info",
                    timestamp = DateTime.UtcNow
                });

            return Ok("✅ تم إرسال إخطار عام لجميع المستخدمين");
        }

        [HttpGet]
        public async Task<IActionResult> TestAdminNotification()
        {
            // Send to admins only
            await _notificationHubContext.Clients.Group("admins")
                .SendAsync("ReceiveNotification", new
                {
                    title = "إخطار إداري",
                    message = "هذا إخطار موجه للمسؤولين فقط",
                    type = "warning",
                    isAdminOnly = true,
                    timestamp = DateTime.UtcNow
                });

            return Ok("✅ تم إرسال إخطار إداري");
        }

        [HttpGet]
        public async Task<IActionResult> TestPropertyUpdate(string propertyId = "test-property-123")
        {
            // Send property update notification
            await _notificationHubContext.Clients.All
                .SendAsync("ReceiveUpdateNotification", new
                {
                    propertyId = propertyId,
                    updateType = "price_update",
                    message = $"تم تحديث السعر للعقار {propertyId}",
                    timestamp = DateTime.UtcNow
                });

            return Ok("✅ تم إرسال إخطار تحديث العقار");
        }
    }
}
