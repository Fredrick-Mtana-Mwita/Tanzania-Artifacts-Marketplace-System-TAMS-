// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// JavaScript code for sidebar toggle
document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("sidebar");
    const toggleBtn = document.getElementById("sidebarToggle");

    toggleBtn?.addEventListener("click", function () {
        sidebar.classList.toggle("show");
    });

    // Optional: click outside sidebar closes it on small screens
    document.addEventListener("click", function (e) {
        if (!sidebar.contains(e.target) && !toggleBtn.contains(e.target) && sidebar.classList.contains("show")) {
            sidebar.classList.remove("show");
        }
    });

    // Optionally refresh cart count on page load
    // refreshCartCount();
});

// 🔁 Refresh Cart Count in Navbar after add/remove
function refreshCartCount() {
    fetch('/CartCount')
        .then(response => response.text())
        .then(html => {
            const container = document.getElementById('cartCountContainer');
            if (container) {
                container.innerHTML = html;
            }
        })
        .catch(error => console.error('Cart count refresh failed:', error));
}

// ✅ Add to Cart via AJAX
function addToCart(productId) {
    fetch('/Cart/Add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId: productId, quantity: 1 })
    })
        .then(response => {
            if (response.ok) {
                refreshCartCount(); // 🔁 Update cart badge
                // Optional: show a toast or confirmation
                console.log("Product added to cart.");
            } else {
                console.error('Add to cart failed');
            }
        })
        .catch(error => {
            console.error('Request error:', error);
        });
}
