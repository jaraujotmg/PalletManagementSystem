// Main Application JavaScript
var PalletManagement = (function () {
    'use strict';

    var touchModeEnabled = false;

    // Initialize the application
    function init() {
        setupTouchMode();
        setupSearchBoxes();
        setupFilterControls();
        setupConfirmationDialogs();
        setupFormValidation();
        setupPalletActions();
        setupItemActions();
        setupPrinting();
        setupDivisionPlatformSelector();
    }

    // Set up touch mode functionality
    function setupTouchMode() {
        var touchModeSwitch = document.getElementById('touchModeSwitch');
        if (touchModeSwitch) {
            addEvent(touchModeSwitch, 'change', function () {
                toggleTouchMode(this.checked);
            });

            // Initialize from current state
            toggleTouchMode(touchModeSwitch.checked);
        }
    }

    // Toggle touch mode on/off
    function toggleTouchMode(enabled) {
        touchModeEnabled = enabled;

        var inputs = document.querySelectorAll('input[type="number"], input[type="text"], select');
        var touchKeyboard = document.getElementById('touchKeyboard');

        if (enabled) {
            // Enable touch mode
            for (var i = 0; i < inputs.length; i++) {
                inputs[i].classList.add('touch-input');
            }
            if (touchKeyboard) {
                touchKeyboard.style.display = 'block';
            }
            document.body.classList.add('touch-mode');
        } else {
            // Disable touch mode
            for (var i = 0; i < inputs.length; i++) {
                inputs[i].classList.remove('touch-input');
            }
            if (touchKeyboard) {
                touchKeyboard.style.display = 'none';
            }
            document.body.classList.remove('touch-mode');
        }

        // Handle numeric keypad if present
        setupNumericKeypad();
    }

    // Setup numeric keypad functionality
    function setupNumericKeypad() {
        if (!touchModeEnabled) return;

        var numKeys = document.querySelectorAll('.num-key');
        var numericInputs = document.querySelectorAll('input[type="number"], input[type="text"].numeric');
        var activeInput = null;

        // Set focus event for numeric inputs
        for (var i = 0; i < numericInputs.length; i++) {
            addEvent(numericInputs[i], 'focus', function () {
                activeInput = this;
            });
        }

        // Handle numeric keypad clicks
        for (var i = 0; i < numKeys.length; i++) {
            addEvent(numKeys[i], 'click', function () {
                if (!activeInput) return;

                var value = this.getAttribute('data-value');

                if (value === 'del') {
                    // Delete last character
                    activeInput.value = activeInput.value.slice(0, -1);
                } else {
                    // Add character
                    activeInput.value += value;
                }

                // Trigger change event
                var event = document.createEvent('Event');
                event.initEvent('change', true, true);
                activeInput.dispatchEvent(event);
            });
        }
    }

    // Set up search boxes with autocomplete
    function setupSearchBoxes() {
        var searchInputs = document.querySelectorAll('.search-container input');

        for (var i = 0; i < searchInputs.length; i++) {
            var input = searchInputs[i];

            // Show search results on focus
            addEvent(input, 'focus', function () {
                var resultsContainer = this.closest('.search-container').querySelector('.search-results');
                if (resultsContainer) {
                    resultsContainer.classList.add('show');
                }
            });

            // Hide results when clicking outside
            addEvent(document, 'click', function (e) {
                var searchContainers = document.querySelectorAll('.search-container');
                for (var j = 0; j < searchContainers.length; j++) {
                    var container = searchContainers[j];
                    if (!container.contains(e.target)) {
                        var resultsContainer = container.querySelector('.search-results');
                        if (resultsContainer) {
                            resultsContainer.classList.remove('show');
                        }
                    }
                }
            });

            // Handle search item selection
            var searchItems = document.querySelectorAll('.search-item');
            for (var j = 0; j < searchItems.length; j++) {
                addEvent(searchItems[j], 'click', function () {
                    var value = this.innerText.trim();
                    var container = this.closest('.search-container');
                    var input = container.querySelector('input');
                    input.value = value;

                    // Hide results
                    var resultsContainer = container.querySelector('.search-results');
                    if (resultsContainer) {
                        resultsContainer.classList.remove('show');
                    }

                    // Trigger change event
                    var event = document.createEvent('Event');
                    event.initEvent('change', true, true);
                    input.dispatchEvent(event);
                });
            }

            // Setup debounced input handler for AJAX search
            var dataUrl = input.getAttribute('data-search-url');
            if (dataUrl) {
                var debouncedSearch = debounce(function () {
                    var value = this.value.trim();
                    if (value.length < 2) return;

                    // In a real implementation, this would make an AJAX call
                    // and update the search results container
                    console.log('Searching for: ' + value);
                }, 300);

                addEvent(input, 'input', debouncedSearch);
            }
        }
    }

    // Setup filter controls
    function setupFilterControls() {
        var filterButtons = document.querySelectorAll('.btn-group .btn');
        for (var i = 0; i < filterButtons.length; i++) {
            addEvent(filterButtons[i], 'click', function () {
                // Remove active class from all buttons in the group
                var buttons = this.closest('.btn-group').querySelectorAll('.btn');
                for (var j = 0; j < buttons.length; j++) {
                    buttons[j].classList.remove('active');
                }

                // Add active class to clicked button
                this.classList.add('active');

                // Additional logic for applying filters could go here
            });
        }
    }

    // Setup confirmation dialogs
    function setupConfirmationDialogs() {
        var confirmButtons = document.querySelectorAll('[data-confirm]');

        for (var i = 0; i < confirmButtons.length; i++) {
            addEvent(confirmButtons[i], 'click', function (e) {
                var message = this.getAttribute('data-confirm');
                if (!confirm(message)) {
                    e.preventDefault();
                }
            });
        }
    }

    // Setup form validation
    function setupFormValidation() {
        var forms = document.querySelectorAll('form.needs-validation');

        for (var i = 0; i < forms.length; i++) {
            addEvent(forms[i], 'submit', function (e) {
                if (!this.checkValidity()) {
                    e.preventDefault();
                    e.stopPropagation();

                    // Mark form as validated to show validation messages
                    this.classList.add('was-validated');

                    // Find the first invalid field and focus it
                    var invalidFields = this.querySelectorAll(':invalid');
                    if (invalidFields.length > 0) {
                        invalidFields[0].focus();
                    }
                }
            });

            // Track form changes
            var inputs = forms[i].querySelectorAll('input, select, textarea');
            var formChanged = false;

            for (var j = 0; j < inputs.length; j++) {
                addEvent(inputs[j], 'change', function () {
                    formChanged = true;
                });
            }

            // Check for unsaved changes when navigating away
            var cancelButton = forms[i].querySelector('[data-cancel]');
            if (cancelButton) {
                addEvent(cancelButton, 'click', function (e) {
                    if (formChanged) {
                        var confirmed = confirm('You have unsaved changes. Are you sure you want to leave?');
                        if (!confirmed) {
                            e.preventDefault();
                        }
                    }
                });
            }
        }
    }

    // Setup pallet actions (selection, move, etc.)
    function setupPalletActions() {
        // Pallet selection for move operations
        var palletCards = document.querySelectorAll('.pallet-card');
        for (var i = 0; i < palletCards.length; i++) {
            addEvent(palletCards[i], 'click', function () {
                var palletId = this.getAttribute('data-pallet-id');
                if (!palletId) return;

                // Remove selection from all pallet cards
                for (var j = 0; j < palletCards.length; j++) {
                    palletCards[j].classList.remove('selected');
                }

                // Add selection to clicked card
                this.classList.add('selected');

                // Update form input if present
                var targetPalletInput = document.getElementById('TargetPalletId');
                if (targetPalletInput) {
                    targetPalletInput.value = palletId;
                }

                // Update display in source-dest box if present
                var destDisplay = document.getElementById('destPalletDisplay');
                if (destDisplay) {
                    var palletTitle = this.querySelector('.pallet-card-title').textContent;
                    var palletSubtitle = this.querySelector('.pallet-card-subtitle').textContent;

                    destDisplay.innerHTML =
                        '<div class="font-weight-bold">' + palletTitle + '</div>' +
                        '<div class="text-muted">' + palletSubtitle + '</div>' +
                        '<div class="badge badge-orange">Open</div>';
                }

                // Enable move button if present
                var moveItemBtn = document.getElementById('moveItemBtn');
                if (moveItemBtn) {
                    moveItemBtn.disabled = false;
                }
            });
        }

        // Move item button
        var moveItemBtn = document.getElementById('moveItemBtn');
        if (moveItemBtn) {
            addEvent(moveItemBtn, 'click', function () {
                // Show confirmation modal
                var confirmModal = document.getElementById('confirmMoveModal');
                if (confirmModal) {
                    // Using jQuery for IE11 compatibility with Bootstrap modals
                    $('#confirmMoveModal').modal('show');
                }
            });
        }

        // Show platform selector when division is clicked
        var divisionButtons = document.querySelectorAll('.division-btn');
        for (var i = 0; i < divisionButtons.length; i++) {
            addEvent(divisionButtons[i], 'click', function (e) {
                e.preventDefault();
                var platformSelector = document.querySelector('.platform-selector');
                if (platformSelector) {
                    platformSelector.style.display = 'block';
                    platformSelector.scrollIntoView();
                }
            });
        }

        // Hide platform selector when "Back" is clicked
        var backButton = document.querySelector('.platform-selector .btn-secondary');
        if (backButton) {
            addEvent(backButton, 'click', function () {
                var platformSelector = document.querySelector('.platform-selector');
                if (platformSelector) {
                    platformSelector.style.display = 'none';
                }
            });
        }
    }

    // Setup item-specific actions
    function setupItemActions() {
        // Edit item fields
        var editButtons = document.querySelectorAll('.item-edit-btn');
        for (var i = 0; i < editButtons.length; i++) {
            addEvent(editButtons[i], 'click', function () {
                var itemId = this.getAttribute('data-item-id');
                if (!itemId) return;

                // In a real implementation, this would navigate to the edit page
                window.location.href = '/Items/Edit/' + itemId;
            });
        }

        // Move item buttons
        var moveButtons = document.querySelectorAll('.item-move-btn');
        for (var i = 0; i < moveButtons.length; i++) {
            addEvent(moveButtons[i], 'click', function () {
                var itemId = this.getAttribute('data-item-id');
                if (!itemId) return;

                // In a real implementation, this would navigate to the move page
                window.location.href = '/Items/Move/' + itemId;
            });
        }
    }

    // Setup printing functionality
    function setupPrinting() {
        var printButtons = document.querySelectorAll('.print-btn');
        for (var i = 0; i < printButtons.length; i++) {
            addEvent(printButtons[i], 'click', function () {
                var type = this.getAttribute('data-print-type');
                var id = this.getAttribute('data-id');

                if (type === 'pallet') {
                    // Navigate to print pallet page
                    window.location.href = '/Pallets/Print/' + id;
                } else if (type === 'item') {
                    // Navigate to print item label page
                    window.location.href = '/Items/PrintLabel/' + id;
                }
            });
        }
    }

    // Setup division/platform selector
    function setupDivisionPlatformSelector() {
        var divisionSelect = document.getElementById('divisionSelect');
        var platformSelect = document.getElementById('platformSelect');

        if (divisionSelect && platformSelect) {
            addEvent(divisionSelect, 'change', function () {
                var division = this.value;
                updatePlatformsForDivision(division, platformSelect);
            });
        }
    }

    // Update platform options based on selected division
    function updatePlatformsForDivision(division, platformSelect) {
        // Clear existing options
        while (platformSelect.firstChild) {
            platformSelect.removeChild(platformSelect.firstChild);
        }

        var platforms = [];

        // In a real implementation, this would make an AJAX call to get platforms
        // For now, hardcoded platform lists by division
        if (division === 'MA') {
            platforms = ['TEC1', 'TEC2', 'TEC4I'];
        } else if (division === 'TC') {
            platforms = ['TEC1', 'TEC3', 'TEC5'];
        }

        // Add platform options
        for (var i = 0; i < platforms.length; i++) {
            var option = document.createElement('option');
            option.value = platforms[i];
            option.textContent = platforms[i];
            platformSelect.appendChild(option);
        }

        // Trigger change event
        var event = document.createEvent('Event');
        event.initEvent('change', true, true);
        platformSelect.dispatchEvent(event);
    }

    // Public API
    return {
        init: init,
        toggleTouchMode: toggleTouchMode
    };
})();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    PalletManagement.init();
});