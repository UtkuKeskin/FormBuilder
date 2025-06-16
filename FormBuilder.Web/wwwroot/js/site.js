// Small, focused functions

// Theme Management
function initThemeSwitcher() {
    const switcher = document.getElementById('themeSwitcher');
    const savedTheme = getCookie('theme') || 'light';
    
    applyTheme(savedTheme);
    switcher.checked = savedTheme === 'dark';
    
    switcher.addEventListener('change', handleThemeChange);
}

function handleThemeChange(e) {
    const theme = e.target.checked ? 'dark' : 'light';
    applyTheme(theme);
    setCookie('theme', theme, 365);
}

function applyTheme(theme) {
    const themeLink = document.getElementById('theme-style');
    themeLink.href = `/css/themes/${theme}.css`;
}

// Cookie Helpers
function setCookie(name, value, days) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = `${name}=${value}; expires=${expires}; path=/`;
}

function getCookie(name) {
    return document.cookie.split('; ')
        .find(row => row.startsWith(name + '='))
        ?.split('=')[1];
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function() {
    initThemeSwitcher();
});
