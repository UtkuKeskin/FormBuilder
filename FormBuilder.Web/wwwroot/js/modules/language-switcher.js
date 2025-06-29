// Language Switcher Module
const LanguageSwitcher = (function() {
    'use strict';
    
    // Initialize
    function init() {
        const languageButtons = document.querySelectorAll('[data-language]');
        const currentLang = getCurrentLanguage();
        
        console.log('Current language:', currentLang); // Debug
        
        // Set active button
        languageButtons.forEach(btn => {
            btn.classList.remove('active');
            if (btn.getAttribute('data-language') === currentLang) {
                btn.classList.add('active');
            }
            btn.addEventListener('click', handleLanguageChange);
        });
    }
    
    // Get current language
    function getCurrentLanguage() {
        // Check ASP.NET Core culture cookie
        const cultureCookie = document.cookie.split('; ')
            .find(row => row.startsWith('.AspNetCore.Culture='));
            
        if (cultureCookie) {
            try {
                const value = decodeURIComponent(cultureCookie.split('=')[1]);
                const culture = value.split('|')[0].split('=')[1];
                console.log('Culture from cookie:', culture); // Debug
                return culture;
            } catch (e) {
                console.error('Error parsing culture cookie:', e);
            }
        }
        
        // Check HTML lang attribute
        const htmlLang = document.documentElement.lang;
        if (htmlLang) {
            console.log('Culture from HTML lang:', htmlLang); // Debug
            return htmlLang;
        }
        
        return 'en';
    }
    
    // Handle language change
    async function handleLanguageChange(e) {
        e.preventDefault();
        const button = e.currentTarget;
        const language = button.getAttribute('data-language');
        
        console.log('Changing language to:', language); // Debug
        
        // Update UI immediately
        document.querySelectorAll('[data-language]').forEach(btn => {
            btn.classList.remove('active');
        });
        button.classList.add('active');
        
        // Update server preference if authenticated (Utils check)
        if (typeof Utils !== 'undefined' && Utils.isAuthenticated && Utils.isAuthenticated()) {
            try {
                await updateServerLanguage(language);
            } catch (error) {
                console.error('Failed to update server language:', error);
            }
        }
        
        // Use SetLanguage action to properly set culture
        const returnUrl = window.location.pathname + window.location.search;
        window.location.href = `/Settings/SetLanguage?culture=${language}&returnUrl=${encodeURIComponent(returnUrl)}`;
    }
    
    // Update server preference
    async function updateServerLanguage(language) {
        if (typeof Utils !== 'undefined' && Utils.postAuthenticated) {
            const response = await Utils.postAuthenticated('/Settings/UpdateLanguage', `language=${language}`);
            if (!response || !response.ok) {
                console.error('Failed to update language preference on server');
            }
        }
    }
    
    // Public API
    return { init };
})();