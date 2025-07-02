// Drag & Drop Module - Enhanced with SortableJS
const DragDrop = (function() {
    'use strict';
    
    let sortableInstance = null;
    
    // Initialize drag and drop
    function init() {
        // Initialize question sorting
        initQuestionSorting();
        
        // Initialize file upload areas
        initFileUploadAreas();
    }
    
    // Initialize question sorting with SortableJS
    function initQuestionSorting() {
        const container = document.getElementById('questions-container');
        if (!container) return;
        
        // Check if SortableJS is loaded
        if (typeof Sortable === 'undefined') {
            console.error('SortableJS not loaded');
            return;
        }
        
        sortableInstance = new Sortable(container, {
            animation: 150,
            handle: '.question-handle',
            draggable: '.question-item',
            ghostClass: 'sortable-ghost',
            chosenClass: 'sortable-chosen',
            dragClass: 'sortable-drag',
            
            onEnd: function(evt) {
                // Notify QuestionManager about order change
                if (typeof QuestionManager !== 'undefined') {
                    QuestionManager.handleOrderChange();
                }
            }
        });
    }
    
    // Initialize file upload areas
    function initFileUploadAreas() {
        const uploadAreas = document.querySelectorAll('[data-upload-area]');
        
        uploadAreas.forEach(area => {
            setupFileUpload(area);
        });
    }
    
    // Setup file upload
    function setupFileUpload(area) {
        const input = area.querySelector('input[type="file"]');
        if (!input) return;
        
        // Prevent defaults
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            area.addEventListener(eventName, preventDefaults, false);
        });
        
        // Highlight drop area
        ['dragenter', 'dragover'].forEach(eventName => {
            area.addEventListener(eventName, () => highlight(area), false);
        });
        
        ['dragleave', 'drop'].forEach(eventName => {
            area.addEventListener(eventName, () => unhighlight(area), false);
        });
        
        // Handle dropped files
        area.addEventListener('drop', e => handleFileDrop(e, input), false);
    }
    
    // Prevent default drag behaviors
    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }
    
    // Highlight drop area
    function highlight(area) {
        area.classList.add('highlight');
    }
    
    // Remove highlight
    function unhighlight(area) {
        area.classList.remove('highlight');
    }
    
    // Handle file drop
    function handleFileDrop(e, input) {
        const dt = e.dataTransfer;
        const files = dt.files;
        
        if (files.length > 0) {
            input.files = files;
            
            // Trigger change event
            const event = new Event('change', { bubbles: true });
            input.dispatchEvent(event);
        }
    }
    
    // Destroy sortable instance
    function destroy() {
        if (sortableInstance) {
            sortableInstance.destroy();
            sortableInstance = null;
        }
    }
    
    // Public API
    return {
        init: init,
        destroy: destroy,
        reinitialize: function() {
            destroy();
            init();
        }
    };
})();