/**
 * Real Estate Shopping Cart & Buy System (Vanilla JS)
 * Handles shopping cart management and property purchase API calls
 */

// Shopping Cart Manager
class PropertyCart {
    constructor(storageKey = 'propertyCart') {
        this.storageKey = storageKey;
        this.items = this.loadCart();
    }

    // Load cart from localStorage
    loadCart() {
        const cart = localStorage.getItem(this.storageKey);
        return cart ? JSON.parse(cart) : [];
    }

    // Save cart to localStorage
    saveCart() {
        localStorage.setItem(this.storageKey, JSON.stringify(this.items));
    }

    // Add property to cart
    addProperty(propertyId, title, price) {
        // Check if already in cart
        const existing = this.items.find(item => item.propertyId === propertyId);
        
        if (existing) {
            console.warn(`Property ${propertyId} is already in cart`);
            return false;
        }

        this.items.push({
            propertyId,
            title,
            price,
            addedAt: new Date().toISOString()
        });

        this.saveCart();
        return true;
    }

    // Remove property from cart
    removeProperty(propertyId) {
        this.items = this.items.filter(item => item.propertyId !== propertyId);
        this.saveCart();
    }

    // Get all cart items
    getItems() {
        return this.items;
    }

    // Get total items in cart
    getCount() {
        return this.items.length;
    }

    // Get total price
    getTotalPrice() {
        return this.items.reduce((sum, item) => sum + item.price, 0);
    }

    // Clear entire cart
    clearCart() {
        this.items = [];
        this.saveCart();
    }

    // Check if property is in cart
    isInCart(propertyId) {
        return this.items.some(item => item.propertyId === propertyId);
    }
}

// Initialize cart globally
const propertyCart = new PropertyCart();

/**
 * Add a property to the shopping cart
 * Call this when user clicks "Add to Cart" button
 */
function addToCart(propertyId, title, price) {
    if (propertyCart.addProperty(propertyId, title, price)) {
        showNotification(`${title} added to cart!`, 'success');
        updateCartUI();
        return true;
    } else {
        showNotification('Property already in cart', 'warning');
        return false;
    }
}

/**
 * Remove a property from the shopping cart
 */
function removeFromCart(propertyId) {
    propertyCart.removeProperty(propertyId);
    updateCartUI();
    showNotification('Property removed from cart', 'info');
}

/**
 * Display current cart items
 */
function displayCart() {
    const items = propertyCart.getItems();
    const cart = document.getElementById('cartItems');
    
    if (!cart) return;

    if (items.length === 0) {
        cart.innerHTML = '<p class="text-center text-muted">Your cart is empty</p>';
        return;
    }

    let html = `
        <table class="table table-striped">
            <thead>
                <tr>
                    <th>Property</th>
                    <th>Price</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody>
    `;

    items.forEach(item => {
        html += `
            <tr>
                <td>${item.title}</td>
                <td>$${item.price.toLocaleString()}</td>
                <td>
                    <button class="btn btn-sm btn-danger" onclick="removeFromCart(${item.propertyId})">
                        Remove
                    </button>
                </td>
            </tr>
        `;
    });

    html += `
            </tbody>
        </table>
        <div class="alert alert-info">
            <strong>Total:</strong> $${propertyCart.getTotalPrice().toLocaleString()}
        </div>
    `;

    cart.innerHTML = html;
}

/**
 * Update cart UI elements (badge count, etc.)
 */
function updateCartUI() {
    const count = propertyCart.getCount();
    const cartBadge = document.getElementById('cartBadge');
    
    if (cartBadge) {
        cartBadge.textContent = count;
        cartBadge.style.display = count > 0 ? 'block' : 'none';
    }

    const cartCount = document.getElementById('cartCount');
    if (cartCount) {
        cartCount.textContent = count;
    }

    displayCart();
}

/**
 * Buy property via API
 * This function triggers the purchase API call
 */
