// Global state
let verbsData = [];
let selectedVerbsData = []; // Verbs from VerbSelection table
let currentTest = null;
let testResults = [];
let currentConfig = {
    numberOfVerbs: 10,
    selectedCategories: []
};

// Initialize the application
document.addEventListener('DOMContentLoaded', async function () {
    await loadVerbs();
    await loadSelectedVerbs();
    initializeEventListeners();
    populateCategories();
    startAutoSave();
});

// Load all verbs from server
async function loadVerbs() {
    try {
        const response = await fetch('/Test/GetVerbs');
        verbsData = await response.json();
        console.log(`Loaded ${verbsData.length} verbs`);
    } catch (error) {
        console.error('Error loading verbs:', error);
        showError('Failed to load verbs. Please refresh the page.');
    }
}

// Load selected verbs from VerbSelection table
async function loadSelectedVerbs() {
    try {
        const response = await fetch('/Test/GetSelectedVerbs');
        selectedVerbsData = await response.json();
        console.log(`Loaded ${selectedVerbsData.length} selected verbs`);
    } catch (error) {
        console.error('Error loading selected verbs:', error);
        selectedVerbsData = [];
    }
}

// Populate category checkboxes
function populateCategories() {
    const categories = ['A1', 'A2', 'B1', 'B2', 'C1', 'C2'];
    const container = document.getElementById('categoryContainer');

    // Add regular categories
    let html = categories.map(cat => `
        <div class="col-6 col-md-4">
            <div class="form-check">
                <input class="form-check-input category-checkbox" 
                       type="checkbox" 
                       value="${cat}" 
                       id="category_${cat}">
                <label class="form-check-label" for="category_${cat}">
                    <span class="badge bg-primary">${cat}</span>
                </label>
            </div>
        </div>
    `).join('');

    // Add Manual option
    html += `
        <div class="col-6 col-md-4">
            <div class="form-check">
                <input class="form-check-input category-checkbox" 
                       type="checkbox" 
                       value="Manual" 
                       id="category_Manual">
                <label class="form-check-label" for="category_Manual">
                    <span class="badge bg-warning text-dark">Manual</span>
                    <small class="text-muted d-block">(${selectedVerbsData.length} verbs)</small>
                </label>
            </div>
        </div>
    `;

    container.innerHTML = html;
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

// Start a new test
function startTest() {
    const numberOfVerbs = parseInt(document.getElementById('numberOfVerbs').value);
    const selectedCategories = Array.from(document.querySelectorAll('.category-checkbox:checked'))
        .map(cb => cb.value);

    if (selectedCategories.length === 0) {
        showError('Please select at least one category.');
        return;
    }

    const includeManual = selectedCategories.includes('Manual');
    const regularCategories = selectedCategories.filter(c => c !== 'Manual');

    // Get verbs from regular categories
    let availableVerbs = [];
    if (regularCategories.length > 0) {
        availableVerbs = verbsData.filter(v => regularCategories.includes(v.category));
    }

    // Get verbs from Manual selection (VerbSelection table)
    let manualVerbs = [];
    if (includeManual && selectedVerbsData.length > 0) {
        manualVerbs = [...selectedVerbsData];
    }

    // If only Manual is selected
    if (regularCategories.length === 0 && includeManual) {
        if (manualVerbs.length === 0) {
            showError('No verbs in Manual selection. Please add verbs in Manage Verbs.');
            return;
        }

        // Shuffle and take requested number from manual verbs
        const shuffled = manualVerbs.sort(() => 0.5 - Math.random());
        const selectedVerbs = shuffled.slice(0, Math.min(numberOfVerbs, manualVerbs.length));

        currentConfig = { numberOfVerbs, selectedCategories };
        createTest(selectedVerbs);
        return;
    }

    // If Manual is selected along with other categories
    if (includeManual && regularCategories.length > 0) {
        // Get IDs of manual verbs to exclude duplicates
        const manualVerbIds = new Set(manualVerbs.map(v => v.id));

        // Filter out verbs that are already in manual selection
        const categoryVerbsWithoutDuplicates = availableVerbs.filter(v => !manualVerbIds.has(v.id));

        // Calculate how many additional verbs we need from categories
        const verbsNeededFromCategories = Math.max(0, numberOfVerbs - manualVerbs.length);

        // Shuffle category verbs and take needed amount
        const shuffledCategoryVerbs = categoryVerbsWithoutDuplicates.sort(() => 0.5 - Math.random());
        const additionalVerbs = shuffledCategoryVerbs.slice(0, verbsNeededFromCategories);

        // Combine manual verbs with additional category verbs
        const combinedVerbs = [...manualVerbs, ...additionalVerbs];

        if (combinedVerbs.length === 0) {
            showError('No verbs found for the selected options.');
            return;
        }

        // Shuffle the combined list
        const finalVerbs = combinedVerbs.sort(() => 0.5 - Math.random());

        currentConfig = { numberOfVerbs, selectedCategories };
        createTest(finalVerbs);
        return;
    }

    // Regular categories only (no Manual)
    if (availableVerbs.length === 0) {
        showError('No verbs found for the selected categories.');
        return;
    }

    if (availableVerbs.length < numberOfVerbs) {
        showError(`Only ${availableVerbs.length} verbs available for selected categories.`);
        return;
    }

    // Save current config
    currentConfig = { numberOfVerbs, selectedCategories };

    // Generate random test
    const shuffled = availableVerbs.sort(() => 0.5 - Math.random());
    const selectedVerbs = shuffled.slice(0, numberOfVerbs);

    createTest(selectedVerbs);
}

// Create test from selected verbs
function createTest(selectedVerbs) {
    currentTest = {
        questions: selectedVerbs.map(v => ({
            verbId: v.id,
            german: v.german,
            english: v.english,
            hint: v.hint,
            userAnswer: '',
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
                <div class="row align-items-center g-3">
                    <div class="col-12 col-md-3 text-center text-md-start">
                        <h4 class="mb-0 verb-toggle" 
                            data-german="${q.german}" 
                            data-hint="${q.hint || ''}"
                            data-current="german"
                            style="cursor: pointer; user-select: none;"
                            title="${q.hint ? 'Double-click to see hint' : 'No hint available'}">
                            ${q.german}
                        </h4>
                    </div>
                    <div class="col-12 col-md-5">
                        ${!currentTest.isCompleted ? `
                            <input type="text" 
                                   class="form-control answer-input" 
                                   data-index="${i}"
                                   placeholder="English translation..."
                                   autocomplete="off"
                                   autocorrect="off"
                                   autocapitalize="off"
                                   spellcheck="false"
                                   required>
                        ` : `
                            <input type="text" 
                                   class="form-control" 
                                   value="${q.userAnswer}" 
                                   disabled>
                        `}
                    </div>
                    ${currentTest.isCompleted ? `
                        <div class="col-12 col-md-4 text-center text-md-start">
                            ${!q.isCorrect ? `
                                <span class="fw-bold text-danger">Correct: ${q.english}</span>
                            ` : `
                                <span class="fw-bold text-success">Correct!</span>
                            `}
                        </div>
                    ` : ''}
                </div>
            </div>
        </div>
    `).join('');

    // Add double-click listeners for verb toggle (show hint instead of English)
    document.querySelectorAll('.verb-toggle').forEach(element => {
        element.addEventListener('dblclick', function () {
            const german = this.getAttribute('data-german');
            const hint = this.getAttribute('data-hint');
            const current = this.getAttribute('data-current');

            if (current === 'german' && hint) {
                this.textContent = hint;
                this.setAttribute('data-current', 'hint');
            } else {
                this.textContent = german;
                this.setAttribute('data-current', 'german');
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

    // Collect answers
    document.querySelectorAll('.answer-input').forEach(input => {
        const index = parseInt(input.dataset.index);
        currentTest.questions[index].userAnswer = input.value.trim();
    });

    // Grade answers
    currentTest.score = 0;
    currentTest.questions.forEach(q => {
        const userAnswer = q.userAnswer.toLowerCase().trim();
        const correctAnswer = q.english.toLowerCase().trim();
        q.isCorrect = userAnswer === correctAnswer;
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
    document.getElementById('numberOfVerbs').value = currentConfig.numberOfVerbs;

    // Restore category selections
    document.querySelectorAll('.category-checkbox').forEach(cb => {
        cb.checked = currentConfig.selectedCategories.includes(cb.value);
    });

    startTest();
}

// Load and display history
async function loadHistory() {
    try {
        const response = await fetch('/Test/GetHistory');
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
        const response = await fetch('/Test/SaveTestResults', {
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