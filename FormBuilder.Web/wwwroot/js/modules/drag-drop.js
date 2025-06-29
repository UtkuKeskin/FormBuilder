// Drag & Drop Module
const DragDrop = (function() {
    'use strict';
    
    let draggedElement = null;
    
    // Initialize drag and drop
    function init() {
        // Initialize sortable lists
        initSortableLists();
        
        // Initialize file upload areas
        initFileUploadAreas();
    }
    
    // Initialize sortable lists
    function initSortableLists() {
        const sortableLists = document.querySelectorAll('[data-sortable="true"]');
        
        sortableLists.forEach(list => {
            makeSortable(list);
        });
    }
    
    // Make list sortable
    function makeSortable(list) {
        const items = list.querySelectorAll('[data-sortable-item]');
        
        items.forEach(item => {
            item.draggable = true;
            
            item.addEventListener('dragstart', handleDragStart);
            item.addEventListener('dragend', handleDragEnd);
            item.addEventListener('dragover', handleDragOver);
            item.addEventListener('drop', handleDrop);
            item.addEventListener('dragenter', handleDragEnter);
            item.addEventListener('dragleave', handleDragLeave);
        });
    }
    
    // Handle drag start
    function handleDragStart(e) {
        draggedElement = this;
        this.classList.add('dragging');
        
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', this.innerHTML);
    }
    
    // Handle drag end
    function handleDragEnd(e) {
        this.classList.remove('dragging');
        
        const items = document.querySelectorAll('[data-sortable-item]');
        items.forEach(item => {
            item.classList.remove('drag-over');
        });
        
        // Trigger order changed event
        const list = this.closest('[data-sortable="true"]');
        if (list) {
            const event = new CustomEvent('sortorderchanged', {
                detail: { items: getOrderedItems(list) }
            });
            list.dispatchEvent(event);
        }
    }
    
    // Handle drag over
    function handleDragOver(e) {
        if (e.preventDefault) {
            e.preventDefault();
        }
        
        e.dataTransfer.dropEffect = 'move';
        return false;
    }
    
    // Handle drop
    function handleDrop(e) {
        if (e.stopPropagation) {
            e.stopPropagation();
        }
        
        if (draggedElement !== this) {
            const list = this.parentNode;
            const allItems = Array.from(list.children);
            const draggedIndex = allItems.indexOf(draggedElement);
            const targetIndex = allItems.indexOf(this);
            
            if (draggedIndex < targetIndex) {
                this.parentNode.insertBefore(draggedElement, this.nextSibling);
            } else {
                this.parentNode.insertBefore(draggedElement, this);
            }
        }
        
        return false;
    }
    
    // Handle drag enter
    function handleDragEnter(e) {
        this.classList.add('drag-over');
    }
    
    // Handle drag leave
    function handleDragLeave(e) {
        this.classList.remove('drag-over');
    }
    
    // Get ordered items
    function getOrderedItems(list) {
        const items = list.querySelectorAll('[data-sortable-item]');
        return Array.from(items).map((item, index) => ({
            id: item.dataset.id || item.id,
            order: index
        }));
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
        
        // Click to upload
        area.addEventListener('click', () => input.click());
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
    
    // Make element sortable (public method)
    function makeElementSortable(element) {
        element.setAttribute('data-sortable', 'true');
        makeSortable(element);
    }
    
    // Public API
    return {
        init: init,
        makeSortable: makeElementSortable,
        getOrderedItems: getOrderedItems
    };
})();