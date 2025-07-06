// Tag Input Module
const TagInput = (function() {
    'use strict';
    
    let tagInputs = [];
    
    function init() {
        console.log('TagInput module initialized');
        
        // Find all tag inputs
        const inputs = document.querySelectorAll('[data-tag-input]');
        inputs.forEach(input => {
            initializeTagInput(input);
        });
    }
    
    function initializeTagInput(input) {
        if (!window.jQuery || !jQuery.fn.select2) {
            console.error('Select2 not loaded');
            return;
        }
        
        const $input = jQuery(input);
        
        $input.select2({
            tags: true,
            tokenSeparators: [',', ' '],
            placeholder: 'Enter tags...',
            minimumInputLength: 2,
            maximumSelectionLength: 10,
            ajax: {
                url: '/api/tags/suggestions',
                dataType: 'json',
                delay: 250,
                data: function(params) {
                    return {
                        q: params.term
                    };
                },
                processResults: function(data) {
                    return {
                        results: data.map(tag => ({
                            id: tag,
                            text: tag
                        }))
                    };
                }
            },
            createTag: function(params) {
                var term = params.term.trim().toLowerCase();
                
                if (term === '') {
                    return null;
                }
                
                return {
                    id: term,
                    text: term,
                    newTag: true
                };
            },
            templateResult: function(tag) {
                if (tag.newTag) {
                    return jQuery('<span>').text(tag.text + ' (new)');
                }
                return tag.text;
            }
        });
        
        // Initialize with existing values
        const existingTags = input.dataset.existingTags;
        if (existingTags) {
            const tags = existingTags.split(',').filter(t => t.trim());
            $input.val(tags).trigger('change');
        }
        
        tagInputs.push($input);
    }
    
    function destroy() {
        tagInputs.forEach($input => {
            if ($input.data('select2')) {
                $input.select2('destroy');
            }
        });
        tagInputs = [];
    }
    
    return {
        init: init,
        destroy: destroy
    };
})();