
document.addEventListener("DOMContentLoaded", function () {
    const themeToggle = document.getElementById("themeToggle");
    const themeIcon = document.getElementById("themeIcon");

    // Stop if the theme button is not on this page.
    if (!themeToggle || !themeIcon) {
        return;
    }

    // Restore the user's saved theme.
    const savedTheme = localStorage.getItem("ajacas-theme");
    const initialTheme = savedTheme || "dark";

    applyTheme(initialTheme);

    // Toggle between dark mode and light mode.
    themeToggle.addEventListener("click", function () {
        const currentTheme =
            document.documentElement.getAttribute("data-theme") || "dark";

        const newTheme = currentTheme === "dark" ? "light" : "dark";

        applyTheme(newTheme);
        localStorage.setItem("ajacas-theme", newTheme);
    });

    // Apply the theme and update the icon.
    function applyTheme(theme) {
        document.documentElement.setAttribute("data-theme", theme);

        if (theme === "light") {
            // In light mode, show the moon to switch back to dark mode.
            themeIcon.className = "bi bi-moon-stars-fill";

            themeToggle.setAttribute("aria-label", "Switch to dark mode");
            themeToggle.setAttribute("title", "Switch to dark mode");
        } else {
            // In dark mode, show the sun to switch to light mode.
            themeIcon.className = "bi bi-sun-fill";

            themeToggle.setAttribute("aria-label", "Switch to light mode");
            themeToggle.setAttribute("title", "Switch to light mode");
        }
    }
});
