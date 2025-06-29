// Theme Manager Module
const ThemeManager = (function() {
    'use strict';
    
    let switcher;
    let themeLink;
    
    // Initialize
    function init() {
        switcher = document.getElementById('themeSwitcher');
        themeLink = document.getElementById('theme-style');
        
        if (!switcher || !themeLink) return;
        
        // Load saved theme
        const savedTheme = Utils.getCookie('theme') || 'light';
        applyTheme(savedTheme);
        switcher.checked = savedTheme === 'dark';
        
        // Add event listener
        switcher.addEventListener('change', handleThemeChange);
    }
    
    // Handle theme change
    async function handleThemeChange(e) {
        const theme = e.target.checked ? 'dark' : 'light';
        applyTheme(theme);
        Utils.setCookie('theme', theme, 365);
        
        // Update server if logged in
        if (Utils.isAuthenticated()) {
            await updateServerPreference(theme);
        }
    }
    
    // Apply theme
    function applyTheme(theme) {
        themeLink.href = `/css/themes/${theme}.css`;
        document.documentElement.setAttribute('data-theme', theme);
    }
    
    // Update server preference
    async function updateServerPreference(theme) {
        const response = await Utils.postAuthenticated('/Settings/UpdateTheme', `theme=${theme}`);
        
        if (!response || !response.ok) {
            console.error('Failed to update theme preference on server');
        }
    }
    
    // Public API
    return { init };
})();