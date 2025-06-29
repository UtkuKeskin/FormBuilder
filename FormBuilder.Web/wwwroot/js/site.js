// Main Application JavaSc
document.addEventListener('DOMContentLoaded', function() {
    'use strict';
    
    // Initialize all modules
    initializeModules();
    
    // Initialize Bootstrap tooltips
    initializeTooltips();
});

// Initialize all modules
function initializeModules() {
    // Core modules
    if (typeof ThemeManager !== 'undefined') ThemeManager.init();
    if (typeof LanguageSwitcher !== 'undefined') LanguageSwitcher.init();
    
    // Feature modules (will be use later)
    if (typeof FormValidation !== 'undefined') FormValidation.init();
    if (typeof Autocomplete !== 'undefined') Autocomplete.init();
    if (typeof DragDrop !== 'undefined') DragDrop.init();
    if (typeof CloudinaryUpload !== 'undefined') CloudinaryUpload.init();
    if (typeof TableActions !== 'undefined') TableActions.init();
}

// Initialize Bootstrap tooltips
function initializeTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Global utility functions
window.FormBuilder = {
    // Show toast notification
    showToast: function(message, type = 'info') {
        console.log(`[${type.toUpperCase()}] ${message}`);
        // TODO: Implement toast notifications
    },
    
    // Show loading spinner
    showLoading: function() {
        console.log('Loading...');
        // TODO: Implement loading spinner
    },
    
    // Hide loading spinner
    hideLoading: function() {
        console.log('Loading complete');
        // TODO: Implement loading spinner
    },
    
    // Get utility functions
    utils: Utils
};