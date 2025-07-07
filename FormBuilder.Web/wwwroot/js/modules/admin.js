// Admin Module
const AdminModule = (function() {
    'use strict';
    
    // Initialize
    function init() {
        console.log('Admin module initialized');
        
        // Initialize tooltips
        initTooltips();
        
        // Initialize confirmation dialogs
        initConfirmDialogs();
        
        // Initialize charts if on dashboard
        if (document.getElementById('activityChart')) {
            // Chart is initialized inline in the view
            console.log('Dashboard charts loaded');
        }
    }
    
    // Initialize tooltips
    function initTooltips() {
        const tooltipTriggerList = [].slice.call(
            document.querySelectorAll('[data-bs-toggle="tooltip"]')
        );
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
    
    // Initialize confirmation dialogs
    function initConfirmDialogs() {
        // Admin self-removal warning is handled inline
        console.log('Confirmation dialogs ready');
    }
    
    // Public API
    return {
        init: init
    };
})();

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', AdminModule.init);