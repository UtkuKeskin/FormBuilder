const PasswordToggle = (function() {
    'use strict';
    
    // Initialize password toggles
    function init() {
        const toggleButtons = document.querySelectorAll('[data-password-toggle]');
        toggleButtons.forEach(button => {
            button.addEventListener('click', handleToggle);
        });
    }
    
    // Handle toggle click
    function handleToggle(e) {
        e.preventDefault();
        const button = e.currentTarget;
        const targetId = button.getAttribute('data-password-toggle');
        const input = document.getElementById(targetId);
        
        toggleVisibility(input, button);
    }
    
    // Toggle password visibility
    function toggleVisibility(input, button) {
        const icon = button.querySelector('i');
        
        if (input.type === 'password') {
            input.type = 'text';
            updateIcon(icon, true);
        } else {
            input.type = 'password';
            updateIcon(icon, false);
        }
    }
    
    // Update icon
    function updateIcon(icon, isVisible) {
        if (isVisible) {
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    }
    
    return { init };
})();

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', PasswordToggle.init);