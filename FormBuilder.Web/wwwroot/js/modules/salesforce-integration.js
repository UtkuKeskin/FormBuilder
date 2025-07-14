// Salesforce Integration Module
const SalesforceIntegration = (function() {
    'use strict';

    let formData = {};
    const STORAGE_KEY = 'salesforce_draft';

    // Initialize module
    function init() {
        if (!document.getElementById('salesforceForm')) return;

        setupFormHandlers();
        loadDraftData();
        setupAutoSave();
    }

    // Setup form handlers
    function setupFormHandlers() {
        const form = document.getElementById('salesforceForm');
        const submitBtn = document.getElementById('submitBtn');

        // Form submission
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            if (!form.checkValidity()) {
                e.stopPropagation();
                form.classList.add('was-validated');
                return;
            }

            showLoadingModal();
            
            // Add a small delay to show the loading state
            setTimeout(() => {
                form.submit();
            }, 500);
        });

        // Enhance validation feedback
        const inputs = form.querySelectorAll('.form-control, .form-select');
        inputs.forEach(input => {
            input.addEventListener('blur', function() {
                validateField(this);
            });
        });
    }

    // Field validation
    function validateField(field) {
        const isValid = field.checkValidity();
        
        if (isValid) {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        } else {
            field.classList.remove('is-valid');
            field.classList.add('is-invalid');
        }
    }

    // Auto-save functionality
    function setupAutoSave() {
        const form = document.getElementById('salesforceForm');
        const inputs = form.querySelectorAll('input, select, textarea');

        inputs.forEach(input => {
            input.addEventListener('input', debounce(function() {
                saveDraftData();
                showSaveIndicator();
            }, 1000));
        });
    }

    // Save draft data to localStorage
    function saveDraftData() {
        const form = document.getElementById('salesforceForm');
        const formData = new FormData(form);
        const data = {};

        for (let [key, value] of formData.entries()) {
            data[key] = value;
        }

        localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
    }

    // Load draft data from localStorage
    function loadDraftData() {
        const savedData = localStorage.getItem(STORAGE_KEY);
        if (!savedData) return;

        try {
            const data = JSON.parse(savedData);
            const form = document.getElementById('salesforceForm');

            Object.keys(data).forEach(key => {
                const field = form.elements[key];
                if (field && field.type !== 'hidden') {
                    field.value = data[key];
                }
            });

            showNotification('Draft data loaded', 'info');
        } catch (e) {
            console.error('Error loading draft data:', e);
        }
    }

    // Clear draft data
    function clearDraftData() {
        localStorage.removeItem(STORAGE_KEY);
    }

    // Show loading modal
    function showLoadingModal() {
        const modal = new bootstrap.Modal(document.getElementById('loadingModal'));
        modal.show();
    }

    // Show save indicator
    function showSaveIndicator() {
        const indicator = document.getElementById('saveIndicator');
        if (!indicator) {
            const div = document.createElement('div');
            div.id = 'saveIndicator';
            div.className = 'position-fixed bottom-0 end-0 m-3';
            div.innerHTML = `
                <div class="toast show" role="alert">
                    <div class="toast-body">
                        <i class="fas fa-save text-success me-2"></i>
                        Draft saved
                    </div>
                </div>
            `;
            document.body.appendChild(div);

            setTimeout(() => {
                div.remove();
            }, 2000);
        }
    }

    // Show notification
    function showNotification(message, type = 'info') {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3`;
        alertDiv.style.zIndex = '9999';
        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        document.body.appendChild(alertDiv);

        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    }

    // Debounce helper
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

    // Check integration status
    function checkStatus(userId) {
        fetch(`/Profile/SalesforceStatus?userId=${userId}`)
            .then(response => response.json())
            .then(data => {
                updateStatusDisplay(data);
            })
            .catch(error => {
                console.error('Error checking status:', error);
            });
    }

    // Update status display
    function updateStatusDisplay(status) {
        const statusBadge = document.getElementById('integrationStatus');
        if (!statusBadge) return;

        if (status.isIntegrated) {
            statusBadge.innerHTML = `
                <span class="badge bg-success">
                    <i class="fas fa-check-circle"></i> Integrated
                </span>
            `;
        } else {
            statusBadge.innerHTML = `
                <span class="badge bg-secondary">
                    <i class="fas fa-times-circle"></i> Not Integrated
                </span>
            `;
        }
    }

    // Public API
    return {
        init: init,
        checkStatus: checkStatus,
        clearDraft: clearDraftData
    };
})();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    SalesforceIntegration.init();
});