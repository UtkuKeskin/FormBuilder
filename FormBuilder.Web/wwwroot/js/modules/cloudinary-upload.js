// Cloudinary Upload Module
const CloudinaryUpload = (function() {
    'use strict';
    
    let uploadArea = null;
    let imageInput = null;
    let imagePreview = null;
    let progressBar = null;
    
    function init() {
        console.log('CloudinaryUpload module initialized');
        
        uploadArea = document.querySelector('[data-upload-area]');
        imageInput = document.querySelector('input[type="file"][name="ImageFile"]');
        imagePreview = document.getElementById('imagePreview');
        
        if (!uploadArea || !imageInput) return;
        
        setupEventListeners();
        createProgressBar();
    }
    
    function setupEventListeners() {
        // Click to browse
        uploadArea.addEventListener('click', (e) => {
            if (e.target === uploadArea || e.target.parentElement === uploadArea) {
                imageInput.click();
            }
        });
        
        // Drag and drop events
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            uploadArea.addEventListener(eventName, preventDefaults, false);
        });
        
        ['dragenter', 'dragover'].forEach(eventName => {
            uploadArea.addEventListener(eventName, highlight, false);
        });
        
        ['dragleave', 'drop'].forEach(eventName => {
            uploadArea.addEventListener(eventName, unhighlight, false);
        });
        
        uploadArea.addEventListener('drop', handleDrop, false);
        
        // File input change
        imageInput.addEventListener('change', handleFiles);
    }
    
    function createProgressBar() {
        progressBar = document.createElement('div');
        progressBar.className = 'progress mt-3 d-none';
        progressBar.innerHTML = `
            <div class="progress-bar progress-bar-striped progress-bar-animated" 
                 role="progressbar" style="width: 0%">0%</div>
        `;
        uploadArea.parentElement.appendChild(progressBar);
    }
    
    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }
    
    function highlight(e) {
        uploadArea.classList.add('highlight');
    }
    
    function unhighlight(e) {
        uploadArea.classList.remove('highlight');
    }
    
    function handleDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;
        
        if (files.length > 0) {
            handleFile(files[0]);
        }
    }
    
    function handleFiles(e) {
        const files = e.target.files;
        if (files.length > 0) {
            handleFile(files[0]);
        }
    }
    
    function handleFile(file) {
        // Validate file type
        if (!file.type.match('image.*')) {
            showError('Please select an image file');
            return;
        }
        
        // Validate file size (5MB)
        const maxSize = 5 * 1024 * 1024;
        if (file.size > maxSize) {
            showError('File size must be less than 5MB');
            return;
        }
        
        // Update file input
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(file);
        imageInput.files = dataTransfer.files;
        
        // Show preview
        showPreview(file);
        
        // Show progress (simulated for client-side)
        showProgress();
    }
    
    function showPreview(file) {
        const reader = new FileReader();
        
        reader.onload = function(e) {
            imagePreview.innerHTML = `
                <div class="position-relative d-inline-block">
                    <img src="${e.target.result}" class="img-thumbnail" style="max-height: 200px;">
                    <button type="button" class="btn btn-sm btn-danger position-absolute top-0 end-0 m-1" 
                            onclick="CloudinaryUpload.clearImage()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <p class="mt-2 mb-0">
                    <small class="text-muted">
                        <i class="fas fa-check-circle text-success"></i> 
                        ${file.name} (${formatFileSize(file.size)})
                    </small>
                </p>
            `;
        };
        
        reader.readAsDataURL(file);
    }
    
    function showProgress() {
        progressBar.classList.remove('d-none');
        const bar = progressBar.querySelector('.progress-bar');
        
        // Simulate progress
        let progress = 0;
        const interval = setInterval(() => {
            progress += 10;
            bar.style.width = progress + '%';
            bar.textContent = progress + '%';
            
            if (progress >= 100) {
                clearInterval(interval);
                setTimeout(() => {
                    progressBar.classList.add('d-none');
                    bar.style.width = '0%';
                    bar.textContent = '0%';
                }, 1000);
            }
        }, 100);
    }
    
    function showError(message) {
        const alert = document.createElement('div');
        alert.className = 'alert alert-danger alert-dismissible fade show mt-2';
        alert.innerHTML = `
            <i class="fas fa-exclamation-circle"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        uploadArea.parentElement.appendChild(alert);
        
        setTimeout(() => {
            alert.remove();
        }, 5000);
    }
    
    function clearImage() {
        imageInput.value = '';
        imagePreview.innerHTML = '';
    }
    
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }
    
    // Public API
    return { 
        init,
        clearImage
    };
})();