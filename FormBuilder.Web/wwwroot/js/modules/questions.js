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
    
    let initialized = false;
    
    // Initialize
    function init() {
        // Prevent multiple initialization
        if (initialized) return;
        initialized = true;
        
        // Count existing questions
        countExistingQuestions();
        
        // Initialize existing buttons (don't create new ones)
        initExistingButtons();
        
        // Initialize existing questions
        initExistingQuestions();
        
        // Initialize question preview
        initQuestionPreview();
        
        // Update button states and counts
        updateButtonStates();
        
        // Hide empty message if questions exist
        updateEmptyMessage();
    }
    
    // Count existing questions
    function countExistingQuestions() {
        // Reset counts
        Object.keys(questionCount).forEach(type => {
            questionCount[type] = 0;
        });
        
        // Count actual questions
        Object.keys(questionTypes).forEach(type => {
            const questions = document.querySelectorAll(`.question-item[data-question-type="${type}"]`);
            questionCount[type] = questions.length;
        });
    }
    
    // Initialize existing buttons in HTML
    function initExistingButtons() {
        const buttons = document.querySelectorAll('.question-type-btn[data-question-type]');
        buttons.forEach(button => {
            // Remove any existing listeners first
            const newButton = button.cloneNode(true);
            button.parentNode.replaceChild(newButton, button);
            
            const type = newButton.dataset.questionType;
            newButton.addEventListener('click', (e) => {
                e.preventDefault();
                addQuestion(type);
            });
        });
    }
    
    // Add question
    function addQuestion(type) {
        // Re-count to ensure accuracy
        countExistingQuestions();
        
        if (questionCount[type] >= 4) {
            alert(`Maximum ${questionTypes[type].label} questions reached`);
            return;
        }
        
        const container = document.querySelector('#questions-container');
        const emptyMessage = document.querySelector('#no-questions-message');
        
        // Hide empty message
        if (emptyMessage) {
            emptyMessage.style.display = 'none';
        }
        
        // Find the highest number for this type
        const existingNumbers = [];
        document.querySelectorAll(`.question-item[data-question-type="${type}"]`).forEach(q => {
            const num = parseInt(q.dataset.questionNumber);
            if (!isNaN(num)) existingNumbers.push(num);
        });
        
        const nextNumber = existingNumbers.length > 0 ? Math.max(...existingNumbers) + 1 : 1;
        const questionHtml = createQuestionHtml(type, nextNumber);
        
        // Insert before empty message or at the end
        if (emptyMessage) {
            emptyMessage.insertAdjacentHTML('beforebegin', questionHtml);
        } else {
            container.insertAdjacentHTML('beforeend', questionHtml);
        }
        
        // Initialize new question - find it specifically
        const allQuestions = container.querySelectorAll('.question-item');
        const newQuestion = allQuestions[allQuestions.length - 1];
        initializeQuestion(newQuestion);
        
        // Update count and UI
        questionCount[type]++;
        updateButtonStates();
        updatePreview();
    }
    
    // Create question HTML
    function createQuestionHtml(type, number) {
        const typeConfig = questionTypes[type];
        const typeCapitalized = type.charAt(0).toUpperCase() + type.slice(1);
        const prefix = `Custom${typeCapitalized}${number}`;
        
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
        // Remove button - clear existing listeners first
        const removeBtn = question.querySelector('.remove-question');
        if (removeBtn) {
            const newRemoveBtn = removeBtn.cloneNode(true);
            removeBtn.parentNode.replaceChild(newRemoveBtn, removeBtn);
            newRemoveBtn.addEventListener('click', () => removeQuestion(question));
        }
        
        // Question text change
        const questionText = question.querySelector('.question-text');
        if (questionText && !questionText.hasAttribute('data-initialized')) {
            questionText.setAttribute('data-initialized', 'true');
            questionText.addEventListener('input', updatePreview);
        }
    }
    
    // Remove question
    function removeQuestion(questionElement) {
        const type = questionElement.dataset.questionType;
        
        if (confirm('Are you sure you want to remove this question?')) {
            questionElement.remove();
            countExistingQuestions();
            updateButtonStates();
            renumberQuestions(type);
            updatePreview();
            updateEmptyMessage();
        }
    }
    
    // Renumber questions after removal
    function renumberQuestions(type) {
        const questions = document.querySelectorAll(`.question-item[data-question-type="${type}"]`);
        
        questions.forEach((question, index) => {
            const newNumber = index + 1;
            const oldNumber = question.dataset.questionNumber;
            question.dataset.questionNumber = newNumber;
            
            // Update header text
            const header = question.querySelector('.question-handle');
            if (header) {
                const typeConfig = questionTypes[type];
                header.innerHTML = `
                    <i class="fas fa-grip-vertical text-muted"></i>
                    <i class="${typeConfig.icon}"></i> ${typeConfig.label} ${newNumber}
                `;
            }
            
            // Update input names only if number changed
            if (oldNumber != newNumber) {
                updateQuestionInputNames(question, type, newNumber);
            }
        });
    }
    
    // Update question input names
    function updateQuestionInputNames(question, type, number) {
        const typeCapitalized = type.charAt(0).toUpperCase() + type.slice(1);
        const prefix = `Custom${typeCapitalized}${number}`;
        const inputs = question.querySelectorAll('input[name], textarea[name]');
        
        inputs.forEach(input => {
            const nameParts = input.name.match(/Custom(\w+)(\d+)(\w+)/);
            if (nameParts) {
                const oldName = input.name;
                const newName = `${prefix}${nameParts[3]}`;
                input.name = newName;
                
                if (input.id) {
                    const oldId = input.id;
                    const newId = `${prefix}${nameParts[3]}`;
                    input.id = newId;
                    
                    // Update label if exists
                    const label = question.querySelector(`label[for="${oldId}"]`);
                    if (label) {
                        label.setAttribute('for', newId);
                    }
                }
            }
        });
    }
    
    // Update button states and counts
    function updateButtonStates() {
        Object.keys(questionTypes).forEach(type => {
            const button = document.querySelector(`.question-type-btn[data-question-type="${type}"]`);
            if (button) {
                const count = questionCount[type];
                const countSpan = button.querySelector('.question-count');
                
                // Update count display
                if (countSpan) {
                    countSpan.textContent = `(${count}/4)`;
                }
                
                // Update disabled state
                button.disabled = count >= 4;
                if (count >= 4) {
                    button.classList.add('disabled');
                    button.title = `Maximum ${questionTypes[type].label} questions reached`;
                } else {
                    button.classList.remove('disabled');
                    button.title = '';
                }
            }
        });
    }
    
    // Update empty message visibility
    function updateEmptyMessage() {
        const totalQuestions = Object.values(questionCount).reduce((sum, count) => sum + count, 0);
        const emptyMessage = document.querySelector('#no-questions-message');
        
        if (emptyMessage) {
            emptyMessage.style.display = totalQuestions === 0 ? 'block' : 'none';
        }
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
        
        if (questions.length === 0) {
            previewContainer.innerHTML = '<p class="text-muted text-center">Add questions to see how your form will look</p>';
            return;
        }
        
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
                inputHtml = '<input type="text" class="form-control" placeholder="Your answer..." disabled />';
                break;
            case 'text':
                inputHtml = '<textarea class="form-control" rows="3" placeholder="Your answer..." disabled></textarea>';
                break;
            case 'integer':
                inputHtml = '<input type="number" class="form-control" placeholder="0" disabled />';
                break;
            case 'checkbox':
                inputHtml = '<div class="form-check"><input type="checkbox" class="form-check-input" disabled /><label class="form-check-label">Check to confirm</label></div>';
                break;
        }
        
        div.innerHTML = `
            <label class="form-label fw-bold">
                ${questionText}
                ${showInTable ? '<span class="badge bg-info ms-1">Shows in table</span>' : ''}
            </label>
            ${description ? `<p class="text-muted small mb-2">${description}</p>` : ''}
            ${inputHtml}
        `;
        
        return div;
    }
    
    // Public API
    return {
        init: init,
        addQuestion: addQuestion,
        getQuestionCount: () => questionCount,
        updatePreview: updatePreview
    };
})();