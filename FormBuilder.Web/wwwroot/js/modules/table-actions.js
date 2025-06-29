// Table Actions Module
const TableActions = (function() {
    'use strict';
    
    let selectedItems = new Set();
    
    // Initialize
    function init() {
        // Initialize clickable rows
        initClickableRows();
        
        // Initialize bulk selection
        initBulkSelection();
        
        // Initialize context menus
        initContextMenus();
    }
    
    // Initialize clickable rows
    function initClickableRows() {
        const tables = document.querySelectorAll('[data-clickable-rows="true"]');
        
        tables.forEach(table => {
            const rows = table.querySelectorAll('tbody tr');
            
            rows.forEach(row => {
                // Skip if row has no-click class
                if (row.classList.contains('no-click')) return;
                
                row.style.cursor = 'pointer';
                
                row.addEventListener('click', function(e) {
                    // Don't trigger on button/link clicks
                    if (e.target.tagName === 'A' || 
                        e.target.tagName === 'BUTTON' ||
                        e.target.closest('a') || 
                        e.target.closest('button')) {
                        return;
                    }
                    
                    // Don't trigger on checkbox clicks
                    if (e.target.type === 'checkbox') {
                        return;
                    }
                    
                    handleRowClick(this);
                });
            });
        });
    }
    
    // Handle row click
    function handleRowClick(row) {
        const url = row.dataset.url;
        const target = row.dataset.target || '_self';
        
        if (url) {
            if (target === '_blank') {
                window.open(url, target);
            } else {
                window.location.href = url;
            }
        }
    }
    
    // Initialize bulk selection
    function initBulkSelection() {
        // Select all checkbox
        const selectAllCheckboxes = document.querySelectorAll('[data-select-all]');
        
        selectAllCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', handleSelectAll);
        });
        
        // Individual checkboxes
        const itemCheckboxes = document.querySelectorAll('[data-select-item]');
        
        itemCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', handleItemSelect);
        });
        
        // Bulk action buttons
        const bulkActionButtons = document.querySelectorAll('[data-bulk-action]');
        
        bulkActionButtons.forEach(button => {
            button.addEventListener('click', handleBulkAction);
        });
    }
    
    // Handle select all
    function handleSelectAll(e) {
        const table = e.target.closest('table');
        const checkboxes = table.querySelectorAll('[data-select-item]');
        
        checkboxes.forEach(checkbox => {
            checkbox.checked = e.target.checked;
            updateSelectedItems(checkbox);
        });
        
        updateBulkActionButtons();
    }
    
    // Handle item select
    function handleItemSelect(e) {
        updateSelectedItems(e.target);
        updateSelectAllCheckbox(e.target);
        updateBulkActionButtons();
    }
    
    // Update selected items
    function updateSelectedItems(checkbox) {
        const itemId = checkbox.dataset.selectItem;
        
        if (checkbox.checked) {
            selectedItems.add(itemId);
        } else {
            selectedItems.delete(itemId);
        }
    }
    
    // Update select all checkbox
    function updateSelectAllCheckbox(checkbox) {
        const table = checkbox.closest('table');
        const selectAllCheckbox = table.querySelector('[data-select-all]');
        const allCheckboxes = table.querySelectorAll('[data-select-item]');
        const checkedCheckboxes = table.querySelectorAll('[data-select-item]:checked');
        
        if (selectAllCheckbox) {
            selectAllCheckbox.checked = allCheckboxes.length === checkedCheckboxes.length;
            selectAllCheckbox.indeterminate = 
                checkedCheckboxes.length > 0 && 
                checkedCheckboxes.length < allCheckboxes.length;
        }
    }
    
    // Update bulk action buttons
    function updateBulkActionButtons() {
        const bulkActionButtons = document.querySelectorAll('[data-bulk-action]');
        const selectedCount = selectedItems.size;
        
        bulkActionButtons.forEach(button => {
            button.disabled = selectedCount === 0;
            
            // Update counter if exists
            const counter = button.querySelector('.selected-count');
            if (counter) {
                counter.textContent = selectedCount;
            }
        });
        
        // Update selected count display
        const countDisplay = document.querySelector('[data-selected-count]');
        if (countDisplay) {
            countDisplay.textContent = selectedCount;
            countDisplay.closest('.selected-info').style.display = 
                selectedCount > 0 ? 'block' : 'none';
        }
    }
    
    // Handle bulk action
    async function handleBulkAction(e) {
        const action = e.currentTarget.dataset.bulkAction;
        const confirmMessage = e.currentTarget.dataset.confirmMessage;
        
        if (selectedItems.size === 0) {
            alert('No items selected');
            return;
        }
        
        if (confirmMessage && !confirm(confirmMessage)) {
            return;
        }
        
        // Perform action
        try {
            await performBulkAction(action, Array.from(selectedItems));
        } catch (error) {
            console.error('Bulk action failed:', error);
            alert('Action failed. Please try again.');
        }
    }
    
    // Perform bulk action
    async function performBulkAction(action, itemIds) {
        const endpoint = `/api/bulk/${action}`;
        
        const response = await Utils.postWithToken(endpoint, {
            action: action,
            items: itemIds
        });
        
        if (response.ok) {
            // Clear selection
            selectedItems.clear();
            
            // Reload or update UI
            window.location.reload();
        } else {
            throw new Error('Bulk action failed');
        }
    }
    
    // Initialize context menus
    function initContextMenus() {
        const contextMenuTriggers = document.querySelectorAll('[data-context-menu]');
        
        contextMenuTriggers.forEach(trigger => {
            trigger.addEventListener('contextmenu', handleContextMenu);
        });
        
        // Close context menu on click outside
        document.addEventListener('click', closeAllContextMenus);
    }
    
    // Handle context menu
    function handleContextMenu(e) {
        e.preventDefault();
        
        closeAllContextMenus();
        
        const menuId = e.currentTarget.dataset.contextMenu;
        const menu = document.getElementById(menuId);
        
        if (menu) {
            menu.style.display = 'block';
            menu.style.left = e.pageX + 'px';
            menu.style.top = e.pageY + 'px';
        }
    }
    
    // Close all context menus
    function closeAllContextMenus() {
        const menus = document.querySelectorAll('.context-menu');
        menus.forEach(menu => {
            menu.style.display = 'none';
        });
    }
    
    // Get selected items
    function getSelectedItems() {
        return Array.from(selectedItems);
    }
    
    // Clear selection
    function clearSelection() {
        selectedItems.clear();
        
        const checkboxes = document.querySelectorAll('[data-select-item]');
        checkboxes.forEach(checkbox => {
            checkbox.checked = false;
        });
        
        updateBulkActionButtons();
    }
    
    // Public API
    return {
        init: init,
        getSelectedItems: getSelectedItems,
        clearSelection: clearSelection
    };
})();