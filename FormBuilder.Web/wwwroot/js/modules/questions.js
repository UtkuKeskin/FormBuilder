// Questions Management Module - Enhanced Version
const QuestionManager = (function() {
    'use strict';
    
    const MAX_QUESTIONS_PER_TYPE = 4;
    const questionTypes = {
        string: { icon: 'fas fa-font', label: 'Short Text', color: 'primary' },
        text: { icon: 'fas fa-align-left', label: 'Long Text', color: 'info' },
        integer: { icon: 'fas fa-hashtag', label: 'Number', color: 'success' },
        checkbox: { icon: 'fas fa-check-square', label: 'Checkbox', color: 'warning' }
    };
    
    let questionCount = {
        string: 0,
        text: 0,
        integer: 0,
        checkbox: 0
    };
    
    // Initialize
    function init() {
        countExistingQuestions();
        initQuestionButtons();
        initExistingQuestions();
        updateButtonStates();
        updateEmptyMessage();
        updatePreview();
    }
    
    // Count existing questions
    function countExistingQuestions() {
        Object.keys(questionTypes).forEach(type => {
            const questions = document.querySelectorAll(`.question-item[data-question-type="${type}"]`);
            questionCount[type] = questions.length;
        });
    }
    
    // Initialize question type buttons
    function initQuestionButtons() {
        const buttons = document.querySelectorAll('.question-type-btn[data-question-type]');
        
        buttons.forEach(button => {
            const type = button.dataset.questionType;
            
            // Remove existing listeners
            const newButton = button.cloneNode(true);
            button.parentNode.replaceChild(newButton, button);
            
            newButton.addEventListener('click', (e) => {
                e.preventDefault();
                addQuestion(type);
            });
        });
    }
    
    // Initialize existing questions
    function initExistingQuestions() {
        const questions = document.querySelectorAll('.question-item');
        questions.forEach(initializeQuestion);
    }
    
    // Initialize single question
    function initializeQuestion(questionElement) {
        // Remove button
        const removeBtn = questionElement.querySelector('.remove-question');
        if (removeBtn) {
            removeBtn.addEventListener('click', () => removeQuestion(questionElement));
        }
        
        // Input changes for preview update
        const inputs = questionElement.querySelectorAll('input[type="text"], input[type="checkbox"]');
        inputs.forEach(input => {
            input.addEventListener('input', debounce(updatePreview, 300));
            input.addEventListener('change', updatePreview);
        });
    }
    
    // Add new question
    function addQuestion(type) {
        if (questionCount[type] >= MAX_QUESTIONS_PER_TYPE) {
            showNotification(`Maximum ${questionTypes[type].label} questions reached`, 'warning');
            return;
        }
        
        const container = document.getElementById('questions-container');
        const emptyMessage = document.getElementById('no-questions-message');
        
        // Hide empty message
        if (emptyMessage) {
            emptyMessage.style.display = 'none';
        }
        
        // Find next available number
        const usedNumbers = getUsedNumbers(type);
        let nextNumber = 1;
        while (usedNumbers.includes(nextNumber) && nextNumber <= MAX_QUESTIONS_PER_TYPE) {
            nextNumber++;
        }
        
        const questionHtml = createQuestionHtml(type, nextNumber);
        container.insertAdjacentHTML('beforeend', questionHtml);
        
        // Initialize the new question
        const newQuestion = container.lastElementChild;
        initializeQuestion(newQuestion);
        
        // Update state
        questionCount[type]++;
        updateButtonStates();
        updatePreview();
        
        // Reinitialize drag & drop
        if (typeof DragDrop !== 'undefined') {
            DragDrop.reinitialize();
        }
        
        // Focus on question text
        const questionInput = newQuestion.querySelector('.question-text');
        if (questionInput) {
            questionInput.focus();
        }
    }
    
    // Get used question numbers for a type
    function getUsedNumbers(type) {
        const questions = document.querySelectorAll(`.question-item[data-question-type="${type}"]`);
        return Array.from(questions).map(q => parseInt(q.dataset.questionNumber));
    }
    
    // Create question HTML
    function createQuestionHtml(type, number) {
        const config = questionTypes[type];
        const typeCapitalized = type.charAt(0).toUpperCase() + type.slice(1);
        const prefix = `Custom${typeCapitalized}${number}`;
        
        return `
            <div class="question-item card mb-3 border-${config.color}" 
                 data-question-type="${type}" 
                 data-question-number="${number}">
                <div class="card-header bg-${config.color} bg-opacity-10 d-flex justify-content-between align-items-center">
                    <span class="question-handle" style="cursor: move;">
                        <i class="fas fa-grip-vertical text-muted me-2"></i>
                        <i class="${config.icon} text-${config.color}"></i> 
                        <span class="fw-bold">${config.label} #${number}</span>
                    </span>
                    <button type="button" class="btn btn-sm btn-danger remove-question" title="Remove question">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
                <div class="card-body">
                    <input type="hidden" name="${prefix}State" value="true" />
                    
                    <div class="mb-3">
                        <label class="form-label fw-bold">
                            <i class="fas fa-question-circle text-muted"></i> Question Text
                        </label>
                        <input type="text" 
                               class="form-control question-text" 
                               name="${prefix}Question" 
                               placeholder="Enter your question here..." 
                               required />
                        <div class="invalid-feedback">Question text is required</div>
                    </div>
                    
                    <div class="mb-3">
                        <label class="form-label">
                            <i class="fas fa-info-circle text-muted"></i> Description
                            <small class="text-muted">(optional)</small>
                        </label>
                        <input type="text" 
                               class="form-control question-description" 
                               name="${prefix}Description" 
                               placeholder="Add helpful instructions or context..." />
                    </div>
                    
                    <div class="form-check form-switch">
                        <input type="checkbox" 
                               class="form-check-input show-in-table-checkbox" 
                               id="${prefix}ShowInTable" 
                               name="${prefix}ShowInTable" 
                               value="true" />
                        <label class="form-check-label" for="${prefix}ShowInTable">
                            <i class="fas fa-table text-muted"></i> 
                            Display this answer in the forms list table
                        </label>
                    </div>
                </div>
            </div>
        `;
    }
    
    // Remove question
    function removeQuestion(questionElement) {
        if (!confirm('Are you sure you want to remove this question? This cannot be undone.')) {
            return;
        }
        
        const type = questionElement.dataset.questionType;
        
        // Add fade out animation
        questionElement.style.transition = 'opacity 0.3s';
        questionElement.style.opacity = '0';
        
        setTimeout(() => {
            questionElement.remove();
            questionCount[type]--;
            updateButtonStates();
            updatePreview();
            updateEmptyMessage();
            
            // Reinitialize drag & drop
            if (typeof DragDrop !== 'undefined') {
                DragDrop.reinitialize();
            }
        }, 300);
    }
    
    // Update button states
    function updateButtonStates() {
        Object.keys(questionTypes).forEach(type => {
            const button = document.querySelector(`.question-type-btn[data-question-type="${type}"]`);
            if (!button) return;
            
            const count = questionCount[type];
            const countSpan = button.querySelector('.question-count');
            
            if (countSpan) {
                countSpan.textContent = `(${count}/${MAX_QUESTIONS_PER_TYPE})`;
            }
            
            button.disabled = count >= MAX_QUESTIONS_PER_TYPE;
            
            if (count >= MAX_QUESTIONS_PER_TYPE) {
                button.classList.add('disabled');
                button.title = `Maximum ${questionTypes[type].label} questions reached`;
            } else {
                button.classList.remove('disabled');
                button.title = `Add ${questionTypes[type].label} question`;
            }
        });
    }
    
    // Update empty message
    function updateEmptyMessage() {
        const totalQuestions = Object.values(questionCount).reduce((sum, count) => sum + count, 0);
        const emptyMessage = document.getElementById('no-questions-message');
        
        if (emptyMessage) {
            emptyMessage.style.display = totalQuestions === 0 ? 'block' : 'none';
        }
    }
    
    // Update preview
    function updatePreview() {
        const previewContainer = document.getElementById('question-preview');
        if (!previewContainer) return;
        
        const questions = document.querySelectorAll('.question-item');
        
        if (questions.length === 0) {
            previewContainer.innerHTML = `
                <div class="text-center py-5 text-muted">
                    <i class="fas fa-eye-slash fa-3x mb-3"></i>
                    <p>Add questions to see the form preview</p>
                </div>
            `;
            return;
        }
        
        previewContainer.innerHTML = '';
        
        questions.forEach((question, index) => {
            const preview = createQuestionPreview(question, index + 1);
            previewContainer.appendChild(preview);
        });
    }
    
    // Create question preview element
    function createQuestionPreview(questionElement, orderNumber) {
        const type = questionElement.dataset.questionType;
        const questionText = questionElement.querySelector('.question-text')?.value || 'Untitled Question';
        const description = questionElement.querySelector('.question-description')?.value || '';
        const showInTable = questionElement.querySelector('.show-in-table-checkbox')?.checked;
        
        const div = document.createElement('div');
        div.className = 'preview-question mb-4 p-3 border rounded bg-light';
        
        let inputHtml = '';
        switch(type) {
            case 'string':
                inputHtml = '<input type="text" class="form-control" placeholder="Short answer text" disabled />';
                break;
            case 'text':
                inputHtml = '<textarea class="form-control" rows="3" placeholder="Long answer text" disabled></textarea>';
                break;
            case 'integer':
                inputHtml = '<input type="number" class="form-control" placeholder="Number" disabled />';
                break;
            case 'checkbox':
                inputHtml = `
                    <div class="form-check">
                        <input type="checkbox" class="form-check-input" disabled />
                        <label class="form-check-label">Check to confirm</label>
                    </div>
                `;
                break;
        }
        
        div.innerHTML = `
            <div class="d-flex justify-content-between align-items-start mb-2">
                <div class="flex-grow-1">
                    <h6 class="mb-1">
                        <span class="badge bg-secondary me-2">${orderNumber}</span>
                        ${questionText}
                    </h6>
                    ${description ? `<p class="text-muted small mb-2">${description}</p>` : ''}
                </div>
                ${showInTable ? '<span class="badge bg-info" title="Shown in forms table"><i class="fas fa-table"></i></span>' : ''}
            </div>
            ${inputHtml}
        `;
        
        return div;
    }
    
    // Handle order change from drag & drop
    function handleOrderChange() {
        updatePreview();
        showNotification('Question order updated', 'success');
    }
    
    // Utility: Debounce function
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
    
    // Show notification (simple implementation)
    function showNotification(message, type = 'info') {
        console.log(`[${type.toUpperCase()}] ${message}`);
        // TODO: Implement toast notifications
    }
    
    // Public API
    return {
        init: init,
        addQuestion: addQuestion,
        updatePreview: updatePreview,
        handleOrderChange: handleOrderChange
    };
})();