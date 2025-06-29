// Utility Functions Module
const Utils = (function() {
    'use strict';
    
    // Get CSRF token
    function getAntiForgeryToken() {
        return document.querySelector('[name="__RequestVerificationToken"]')?.value || '';
    }
    
    // Set cookie with secure options
    function setCookie(name, value, days) {
        const expires = new Date(Date.now() + days * 864e5).toUTCString();
        const isSecure = window.location.protocol === 'https:';
        
        let cookieString = `${name}=${value}; expires=${expires}; path=/; SameSite=Lax`;
        
        // Add Secure flag for HTTPS
        if (isSecure) {
            cookieString += '; Secure';
        }
        
        document.cookie = cookieString;
    }
    
    // Get cookie value
    function getCookie(name) {
        return document.cookie.split('; ')
            .find(row => row.startsWith(name + '='))
            ?.split('=')[1];
    }
    
    // Delete cookie
    function deleteCookie(name) {
        document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
    }
    
    // Check if user is authenticated
    function isAuthenticated() {
        return document.querySelector('[name="__RequestVerificationToken"]') !== null;
    }
    
    // Make authenticated POST request
    async function postAuthenticated(url, data) {
        const token = getAntiForgeryToken();
        if (!token && isAuthenticated()) {
            console.error('Anti-forgery token not found');
            return null;
        }
        
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: data
            });
            
            return response;
        } catch (error) {
            console.error(`Error posting to ${url}:`, error);
            return null;
        }
    }
    
    // Navigate preserving query parameters
    function navigatePreservingQuery(url) {
        const currentParams = window.location.search;
        const hash = window.location.hash;
        window.location.href = url + currentParams + hash;
    }
    
    // Make AJAX request with CSRF token (legacy support)
    async function postWithToken(url, data) {
        const formData = new URLSearchParams();
        for (const key in data) {
            formData.append(key, data[key]);
        }
        
        const token = getAntiForgeryToken();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: formData.toString()
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return response;
    }
    
    // Debounce function
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
    
    // Format date
    function formatDate(date, format = 'short') {
        const options = format === 'short' 
            ? { year: 'numeric', month: 'short', day: 'numeric' }
            : { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
            
        return new Date(date).toLocaleDateString(undefined, options);
    }
    
    // Public API -
    return {
        getAntiForgeryToken: getAntiForgeryToken,
        setCookie: setCookie,
        getCookie: getCookie,
        deleteCookie: deleteCookie,
        isAuthenticated: isAuthenticated,
        postAuthenticated: postAuthenticated,
        navigatePreservingQuery: navigatePreservingQuery,
        postWithToken: postWithToken,
        debounce: debounce,
        formatDate: formatDate
    };
})();