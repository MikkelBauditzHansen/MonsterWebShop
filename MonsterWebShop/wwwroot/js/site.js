// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {

    const navbar = document.querySelector(".top-nav");

    if (!navbar) {
        return;
    }

    const isHomePage = window.location.pathname === "/";

    if (!isHomePage) {
        return;
    }

    function updateNavbar() {

        if (window.scrollY > 50) {
            navbar.classList.add("light-header");
        } else {
            navbar.classList.remove("light-header");
        }
    }

    window.addEventListener("scroll", updateNavbar);

    updateNavbar();
});