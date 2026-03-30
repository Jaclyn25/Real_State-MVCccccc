/**
 * QUICK REFERENCE - Copy/Paste Code Snippets
 * Use these snippets to quickly integrate the buy feature
 */

// ============================================
// 1. ADD TO YOUR _Layout.cshtml
// ============================================
// Add this to your <head> or before closing </body>

<script src="~/js/shopping-cart.js"></script>

<!-- Cart Button in Navbar -->
<div class="navbar-nav ms-auto">
    <div class="position-relative" id="cartContainer" style="margin-right: 15px;">
        <button class="btn btn-outline-primary" id="cartBtn">
            🛒 Cart
            <span id="cartBadge" class="badge bg-danger" style="display: none;">0</span>
        </button>
    </div>
</div>

<!-- Cart Modal (add after main content) -->
<div class="modal fade" id="cartModal" tabindex="-1" aria-labelledby="cartModalLabel">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="cartModalLabel">Shopping Cart</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div id="cartItems"></div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                <button type="button" class="btn btn-danger" onclick="propertyCart.clearCart(); updateCartUI();">
                    Clear Cart
                </button>
                <button type="button" class="btn btn-success" onclick="buyAllInCart(this)">
                    Confirm Purchase
                </button>
            </div>
        </div>
    </div>
</div>

<!-- JavaScript to open cart -->
<script>
    document.addEventListener('DOMContentLoaded', function() {
        const cartBtn = document.getElementById('cartBtn');
        if (cartBtn) {
            cartBtn.addEventListener('click', function() {
                const modal = new bootstrap.Modal(document.getElementById('cartModal'));
                updateCartUI();
                modal.show();
            });
        }
    });
</script>


// ============================================
// 2. ADD TO YOUR Property/Details.cshtml
// ============================================

@{
    // Check if property is sold
    bool isSold = Model.IsSold || string.Equals(Model.Status, "Sold", StringComparison.OrdinalIgnoreCase);
}

<!-- Price Display -->
<div class="mb-4">
    <h2 class="text-primary">${@Model.Price.ToString("N0")}</h2>
    @if (isSold)
    {
        <span class="badge bg-danger">SOLD</span>
    }
    else
    {
        <span class="badge bg-success">AVAILABLE</span>
    }
</div>

<!-- Buy Buttons - Add this in your action buttons section -->
@if (!isSold && User.IsInRole("Customer"))
{
    <div class="d-grid gap-2 mb-3">
        <button type="button" class="btn btn-primary btn-lg" 
                onclick="addToCart(@Model.Id, '@Model.Title', @Model.Price)">
            🛒 Add to Cart
        </button>
        
        <button type="button" class="btn btn-success btn-lg" id="buyNowBtn"
                onclick="buyProperty(@Model.Id, this)">
            ✓ Buy Now
        </button>
    </div>
}
else if (!User.IsInRole("Customer") && !User.Identity.IsAuthenticated)
{
    <div class="d-grid">
        <a href="@Url.Action("Login", "Account")" class="btn btn-warning btn-lg">
            Login to Purchase
        </a>
    </div>
}
else if (isSold)
{
    <div class="d-grid">
        <button type="button" class="btn btn-secondary btn-lg" disabled>
            Sold
        </button>
    </div>
}


// ============================================
// 3. CONTROLLER - Already Updated PropertyController
// ============================================

// The BuyApi endpoint is already added to PropertyController.cs:

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


// ============================================
// 4. SIMPLE IMPLEMENTATION EXAMPLE
// ============================================

<!-- Minimal HTML for testing -->
<div class="card">
    <div class="card-body">
        <h5 class="card-title">Buy Property</h5>
        <p class="card-text">Price: $<span id="price">500000</span></p>
        
        <button class="btn btn-primary" onclick="addToCart(1, 'Test Property', 500000)">
            Add to Cart
        </button>
        
        <button class="btn btn-success" id="buyBtn" onclick="buyProperty(1, this)">
            Buy Now
        </button>
    </div>
</div>

<!-- Expected Results after clicking "Buy Now":
1. Button shows "Processing..."
2. API call is made to /api/property/buy/1
3. Success message appears
4. Button changes to "Sold" and is disabled
5. Page reloads after 2 seconds
-->


// ============================================
// 5. TESTING WITH BROWSER CONSOLE
// ============================================

// Open Browser Console (F12) and run these commands:

// Check if script is loaded
console.log(typeof propertyCart); // Should show "object"

// Add item to cart
propertyCart.addProperty(1, "Test Property", 500000);

// View cart
console.log(propertyCart.getItems());

// Get cart count
console.log(`Items in cart: ${propertyCart.getCount()}`);

// Get total price
console.log(`Total: $${propertyCart.getTotalPrice()}`);

// Clear cart
propertyCart.clearCart();

// Manual API call test
fetch('/api/property/buy/1', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest'
    }
})
.then(r => r.json())
.then(data => console.log(data));


// ============================================
// 6. MIGRATION STEPS
// ============================================

// In Package Manager Console or PowerShell:

cd RealState_Platform

// Add migration (already created, but if needed:
// dotnet ef migrations add AddIsSoldToProperty -p RealState_Platform -s RealState_Platform

// Apply migration
dotnet ef database update

// Verify it was applied by checking Property table schema


// ============================================
// 7. DATABASE VERIFICATION
// ============================================

-- Run this SQL to verify the IsSold column exists:

USE RealState_Platform  -- Your database name

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Properties' 
AND COLUMN_NAME = 'IsSold'

-- Expected result: IsSold | bit | NO


// ============================================
// 8. DISABLE BUY FOR SOLD PROPERTIES
// ============================================

// Use this conditional in your View:

@if (Model.IsSold)
{
    <span class="badge bg-danger">This property has been sold</span>
}
else if (User.IsInRole("Customer"))
{
    <button onclick="buyProperty(@Model.Id, this)">Buy Now</button>
}


// ============================================
// 9. AUTHORIZATION IN PROGRAM.CS
// ============================================

// Ensure you have role-based authorization in Program.cs:

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Customer", policy => 
        policy.RequireRole("Customer"));
});

// And in ConfigureServices:
builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


// ============================================
// 10. FILTER SOLD PROPERTIES FROM LISTING
// ============================================

// In PropertyController Index action:

// Only show available properties in public catalog
properties = properties.Where(p =>
    p.IsApproved &&
    !p.IsSold && // Add this line
    string.Equals(p.Status, "Available", StringComparison.OrdinalIgnoreCase));
