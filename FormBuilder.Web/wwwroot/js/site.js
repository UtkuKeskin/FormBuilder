// Main Application JavaScript
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
    
    // Feature modules
    if (typeof FormValidation !== 'undefined') FormValidation.init();
    if (typeof Autocomplete !== 'undefined') Autocomplete.init();
    if (typeof DragDrop !== 'undefined') DragDrop.init();
    if (typeof CloudinaryUpload !== 'undefined') CloudinaryUpload.init();
    if (typeof TableActions !== 'undefined') TableActions.init();
    if (typeof QuestionManager !== 'undefined') QuestionManager.init();
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

// Search autocomplete with Recent Searches
(function() {
    const searchInput = document.getElementById('searchInput');
    const suggestionsDiv = document.getElementById('searchSuggestions');
    let searchTimeout;
    const RECENT_SEARCHES_KEY = 'recentSearches';
    const MAX_RECENT_SEARCHES = 5;
    
    if (searchInput && suggestionsDiv) {
        // Show recent searches on focus
        searchInput.addEventListener('focus', function() {
            const query = this.value.trim();
            if (query.length < 2) {
                showRecentSearches();
            }
        });
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            const query = this.value.trim();
            
            if (query.length < 2) {
                showRecentSearches();
                return;
            }
            
            searchTimeout = setTimeout(() => {
                fetchSuggestions(query);
            }, 300);
        });
        
        // Save search on form submit
        const searchForm = searchInput.closest('form');
        if (searchForm) {
            searchForm.addEventListener('submit', function() {
                const query = searchInput.value.trim();
                if (query) {
                    saveRecentSearch(query);
                }
            });
        }
        
        // Hide suggestions on click outside
        document.addEventListener('click', function(e) {
            if (!searchInput.contains(e.target) && !suggestionsDiv.contains(e.target)) {
                suggestionsDiv.classList.remove('show');
            }
        });
    }
    
    function showRecentSearches() {
        const recentSearches = getRecentSearches();
        if (recentSearches.length === 0) return;
        
        suggestionsDiv.innerHTML = `
            <div class="dropdown-header d-flex justify-content-between align-items-center">
                <span><i class="fas fa-history"></i> Recent Searches</span>
                <small class="text-muted cursor-pointer" onclick="clearRecentSearches()">Clear</small>
            </div>
            <div class="dropdown-divider"></div>
        `;
        
        recentSearches.forEach(search => {
            const item = document.createElement('a');
            item.className = 'dropdown-item d-flex justify-content-between align-items-center';
            item.href = `/Search/Results?q=${encodeURIComponent(search)}`;
            item.innerHTML = `
                <span>${search}</span>
                <i class="fas fa-times text-muted" onclick="removeRecentSearch('${search}', event)"></i>
            `;
            suggestionsDiv.appendChild(item);
        });
        
        suggestionsDiv.classList.add('show');
    }
    
    function getRecentSearches() {
        try {
            const searches = localStorage.getItem(RECENT_SEARCHES_KEY);
            return searches ? JSON.parse(searches) : [];
        } catch (error) {
            console.error('Error loading recent searches:', error);
            return [];
        }
    }
    
    function saveRecentSearch(query) {
        try {
            let searches = getRecentSearches();
            // Remove if already exists (case-insensitive)
            searches = searches.filter(s => s.toLowerCase() !== query.toLowerCase());
            // Add to beginning
            searches.unshift(query);
            // Keep only max items
            searches = searches.slice(0, MAX_RECENT_SEARCHES);
            localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(searches));
        } catch (error) {
            console.error('Error saving recent search:', error);
        }
    }
    
    // Global functions for onclick handlers
    window.clearRecentSearches = function() {
        try {
            localStorage.removeItem(RECENT_SEARCHES_KEY);
            suggestionsDiv.classList.remove('show');
        } catch (error) {
            console.error('Error clearing recent searches:', error);
        }
    };
    
    window.removeRecentSearch = function(search, event) {
        event.preventDefault();
        event.stopPropagation();
        try {
            let searches = getRecentSearches();
            searches = searches.filter(s => s !== search);
            localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(searches));
            showRecentSearches();
        } catch (error) {
            console.error('Error removing recent search:', error);
        }
    };
    
    async function fetchSuggestions(query) {
        try {
            const response = await fetch(`/Search/Suggestions?q=${encodeURIComponent(query)}`);
            const suggestions = await response.json();
            
            displaySuggestions(suggestions, query);
        } catch (error) {
            console.error('Error fetching suggestions:', error);
            suggestionsDiv.classList.remove('show');
        }
    }
    
    function displaySuggestions(suggestions, query) {
        suggestionsDiv.innerHTML = '';
        
        // Add search header
        if (suggestions.length > 0) {
            const header = document.createElement('div');
            header.className = 'dropdown-header';
            header.innerHTML = '<i class="fas fa-search"></i> Suggestions';
            suggestionsDiv.appendChild(header);
            
            const divider = document.createElement('div');
            divider.className = 'dropdown-divider';
            suggestionsDiv.appendChild(divider);
        }
        
        suggestions.forEach(suggestion => {
            const item = document.createElement('a');
            item.className = 'dropdown-item';
            item.href = `/Search/Results?q=${encodeURIComponent(suggestion)}`;
            item.innerHTML = highlightMatch(suggestion, query);
            suggestionsDiv.appendChild(item);
        });
        
        // Add "Search for..." option
        if (query.length > 0) {
            const divider = document.createElement('div');
            divider.className = 'dropdown-divider';
            suggestionsDiv.appendChild(divider);
            
            const searchFor = document.createElement('a');
            searchFor.className = 'dropdown-item text-primary';
            searchFor.href = `/Search/Results?q=${encodeURIComponent(query)}`;
            searchFor.innerHTML = `<i class="fas fa-search"></i> Search for "<strong>${query}</strong>"`;
            suggestionsDiv.appendChild(searchFor);
        }
        
        suggestionsDiv.classList.add('show');
    }
    
    function highlightMatch(text, query) {
        const regex = new RegExp(`(${query})`, 'gi');
        return text.replace(regex, '<strong>$1</strong>');
    }
})();