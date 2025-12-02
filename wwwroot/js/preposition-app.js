// Global state
let prepositionsData = [];
let currentTest = null;
let testResults = [];
let currentConfig = {
    numberOfPrepositions: 10
};

// Initialize the application
document.addEventListener('DOMContentLoaded', async function () {
    await loadPrepositions();
    initializeEventListeners();
    startAutoSave();
});

// Load all prepositions from server
async function loadPrepositions() {
    try {
        const response = await fetch('/Preposition/GetPrepositions');
        prepositionsData = await response.json();
        console.log(`Loaded ${prepositionsData.length} prepositions`);
    } catch (error) {
        console.error('Error loading prepositions:', error);
        showError('Failed to load prepositions. Please refresh the page.');
    }
}

// Initialize all event listeners
function initializeEventListeners() {
    document.getElementById('startTestBtn').addEventListener('click', startTest);
    document.getElementById('viewHistoryBtn').addEventListener('click', showHistory);
    document.getElementById('viewHistoryFromTestBtn').addEventListener('click', showHistory);
    document.getElementById('backToHomeFromHistoryBtn').addEventListener('click', showHome);
    document.getElementById('backToHomeBtn').addEventListener('click', showHome);
    document.getElementById('takeAnotherTestBtn').addEventListener('click', takeAnotherTest);
    document.getElementById('testForm').addEventListener('submit', submitTest);
}

// View switching functions
function showView(viewId) {
    document.querySelectorAll('.view-container').forEach(view => {
        view.classList.add('d-none');
    });
    document.getElementById(viewId).classList.remove('d-none');
}

function showHome() {
    showView('homeView');
    document.getElementById('errorMessage').classList.add('d-none');
}

async function showHistory() {
    showView('historyView');
    await loadHistory();
}

// Generate multiple choice options for a question
function generateMultipleChoiceOptions(correctEnglish, allPrepositions) {
    // Get all other English translations
    const otherTranslations = allPrepositions
        .map(p => p.english)
        .filter(e => e.toLowerCase() !== correctEnglish.toLowerCase());

    // Shuffle and pick 3 random wrong answers
    const shuffled = otherTranslations.sort(() => 0.5 - Math.random());
    const wrongAnswers = shuffled.slice(0, 3);

    // Combine correct answer with wrong answers
    const allOptions = [correctEnglish, ...wrongAnswers];

    // Shuffle the options
    return allOptions.sort(() => 0.5 - Math.random());
}

// Start a new test
function startTest() {
    const numberOfPrepositions = parseInt(document.getElementById('numberOfPrepositions').value);

    if (prepositionsData.length === 0) {
        showError('No prepositions available.');
        return;
    }

    if (prepositionsData.length < numberOfPrepositions) {
        showError(`Only ${prepositionsData.length} prepositions available.`);
        return;
    }

    if (prepositionsData.length < 4) {
        showError('You need at least 4 prepositions in the database for multiple choice questions.');
        return;
    }

    // Save current config
    currentConfig = { numberOfPrepositions };

    // Generate random test
    const shuffled = [...prepositionsData].sort(() => 0.5 - Math.random());
    const selectedPrepositions = shuffled.slice(0, numberOfPrepositions);

    currentTest = {
        questions: selectedPrepositions.map(p => ({
            prepositionId: p.id,
            german: p.german,
            english: p.english,
            correctCase: p.case,
            explanation: p.explanation,
            options: generateMultipleChoiceOptions(p.english, prepositionsData),
            userTranslation: '',
            userCase: '',
            isCorrect: false
        })),
        score: 0,
        isCompleted: false
    };

    renderTest();
    showView('testView');
}