async function buyProperty(propertyId, buyButton = null, propertyTitle = 'Property') {
    if (!propertyId) {
        showNotification('Property ID is required', 'error');
        return false;
    }

    // Show confirmation dialog
    const confirmed = await showConfirmationDialog(
        'Confirm Purchase',
        `Are you sure you want to purchase "${propertyTitle}"? This listing will be marked as Sold and hidden from other customers.`,
        'Yes, Buy Now',
        'Cancel'
    );

    if (!confirmed) {
        return false;
    }

    // Disable button during processing
    if (buyButton) {
        buyButton.disabled = true;
        buyButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
    }

    try {
        const response = await fetch(`/api/property/buy/${propertyId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || 'Purchase failed');
        }

        // Success
        showNotification(data.message, 'success');

        // Update button to "Sold"
        if (buyButton) {
            buyButton.textContent = 'Sold';
            buyButton.classList.remove('btn-primary');
            buyButton.classList.add('btn-secondary');
            buyButton.disabled = true;
        }

        // Remove from cart if present
        if (propertyCart.isInCart(propertyId)) {
            propertyCart.removeProperty(propertyId);
            updateCartUI();
        }

        // Optional: Refresh page after 2 seconds
        setTimeout(() => {
            location.reload();
        }, 2000);

        return true;

    } catch (error) {
        console.error('Purchase error:', error);
        showNotification(error.message || 'An error occurred during purchase', 'error');

        // Restore button
        if (buyButton) {
            buyButton.disabled = false;
            buyButton.innerHTML = 'Buy Now';
        }

        return false;
    }
}

/**
 * Buy all properties in cart (batch purchase)
 */
async function buyAllInCart(confirmButton = null) {
    const items = propertyCart.getItems();

    if (items.length === 0) {
        showNotification('Your cart is empty', 'warning');
        return false;
    }

    // Show confirmation
    if (!confirm(`Are you sure you want to purchase ${items.length} property/properties?`)) {
        return false;
    }

    if (confirmButton) {
        confirmButton.disabled = true;
        confirmButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';
    }

    let successCount = 0;
    let failCount = 0;

    // Process each property
    for (const item of items) {
        try {
            const response = await fetch(`/api/property/buy/${item.propertyId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                successCount++;
                propertyCart.removeProperty(item.propertyId);
            } else {
                failCount++;
            }
        } catch (error) {
            failCount++;
        }
    }

    // Show result
    if (successCount > 0) {
        showNotification(`Successfully purchased ${successCount} property/properties!`, 'success');
        updateCartUI();
    }

    if (failCount > 0) {
        showNotification(`Failed to purchase ${failCount} property/properties`, 'warning');
    }

    if (confirmButton) {
        confirmButton.disabled = false;
        confirmButton.innerHTML = 'Confirm Purchase';
    }

    return successCount > 0;
}

/**
 * Show confirmation dialog (returns Promise)
 * Uses Bootstrap modal for professional appearance
 */
function showConfirmationDialog(title = 'Confirm', message = '', confirmText = 'OK', cancelText = 'Cancel') {
    return new Promise((resolve) => {
        // Create modal HTML
        const modalId = 'confirmModal_' + Date.now();
        const modalHTML = `
            <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header border-0">
                            <h5 class="modal-title">${title}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <p class="mb-0">${message}</p>
                        </div>
                        <div class="modal-footer border-0">
                            <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">${cancelText}</button>
                            <button type="button" class="btn btn-primary" id="confirmBtn_${modalId}">${confirmText}</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Add modal to DOM
        const modalContainer = document.createElement('div');
        modalContainer.innerHTML = modalHTML;
        document.body.appendChild(modalContainer);

        const modalElement = document.getElementById(modalId);
        const bsModal = new bootstrap.Modal(modalElement);

        // Handle confirm button click
        document.getElementById(`confirmBtn_${modalId}`).addEventListener('click', () => {
            bsModal.hide();
            resolve(true);
        });

        // Handle modal hidden (cancel or X button)
        modalElement.addEventListener('hidden.bs.modal', () => {
            modalContainer.remove();
            resolve(false);
        });

        // Show modal
        bsModal.show();
    });
}

/**
 * Show notification message
 * Requires Bootstrap toast component
 */
function showNotification(message, type = 'info') {
    // Create toast element if it doesn't exist
    let toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.style.position = 'fixed';
        toastContainer.style.top = '20px';
        toastContainer.style.right = '20px';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    const toastEl = document.createElement('div');
    toastEl.className = `alert alert-${type} alert-dismissible fade show`;
    toastEl.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    toastContainer.appendChild(toastEl);

    // Auto-remove after 5 seconds
    setTimeout(() => {
        toastEl.remove();
    }, 5000);
}

/**
 * Initialize cart UI on page load
 */
document.addEventListener('DOMContentLoaded', function() {
    updateCartUI();
});
