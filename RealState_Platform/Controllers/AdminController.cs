using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealState_Platform.Interfaces;
using RealState_Platform.Models;
using RealState_Platform.ViewModel;

namespace RealState_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IGenericRepository<Property> _propertyRepo;
        private readonly IGenericRepository<Inquiry> _inquiryRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            IGenericRepository<Property> propertyRepo,
            IGenericRepository<Inquiry> inquiryRepo,
            UserManager<ApplicationUser> userManager)
        {
            _propertyRepo = propertyRepo;
            _inquiryRepo = inquiryRepo;
            _userManager = userManager;
        }

        // ================= DASHBOARD =================
        public async Task<IActionResult> Dashboard()
        {
            var vm = new DashboardVM
            {
                TotalProperties = await _propertyRepo.CountAsync(),
                PendingProperties = await _propertyRepo.CountAsync(p => !p.IsApproved),
                TotalUsers = _userManager.Users.Count(),
                TotalInquiries = await _inquiryRepo.CountAsync()
            };

            return View(vm);
        }

        // ================= APPROVE LISTINGS =================
        public async Task<IActionResult> ApproveListings()
        {
            var properties = await _propertyRepo.FindAsync(p => !p.IsApproved);
            return View(properties);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null) return NotFound();

            property.IsApproved = true;
            _propertyRepo.Update(property);
            await _propertyRepo.SaveChangesAsync();

            return RedirectToAction("ApproveListings");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var property = await _propertyRepo.GetByIdAsync(id);
            if (property == null) return NotFound();

            _propertyRepo.Delete(property);
            await _propertyRepo.SaveChangesAsync();

            return RedirectToAction("ApproveListings");
        }

        // ================= USERS =================
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var model = new List<UserWithRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserWithRoleViewModel
                {
                    UserId = user.Id,
                    Name = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CurrentRole = roles.FirstOrDefault()
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (!currentRoles.Contains(newRole))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
            }

            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("ManageUsers");
        }
        public IActionResult Inquiries()
        {
            return View();
        }
    }

}
