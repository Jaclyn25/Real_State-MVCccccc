# Real Estate Buy Feature - Implementation Guide

## Overview
This guide helps you implement the "Buy" feature for properties in your Real Estate System with:
- Backend API endpoint for purchasing
- Frontend shopping cart system
- JavaScript/Fetch API integration

---

## Backend Implementation (✓ Completed)

### 1. Database Model Changes

**File:** `Models/Property.cs`

The following changes have been made:
- ✓ Added `public bool IsSold { get; set; } = false;` 

### 2. API Endpoint

**File:** `Controllers/PropertyController.cs`

A new API endpoint has been added:

```csharp
[HttpPost]
[Authorize(Roles = "Customer")]
[Route("api/property/buy/{id}")]
public async Task<IActionResult> BuyApi(int id)
```

**Endpoint Details:**
- **Route:** `POST /api/property/buy/{id}`
- **Authentication:** Required (Customer role)
- **Response Format:** JSON

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Purchase completed successfully!",
  "propertyId": 1,
  "propertyTitle": "Luxury Villa",
  "price": 500000,
  "soldAt": "2026-03-30T12:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request` - Property already sold or no longer available
- `401 Unauthorized` - User not authenticated
- `404 Not Found` - Property not found
- `500 Internal Server Error` - Server error

### 3. Logic Implemented

The API endpoint includes:
1. ✓ User authentication check
2. ✓ Property existence validation
3. ✓ Property approval check
4. ✓ Prevention of buying already sold property (checks both `IsSold` and `Status`)
5. ✓ Updates `Status`, `IsSold`, `BuyerId`, and `SoldAt` fields
6. ✓ Error handling with meaningful messages

---

## Database Migration

### Required Steps:

1. **Stop the development server** (stop IIS Express)

2. **Apply the migration:**
   ```powershell
   cd RealState_Platform
   dotnet ef database update
   ```

3. **Verify the migration was applied:**
   - Check your SQL Server database
   - Verify the `IsSold` column exists in the `Properties` table

**Migration File Location:** 
`Migrations/20260330_AddIsSoldToProperty.cs`

---

## Frontend Implementation

### 1. Include the Shopping Cart Script

Add this to your `Views/Shared/_Layout.cshtml`:

```html
<!-- Include before closing body tag -->
<script src="~/js/shopping-cart.js"></script>
```

### 2. Update Property Details View

**File:** `Views/Property/Details.cshtml`

Modify your property details view to include:

```html
<!-- Add to Cart Button -->
<button type="button" class="btn btn-primary btn-lg" 
        onclick="addToCart(@Model.Id, '@Model.Title', @Model.Price)">
    Add to Cart
</button>

<!-- Buy Now Button -->
<button type="button" class="btn btn-success btn-lg" id="buyNowBtn"
        onclick="buyProperty(@Model.Id, this)">
    Buy Now
</button>

<!-- Sold Status Display -->
@if (Model.IsSold)
{
    <button type="button" class="btn btn-secondary btn-lg disabled">
        Sold
    </button>
}
```

### 3. Shopping Cart Display

Display the shopping cart in your navigation or sidebar:

```html
<!-- In Your Navbar -->
<div class="position-relative" id="cartContainer">
    <button class="btn btn-outline-primary" id="cartBtn">
        🛒 Cart
        <span id="cartBadge" class="badge bg-danger">0</span>
    </button>
</div>

<!-- Cart Modal -->
<div class="modal fade" id="cartModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Shopping Cart</h5>
            </div>
            <div class="modal-body">
                <div id="cartItems"></div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-success" 
                        onclick="buyAllInCart(this)">
                    Confirm Purchase
                </button>
            </div>
        </div>
    </div>
</div>
```

---

## JavaScript API Reference

### Core Functions:

#### 1. **addToCart(propertyId, title, price)**
Adds a property to the shopping cart.

```javascript
addToCart(5, "Modern Apartment", 350000);
```

#### 2. **buyProperty(propertyId, buyButton)**
Purchases a single property via API.

