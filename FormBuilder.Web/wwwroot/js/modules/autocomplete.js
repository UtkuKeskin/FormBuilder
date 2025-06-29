// Autocomplete Module
const Autocomplete = (function() {
    'use strict';
    
    let activeAutocomplete = null;
    
    // Initialize autocomplete
    function init() {
        // Initialize tag autocomplete
        initTagAutocomplete();
        
        // Initialize user autocomplete
        initUserAutocomplete();
    }
    
    // Initialize tag autocomplete
    function initTagAutocomplete() {
        const tagInputs = document.querySelectorAll('[data-autocomplete="tags"]');
        tagInputs.forEach(input => {
            setupAutocomplete(input, '/api/tags/search', formatTagItem);
        });
    }
    
    // Initialize user autocomplete
    function initUserAutocomplete() {
        const userInputs = document.querySelectorAll('[data-autocomplete="users"]');
        userInputs.forEach(input => {
            setupAutocomplete(input, '/api/users/search', formatUserItem);
        });
    }
    
    // Setup autocomplete for an input
    function setupAutocomplete(input, endpoint, formatter) {
        const wrapper = createWrapper(input);
        const dropdown = createDropdown();
        wrapper.appendChild(dropdown);
        
        // Event listeners
        input.addEventListener('input', Utils.debounce(function() {
            handleInput(this, endpoint, dropdown, formatter);
        }, 300));
        
        input.addEventListener('blur', function() {
            setTimeout(() => hideDropdown(dropdown), 200);
        });
        
        input.addEventListener('keydown', function(e) {
            handleKeydown(e, dropdown);
        });
    }
    
    // Create wrapper div
    function createWrapper(input) {
        const wrapper = document.createElement('div');
        wrapper.className = 'autocomplete-wrapper position-relative';
        input.parentNode.insertBefore(wrapper, input);
        wrapper.appendChild(input);
        return wrapper;
    }
    
    // Create dropdown
    function createDropdown() {
        const dropdown = document.createElement('div');
        dropdown.className = 'autocomplete-dropdown position-absolute w-100 bg-white border rounded shadow-sm d-none';
        dropdown.style.zIndex = '1000';
        dropdown.style.maxHeight = '200px';
        dropdown.style.overflowY = 'auto';
        return dropdown;
    }
    
    // Handle input event
    async function handleInput(input, endpoint, dropdown, formatter) {
        const query = input.value.trim();
        
        if (query.length < 2) {
            hideDropdown(dropdown);
            return;
        }
        
        try {
            const response = await fetch(`${endpoint}?q=${encodeURIComponent(query)}`);
            const data = await response.json();
            
            if (data.length > 0) {
                showResults(dropdown, data, input, formatter);
            } else {
                hideDropdown(dropdown);
            }
        } catch (error) {
            console.error('Autocomplete error:', error);
            hideDropdown(dropdown);
        }
    }
    
    // Show results in dropdown
    function showResults(dropdown, items, input, formatter) {
        dropdown.innerHTML = '';
        
        items.forEach((item, index) => {
            const div = document.createElement('div');
            div.className = 'autocomplete-item p-2';
            div.innerHTML = formatter(item);
            div.dataset.value = item.value || item.name || item.id;
            div.dataset.index = index;
            
            div.addEventListener('click', function() {
                selectItem(input, this.dataset.value);
                hideDropdown(dropdown);
            });
            
            div.addEventListener('mouseenter', function() {
                setActiveItem(dropdown, index);
            });
            
            dropdown.appendChild(div);
        });
        
        showDropdown(dropdown);
        activeAutocomplete = { dropdown, input, items };
    }
    
    // Format tag item
    function formatTagItem(tag) {
        return `<span class="badge bg-secondary me-1">${tag.name}</span> (${tag.count} uses)`;
    }
    
    // Format user item
    function formatUserItem(user) {
        return `<strong>${user.name}</strong> <small class="text-muted">${user.email}</small>`;
    }
    
    // Handle keyboard navigation
    function handleKeydown(e, dropdown) {
        if (!activeAutocomplete || dropdown.classList.contains('d-none')) return;
        
        const items = dropdown.querySelectorAll('.autocomplete-item');
        const activeIndex = Array.from(items).findIndex(item => 
            item.classList.contains('active')
        );
        
        switch(e.key) {
            case 'ArrowDown':
                e.preventDefault();
                setActiveItem(dropdown, activeIndex + 1);
                break;
            case 'ArrowUp':
                e.preventDefault();
                setActiveItem(dropdown, activeIndex - 1);
                break;
            case 'Enter':
                e.preventDefault();
                if (activeIndex >= 0) {
                    items[activeIndex].click();
                }
                break;
            case 'Escape':
                hideDropdown(dropdown);
                break;
        }
    }
    
    // Set active item
    function setActiveItem(dropdown, index) {
        const items = dropdown.querySelectorAll('.autocomplete-item');
        
        items.forEach(item => item.classList.remove('active', 'bg-light'));
        
        if (index >= 0 && index < items.length) {
            items[index].classList.add('active', 'bg-light');
        }
    }
    
    // Select item
    function selectItem(input, value) {
        input.value = value;
        input.dispatchEvent(new Event('change'));
    }
    
    // Show dropdown
    function showDropdown(dropdown) {
        dropdown.classList.remove('d-none');
    }
    
    // Hide dropdown
    function hideDropdown(dropdown) {
        dropdown.classList.add('d-none');
        activeAutocomplete = null;
    }
    
    // Public API
    return {
        init: init,
        setupAutocomplete: setupAutocomplete
    };
})();