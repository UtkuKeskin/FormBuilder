// Questions Management Module
const QuestionManager = (function() {
    'use strict';
    
    const questionTypes = {
        string: { icon: 'fas fa-font', label: 'Short Text' },
        text: { icon: 'fas fa-align-left', label: 'Long Text' },
        integer: { icon: 'fas fa-hashtag', label: 'Number' },
        checkbox: { icon: 'fas fa-check-square', label: 'Checkbox' }
    };
    
    let questionCount = {
        string: 0,
        text: 0,
        integer: 0,
        checkbox: 0
    };
    
    // Initialize
    function init() {
        // Count existing questions
        countExistingQuestions();
        
        // Initialize add question buttons
        initAddQuestionButtons();
        
        // Initialize existing questions
        initExistingQuestions();
        
        // Initialize question preview
        initQuestionPreview();
    }
    
    // Count existing questions
    function countExistingQuestions() {
        Object.keys(questionTypes).forEach(type => {
            const questions = document.querySelectorAll(`[data-question-type="${type}"]`);
            questionCount[type] = questions.length;
        });
    }
    
    // Initialize add question buttons
    function initAddQuestionButtons() {
        const container = document.querySelector('#question-type-selector');
        if (!container) return;
        
        Object.entries(questionTypes).forEach(([type, config]) => {
            const button = createAddQuestionButton(type, config);
            container.appendChild(button);
        });
    }
    
    // Create add question button
    function createAddQuestionButton(type, config) {
        const button = document.createElement('button');
        button.className = 'btn btn-outline-primary m-1';
        button.innerHTML = `<i class="${config.icon}"></i> ${config.label}`;
        button.dataset.questionType = type;
        
        // Disable if max reached
        if (questionCount[type] >= 4) {
            button.disabled = true;
            button.title = `Maximum ${type} questions reached`;
        }
        
        button.addEventListener('click', () => addQuestion(type));
        
        return button;
    }
    
    // Add question
    function addQuestion(type) {
        if (questionCount[type] >= 4) {
            alert(`Maximum ${questionTypes[type].label} questions reached`);
            return;
        }
        
        const container = document.querySelector('#questions-container');
        const questionNumber = questionCount[type] + 1;
        const questionHtml = createQuestionHtml(type, questionNumber);
        
        container.insertAdjacentHTML('beforeend', questionHtml);
        
        // Initialize new question
        const newQuestion = container.lastElementChild;
        initializeQuestion(newQuestion);
        
        questionCount[type]++;
        updateAddButtons();
    }
    
    // Create question HTML
    function createQuestionHtml(type, number) {
        const typeConfig = questionTypes[type];
        const prefix = `Custom${capitalizeFirst(type)}${number}`;
        
        return `
            <div class="question-item card mb-3" data-question-type="${type}" data-question-number="${number}">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span class="question-handle">
                        <i class="fas fa-grip-vertical text-muted"></i>
                        <i class="${typeConfig.icon}"></i> ${typeConfig.label} ${number}
                    </span>
                    <button type="button" class="btn btn-sm btn-danger remove-question">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="card-body">
                    <input type="hidden" name="${prefix}State" value="true" />
                    
                    <div class="mb-3">
                        <label class="form-label">Question</label>
                        <input type="text" class="form-control question-text" 
                               name="${prefix}Question" 
                               placeholder="Enter your question" required />
                    </div>
                    
                    <div class="mb-3">
                        <label class="form-label">Description (optional)</label>
                        <input type="text" class="form-control" 
                               name="${prefix}Description" 
                               placeholder="Add helpful description" />
                    </div>
                    
                    <div class="form-check">
                        <input type="checkbox" class="form-check-input" 
                               id="${prefix}ShowInTable" 
                               name="${prefix}ShowInTable" value="true" />
                        <label class="form-check-label" for="${prefix}ShowInTable">
                            Show in forms table
                        </label>
                    </div>
                </div>
            </div>
        `;
    }
    
    // Initialize existing questions
    function initExistingQuestions() {
        const questions = document.querySelectorAll('.question-item');
        questions.forEach(initializeQuestion);
    }
    
    // Initialize question
    function initializeQuestion(question) {
        // Remove button
        const removeBtn = question.querySelector('.remove-question');
        if (removeBtn) {
            removeBtn.addEventListener('click', () => removeQuestion(question));
        }
        
        // Question text change
        const questionText = question.querySelector('.question-text');
        if (questionText) {
            questionText.addEventListener('input', updatePreview);
        }
    }
    
    // Remove question
    function removeQuestion(questionElement) {
        const type = questionElement.dataset.questionType;
        
        if (confirm('Are you sure you want to remove this question?')) {
            questionElement.remove();
            questionCount[type]--;
            updateAddButtons();
            renumberQuestions(type);
            updatePreview();
        }
    }
    
    // Renumber questions after removal
    function renumberQuestions(type) {
        const questions = document.querySelectorAll(`[data-question-type="${type}"]`);
        
        questions.forEach((question, index) => {
            const newNumber = index + 1;
            question.dataset.questionNumber = newNumber;
            
            // Update header text
            const header = question.querySelector('.question-handle');
            if (header) {
                header.innerHTML = header.innerHTML.replace(/\d+$/, newNumber);
            }
            
            // Update input names
            updateQuestionInputNames(question, type, newNumber);
        });
    }
    
    // Update question input names
    function updateQuestionInputNames(question, type, number) {
        const prefix = `Custom${capitalizeFirst(type)}${number}`;
        const inputs = question.querySelectorAll('input[name], textarea[name]');
        
        inputs.forEach(input => {
            const nameParts = input.name.match(/Custom(\w+)(\d+)(\w+)/);
            if (nameParts) {
                input.name = `${prefix}${nameParts[3]}`;
                if (input.id) {
                    input.id = `${prefix}${nameParts[3]}`;
                }
            }
        });
    }
    
    // Update add buttons state
    function updateAddButtons() {
        Object.keys(questionTypes).forEach(type => {
            const button = document.querySelector(`button[data-question-type="${type}"]`);
            if (button) {
                button.disabled = questionCount[type] >= 4;
                button.title = questionCount[type] >= 4 
                    ? `Maximum ${type} questions reached` 
                    : '';
            }
        });
    }
    
    // Initialize question preview
    function initQuestionPreview() {
        const previewContainer = document.querySelector('#question-preview');
        if (!previewContainer) return;
        
        updatePreview();
    }
    
    // Update preview
    function updatePreview() {
        const previewContainer = document.querySelector('#question-preview');
        if (!previewContainer) return;
        
        const questions = document.querySelectorAll('.question-item');
        previewContainer.innerHTML = '';
        
        questions.forEach(question => {
            const preview = createQuestionPreview(question);
            previewContainer.appendChild(preview);
        });
    }
    
    // Create question preview
    function createQuestionPreview(questionElement) {
        const type = questionElement.dataset.questionType;
        const questionText = questionElement.querySelector('.question-text')?.value || 'Untitled Question';
        const description = questionElement.querySelector('[name$="Description"]')?.value || '';
        const showInTable = questionElement.querySelector('[name$="ShowInTable"]')?.checked;
        
        const div = document.createElement('div');
        div.className = 'preview-question mb-3';
        
        let inputHtml = '';
        switch(type) {
            case 'string':
                inputHtml = '<input type="text" class="form-control" disabled />';
                break;
            case 'text':
                inputHtml = '<textarea class="form-control" rows="3" disabled></textarea>';
                break;
            case 'integer':
                inputHtml = '<input type="number" class="form-control" disabled />';
                break;
            case 'checkbox':
                inputHtml = '<input type="checkbox" class="form-check-input" disabled />';
                break;
        }
        
        div.innerHTML = `
            <label class="form-label">
                ${questionText}
                ${showInTable ? '<span class="badge bg-info ms-1">Table</span>' : ''}
            </label>
            ${description ? `<p class="text-muted small">${description}</p>` : ''}
            ${inputHtml}
        `;
        
        return div;
    }
    
    // Capitalize first letter
    function capitalizeFirst(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }
    
    // Public API
    return {
        init: init,
        addQuestion: addQuestion,
        getQuestionCount: () => questionCount
    };
})();

// Initialize when DOM is ready
if (document.querySelector('#questions-container')) {
    document.addEventListener('DOMContentLoaded', QuestionManager.init);
}