class SupportTicketManager {
    constructor() {
        this.modal = null;
        this.form = null;
        this.button = null;
        this.init();
    }

    init() {
        // Wait for DOM
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.setup());
        } else {
            this.setup();
        }
    }

    setup() {
        // Get elements
        this.button = document.getElementById('support-button');
        this.form = document.getElementById('supportTicketForm');
        this.modal = new bootstrap.Modal(document.getElementById('supportTicketModal'));

        if (!this.button || !this.form) return;

        // Initialize tooltips
        const tooltip = new bootstrap.Tooltip(this.button);

        // Event listeners
        this.button.addEventListener('click', () => this.openModal());
        this.form.addEventListener('submit', (e) => this.handleSubmit(e));

        // Character counter
        const summaryField = document.getElementById('ticketSummary');
        const charCount = document.getElementById('charCount');
        
        summaryField.addEventListener('input', () => {
            const length = summaryField.value.length;
            charCount.textContent = length;
            
            // Change color based on length
            charCount.classList.remove('warning', 'danger');
            if (length > 400) {
                charCount.classList.add('danger');
            } else if (length > 300) {
                charCount.classList.add('warning');
            }
        });
    }

    openModal() {
        // Reset form
        this.form.reset();
        document.getElementById('charCount').textContent = '0';
        
        // Clear any previous alerts
        document.getElementById('supportAlertContainer').innerHTML = '';
        
        // Detect current context
        this.detectContext();
        
        // Check authentication
        const supportData = document.getElementById('support-data');
        const isAuthenticated = supportData.dataset.authenticated === 'true';
        
        const anonymousInfo = document.getElementById('anonymousInfo');
        if (!isAuthenticated) {
            anonymousInfo.classList.remove('d-none');
        } else {
            anonymousInfo.classList.add('d-none');
        }
        
        // Show modal
        this.modal.show();
    }

    detectContext() {
        const currentUrl = window.location.href;
        const currentPath = window.location.pathname;
        
        // Set current URL
        document.getElementById('currentUrl').value = currentUrl;
        document.getElementById('currentPageDisplay').value = currentUrl;
        
        // Try to detect template ID
        let templateId = null;
        
        // Check if we're on a template page
        const templateMatch = currentPath.match(/\/Template\/(\w+)\/(\d+)/);
        if (templateMatch) {
            templateId = templateMatch[2];
        }
        
        // Check if template ID is in query string
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.has('templateId')) {
            templateId = urlParams.get('templateId');
        }
        
        // Set template ID if found
        if (templateId) {
            document.getElementById('templateId').value = templateId;
        }
    }

    async handleSubmit(e) {
        e.preventDefault();
        
        // Validate form
        if (!this.form.checkValidity()) {
            e.stopPropagation();
            this.form.classList.add('was-validated');
            return;
        }
        
        // Get form data
        const formData = new FormData(this.form);
        const data = {
            summary: formData.get('summary'),
            priority: formData.get('priority'),
            currentUrl: formData.get('currentUrl'),
            templateId: formData.get('templateId') ? parseInt(formData.get('templateId')) : null
        };
        
        // Show loading state
        this.setLoadingState(true);
        
        try {
            const response = await fetch('/api/support/ticket', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data)
            });
            
            const result = await response.json();
            
            if (response.ok) {
                this.showAlert('success', 
                    `<strong>Success!</strong> Your support ticket has been created. 
                    Ticket ID: <code>${result.ticketId}</code>`);
                
                // Reset form after 2 seconds and close modal
                setTimeout(() => {
                    this.form.reset();
                    this.modal.hide();
                }, 2000);
            } else {
                this.showAlert('danger', 
                    `<strong>Error!</strong> ${result.error || 'Failed to create ticket. Please try again.'}`);
            }
        } catch (error) {
            console.error('Error:', error);
            this.showAlert('danger', 
                '<strong>Error!</strong> Network error. Please check your connection and try again.');
        } finally {
            this.setLoadingState(false);
        }
    }

    setLoadingState(loading) {
        const submitBtn = document.getElementById('submitTicketBtn');
        const btnText = document.getElementById('submitBtnText');
        const btnSpinner = document.getElementById('submitBtnSpinner');
        
        if (loading) {
            submitBtn.disabled = true;
            btnText.classList.add('d-none');
            btnSpinner.classList.remove('d-none');
        } else {
            submitBtn.disabled = false;
            btnText.classList.remove('d-none');
            btnSpinner.classList.add('d-none');
        }
    }

    showAlert(type, message) {
        const alertContainer = document.getElementById('supportAlertContainer');
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        alertContainer.appendChild(alert);
    }
}

// Initialize when ready
const supportTicketManager = new SupportTicketManager();