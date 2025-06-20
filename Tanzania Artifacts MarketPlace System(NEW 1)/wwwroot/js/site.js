// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Sidebar toggle logic
document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("sidebar");
    const toggleBtn = document.getElementById("sidebarToggle");

    // Existing sidebar toggle for main layout
    toggleBtn?.addEventListener("click", function () {
        sidebar.classList.toggle("show");
    });

    // Close sidebar on outside click for mobile
    document.addEventListener("click", function (e) {
        if (
            sidebar &&
            toggleBtn &&
            !sidebar.contains(e.target) &&
            !toggleBtn.contains(e.target) &&
            sidebar.classList.contains("show")
        ) {
            sidebar.classList.remove("show");
        }
    });

    // Theme initialization
    const savedTheme = localStorage.getItem("theme") || "dark";
    document.body.classList.add(`${savedTheme}-theme`);

    const icon = document.getElementById("themeIcon");
    if (icon) {
        if (savedTheme === "light") {
            icon.classList.remove("bi-moon-stars-fill");
            icon.classList.add("bi-sun-fill");
        }
    }

    // Admin mobile sidebar toggle button
    const adminSidebarToggle = document.querySelector(".toggle-sidebar");
    const adminSidebar = document.querySelector(".admin-sidebar");

    if (adminSidebarToggle && adminSidebar) {
        adminSidebarToggle.addEventListener("click", function () {
            adminSidebar.classList.toggle("show");
        });
    }
});

// 🌗 Theme toggle function (dark/light)
function toggleTheme() {
    const body = document.body;
    const icon = document.getElementById("themeIcon");

    if (body.classList.contains("dark-theme")) {
        body.classList.remove("dark-theme");
        body.classList.add("light-theme");
        if (icon) {
            icon.classList.remove("bi-sun-fill");
            icon.classList.add("bi-moon-stars-fill");
        }
        localStorage.setItem("theme", "light");
    } else {
        body.classList.remove("light-theme");
        body.classList.add("dark-theme");
        if (icon) {
            icon.classList.remove("bi-moon-stars-fill");
            icon.classList.add("bi-sun-fill");
        }
        localStorage.setItem("theme", "dark");
    }
}

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
                console.log("Product added to cart.");
            } else {
                console.error('Add to cart failed');
            }
        })
        .catch(error => {
            console.error('Request error:', error);
        });
}