```javascript
buyProperty(5, document.getElementById('buyNowBtn'));
```

#### 3. **removeFromCart(propertyId)**
Removes a property from the cart.

```javascript
removeFromCart(5);
```

#### 4. **buyAllInCart(confirmButton)**
Purchases all properties in the cart.

```javascript
buyAllInCart(document.querySelector('.confirm-btn'));
```

#### 5. **displayCart()**
Displays all items currently in the cart.

```javascript
displayCart();
```

### Shopping Cart Manager (Class):

```javascript
// Access the global cart instance
const cart = propertyCart;

// Get all items
cart.getItems();

// Get item count
cart.getCount();

// Get total price
cart.getTotalPrice();

// Clear entire cart
cart.clearCart();

// Check if property is in cart
cart.isInCart(propertyId);
```

---

## Complete HTML Example

For a complete working example, see:
`EXAMPLE_PROPERTY_DETAILS_INTEGRATION.html`

This file shows:
- Property details display
- Buy Now & Add to Cart buttons
- Shopping cart modal
- Error handling
- Success notifications

---

## Testing Checklist

### Backend API Testing (Using Postman or curl):

1. **Test Successful Purchase:**
   ```bash
   curl -X POST "https://localhost:7001/api/property/buy/1" \
     -H "Authorization: Bearer [YOUR_TOKEN]" \
     -H "Content-Type: application/json"
   ```

2. **Test Already Sold Property:**
   - Try to buy the same property again
   - Should return: `"This property is no longer available for purchase"`

3. **Test Unapproved Property:**
   - Create property and don't approve it
   - Try to buy it
   - Should return: `"This property has not been approved yet"`

### Frontend Testing:

1. ✓ Add property to cart from Details page
2. ✓ View cart contents
3. ✓ Remove item from cart
4. ✓ Click "Buy Now" button
5. ✓ See "Purchase Successful" message
6. ✓ Button changes to "Sold" and is disabled
7. ✓ Verify property is removed from cart
8. ✓ Verify purchased property shows `IsSold = true`

---

## Security Considerations

1. **Authentication:** Only authenticated customers can buy
2. **Authorization:** API checks user roles
3. **Status Validation:** Prevents buying already sold properties
4. **Approval Check:** Only approved properties can be purchased
5. **CSRF Protection:** Include CSRF token if using traditional forms

---

## Customization Options

### Modify Success Message:
```javascript
// In shopping-cart.js, find the buyProperty function
showNotification(data.message, 'success');
```

### Change Cart Storage Key:
```javascript
const cart = new PropertyCart('myCustomCartKey');
```

### Customize Notification Style:
```javascript
// Modify the showNotification function for your UI framework
// Currently supports Bootstrap alerts
```

### Add Logging:
```javascript
// Add to shopping-cart.js
console.log('Purchasing property:', propertyId);
console.log('Cart contents:', propertyCart.getItems());
```

---

## Troubleshooting

### Issue: API returns 401 Unauthorized
**Solution:** Ensure user is logged in with "Customer" role

### Issue: Button doesn't respond to click
**Solution:** 
- Verify shopping-cart.js is loaded
- Check browser console for JavaScript errors
- Ensure `propertyId` is passed correctly

### Issue: Purchase fails with "Property not found"
**Solution:** Verify the property ID exists in the database

### Issue: IsSold not updating in database
**Solution:**
- Apply the migration: `dotnet ef database update`
- Restart the application

### Issue: CORS error when calling API
**Solution:**
- Ensure CORS is configured in Program.cs if calling from different domain
- This shouldn't occur if frontend and backend are on same domain

---

## Next Steps

1. **Stop your development server** (IIS Express)
2. **Apply database migration:** `dotnet ef database update`
3. **Update your Property Details view** with the new buttons
4. **Include the shopping-cart.js script** in your layout
5. **Test the functionality**
6. **Deploy to production** when ready

## Support

If you encounter any issues:
1. Check the browser console for errors (F12)
2. Check the server logs
3. Verify all files are in the correct locations
4. Ensure the migration has been applied to the database
