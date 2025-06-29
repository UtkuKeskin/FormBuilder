// Form Validation Module
const FormValidation = (function() {
    'use strict';
    
    // Initialize validation
    function init() {
        // Bootstrap validation styles
        addBootstrapValidation();
        
        // Custom validation rules
        addCustomValidators();
    }
    
    // Add Bootstrap validation styles
    function addBootstrapValidation() {
        const forms = document.querySelectorAll('.needs-validation');
        
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                
                form.classList.add('was-validated');
            }, false);
        });
    }
    
    // Add custom validators
    function addCustomValidators() {
        // Email validation
        addEmailValidator();
        
        // Integer validation for question answers
        addIntegerValidator();
        
        // Required field validation
        addRequiredValidator();
    }
    
    // Email validator
    function addEmailValidator() {
        const emailInputs = document.querySelectorAll('input[type="email"]');
        emailInputs.forEach(input => {
            input.addEventListener('blur', function() {
                validateEmail(this);
            });
        });
    }
    
    // Validate email format
    function validateEmail(input) {
        const email = input.value;
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        
        if (!re.test(email) && email !== '') {
            showError(input, 'Please enter a valid email address');
            return false;
        }
        
        clearError(input);
        return true;
    }
    
    // Integer validator
    function addIntegerValidator() {
        const intInputs = document.querySelectorAll('input[data-type="integer"]');
        intInputs.forEach(input => {
            input.addEventListener('input', function() {
                validateInteger(this);
            });
        });
    }
    
    // Validate integer input
    function validateInteger(input) {
        const value = input.value;
        
        if (value && !Number.isInteger(Number(value))) {
            showError(input, 'Please enter a valid number');
            return false;
        }
        
        clearError(input);
        return true;
    }
    
    // Required field validator
    function addRequiredValidator() {
        const requiredInputs = document.querySelectorAll('[required]');
        requiredInputs.forEach(input => {
            input.addEventListener('blur', function() {
                validateRequired(this);
            });
        });
    }
    
    // Validate required field
    function validateRequired(input) {
        if (!input.value.trim()) {
            showError(input, 'This field is required');
            return false;
        }
        
        clearError(input);
        return true;
    }
    
    // Show error message
    function showError(input, message) {
        const formGroup = input.closest('.mb-3');
        let errorDiv = formGroup.querySelector('.invalid-feedback');
        
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.className = 'invalid-feedback';
            formGroup.appendChild(errorDiv);
        }
        
        errorDiv.textContent = message;
        input.classList.add('is-invalid');
    }
    
    // Clear error message
    function clearError(input) {
        input.classList.remove('is-invalid');
        const errorDiv = input.closest('.mb-3')?.querySelector('.invalid-feedback');
        if (errorDiv) {
            errorDiv.remove();
        }
    }
    
    // Validate entire form
    function validateForm(formId) {
        const form = document.getElementById(formId);
        if (!form) return false;
        
        let isValid = true;
        
        // Check all required fields
        form.querySelectorAll('[required]').forEach(input => {
            if (!validateRequired(input)) {
                isValid = false;
            }
        });
        
        // Check email fields
        form.querySelectorAll('input[type="email"]').forEach(input => {
            if (!validateEmail(input)) {
                isValid = false;
            }
        });
        
        // Check integer fields
        form.querySelectorAll('input[data-type="integer"]').forEach(input => {
            if (!validateInteger(input)) {
                isValid = false;
            }
        });
        
        return isValid;
    }
    
    // Public API
    return {
        init: init,
        validateForm: validateForm,
        showError: showError,
        clearError: clearError
    };
})();