// Render test questions
function renderTest() {
    const container = document.getElementById('questionsContainer');

    container.innerHTML = currentTest.questions.map((q, i) => `
        <div class="card mb-3">
            <div class="card-body ${currentTest.isCompleted ? (q.isCorrect ? 'bg-success bg-opacity-25' : 'bg-danger bg-opacity-25') : ''}">
                <div class="row g-3">
                    <!-- German Preposition (Mobile: Full Width, Desktop: 3 columns) -->
                    <div class="col-12 col-md-3">
                        <h4 class="mb-2 preposition-toggle" 
                            data-german="${q.german}" 
                            data-explanation="${q.explanation || ''}"
                            data-current="german"
                            style="cursor: pointer; user-select: none;"
                            title="Double-click to see explanation">
                            ${q.german}
                        </h4>
                    </div>
                    
                    <!-- English Translation Options (Mobile: Full Width, Desktop: 5 columns) -->
                    <div class="col-12 col-md-5">
                        ${!currentTest.isCompleted ? `
                            <div class="d-grid gap-2">
                                ${q.options.map((option, optIdx) => `
                                    <div class="form-check">
                                        <input class="form-check-input translation-radio" 
                                               type="radio" 
                                               name="translation_${i}" 
                                               id="translation_${i}_${optIdx}"
                                               data-index="${i}"
                                               value="${option}"
                                               autocomplete="off">
                                        <label class="form-check-label w-100" for="translation_${i}_${optIdx}">
                                            ${option}
                                        </label>
                                    </div>
                                `).join('')}
                            </div>
                        ` : `
                            <div class="mb-2">
                                <strong>Your answer:</strong> 
                                <span class="${q.userTranslation.toLowerCase() === q.english.toLowerCase() ? 'text-success fw-bold' : 'text-danger fw-bold'}">
                                    ${q.userTranslation || 'Not selected'}
                                </span>
                            </div>
                            ${q.userTranslation.toLowerCase() !== q.english.toLowerCase() ? `
                                <div>
                                    <strong>Correct answer:</strong> 
                                    <span class="text-success fw-bold">${q.english}</span>
                                </div>
                            ` : ''}
                        `}
                    </div>
                    
                    <!-- Case Selection (Mobile: Full Width, Desktop: 4 columns) -->
                    <div class="col-12 col-md-4">
                        ${!currentTest.isCompleted ? `
                            <div class="btn-group w-100" role="group" aria-label="Case selection">
                                <input type="radio" class="btn-check case-radio" name="case_${i}" id="accusative_${i}" 
                                       data-index="${i}" value="Accusative" autocomplete="off">
                                <label class="btn btn-outline-primary" for="accusative_${i}">Accusative</label>

                                <input type="radio" class="btn-check case-radio" name="case_${i}" id="dative_${i}" 
                                       data-index="${i}" value="Dative" autocomplete="off">
                                <label class="btn btn-outline-primary" for="dative_${i}">Dative</label>
                            </div>
                        ` : `
                            <div>
                                <div class="mb-2">
                                    <strong>Your case:</strong> 
                                    <span class="badge ${q.userCase.toLowerCase() === q.correctCase.toLowerCase() || (q.correctCase.toLowerCase() === 'both' && q.userCase) ? 'bg-success' : 'bg-danger'}">
                                        ${q.userCase || 'Not selected'}
                                    </span>
                                </div>
                                ${q.userCase.toLowerCase() !== q.correctCase.toLowerCase() && !(q.correctCase.toLowerCase() === 'both' && q.userCase) ? `
                                    <div>
                                        <strong>Correct case:</strong> 
                                        <span class="badge bg-info">${q.correctCase}</span>
                                    </div>
                                ` : ''}
                            </div>
                        `}
                    </div>
                </div>
            </div>
        </div>
    `).join('');

    // Add double-click listeners for preposition toggle (show explanation)
    document.querySelectorAll('.preposition-toggle').forEach(element => {
        element.addEventListener('dblclick', function () {
            const german = this.getAttribute('data-german');
            const explanation = this.getAttribute('data-explanation');
            const current = this.getAttribute('data-current');

            if (current === 'german' && explanation) {
                this.textContent = explanation;
                this.setAttribute('data-current', 'explanation');
                this.style.fontSize = '0.9rem';
            } else {
                this.textContent = german;
                this.setAttribute('data-current', 'german');
                this.style.fontSize = '';
            }
        });
    });

    // Show/hide elements based on test state
    document.getElementById('testTitle').classList.toggle('d-none', currentTest.isCompleted);
    document.getElementById('submitButtonContainer').classList.toggle('d-none', currentTest.isCompleted);
    document.getElementById('testCompleted').classList.toggle('d-none', !currentTest.isCompleted);
}

// Submit test answers
function submitTest(e) {
    e.preventDefault();

    // Collect translation answers (from radio buttons)
    document.querySelectorAll('.translation-radio:checked').forEach(radio => {
        const index = parseInt(radio.dataset.index);
        currentTest.questions[index].userTranslation = radio.value;
    });

    // Collect case answers
    document.querySelectorAll('.case-radio:checked').forEach(radio => {
        const index = parseInt(radio.dataset.index);
        currentTest.questions[index].userCase = radio.value;
    });

    // Grade answers (case-insensitive)
    currentTest.score = 0;
    currentTest.questions.forEach(q => {
        const translationCorrect = q.userTranslation.toLowerCase() === q.english.toLowerCase();
        const caseCorrect = q.userCase.toLowerCase() === q.correctCase.toLowerCase() ||
            (q.correctCase.toLowerCase() === 'both' && (q.userCase.toLowerCase() === 'accusative' || q.userCase.toLowerCase() === 'dative'));

        q.isCorrect = translationCorrect && caseCorrect;
        if (q.isCorrect) currentTest.score++;
    });

    currentTest.isCompleted = true;

    // Add to results queue
    testResults.push({
        correctAnswers: currentTest.score,
        totalQuestions: currentTest.questions.length
    });

    // Update UI
    document.getElementById('scoreDisplay').textContent =
        `You got ${currentTest.score} / ${currentTest.questions.length} correct!`;

    renderTest();
}

// Take another test with same settings
function takeAnotherTest() {
    document.getElementById('numberOfPrepositions').value = currentConfig.numberOfPrepositions;
    startTest();
}

// Load and display history
async function loadHistory() {
    try {
        const response = await fetch('/Preposition/GetHistory');
        const history = await response.json();

        const tbody = document.getElementById('historyTableBody');
        tbody.innerHTML = history.map(item => {
            const date = new Date(item.submissionDate).toLocaleString();
            const percentage = Math.round((item.correctAnswers / item.totalQuestions) * 100);

            return `
                <tr>
                    <td>${date}</td>
                    <td>${item.correctAnswers} / ${item.totalQuestions}</td>
                    <td>${percentage}%</td>
                </tr>
            `;
        }).join('');
    } catch (error) {
        console.error('Error loading history:', error);
    }
}

// Auto-save test results every 5 minutes
function startAutoSave() {
    setInterval(async () => {
        if (testResults.length > 0) {
            await saveTestResults();
        }
    }, 5 * 60 * 1000); // 5 minutes
}

// Save test results to server
async function saveTestResults() {
    if (testResults.length === 0) return;

    try {
        const response = await fetch('/Preposition/SaveTestResults', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(testResults)
        });

        if (response.ok) {
            console.log(`Saved ${testResults.length} test results`);
            testResults = []; // Clear the queue
        }
    } catch (error) {
        console.error('Error saving test results:', error);
    }
}

// Show error message
function showError(message) {
    const errorDiv = document.getElementById('errorMessage');
    errorDiv.textContent = message;
    errorDiv.classList.remove('d-none');
}

// Save results before page unload
window.addEventListener('beforeunload', async (e) => {
    if (testResults.length > 0) {
        await saveTestResults();
    }
});