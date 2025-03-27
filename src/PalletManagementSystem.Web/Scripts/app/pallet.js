// Main Application JavaScript for Pallet Management System
(function () {
    'use strict';

    // Initialize the application when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        initializePalletManagement();
    });

    function initializePalletManagement() {
        setupTouchMode();
        setupFilterControls();
        setupSearchInputs();
        setupConfirmPrompts();
        setupTableSorting();
        setupAutoRefresh();
    }

    // Set up touch mode functionality
    function setupTouchMode() {
        var touchModeSwitch = document.getElementById('touchModeSwitch');
        if (touchModeSwitch) {
            touchModeSwitch.addEventListener('change', function () {
                var isEnabled = this.checked;
                toggleTouchMode(isEnabled);

                // Save preference via AJAX
                saveUserPreference('TouchModeEnabled', isEnabled);
            });

            // Initialize from current state
            toggleTouchMode(touchModeSwitch.checked);
        }
    }

    // Toggle touch mode elements
    function toggleTouchMode(enabled) {
        document.body.classList.toggle('touch-mode', enabled);

        var inputs = document.querySelectorAll('input, select, button:not(.num-key)');
        for (var i = 0; i < inputs.length; i++) {
            if (enabled) {
                inputs[i].classList.add('touch-input');
            } else {
                inputs[i].classList.remove('touch-input');
            }
        }

        // Show/hide numeric keypad
        var keypad = document.getElementById('touchKeyboard');
        if (keypad) {
            keypad.style.display = enabled ? 'block' : 'none';
        }

        // Setup numeric inputs if touch mode is enabled
        if (enabled) {
            setupNumericKeypad();
        }
    }

    // Set up numeric keypad functionality
    function setupNumericKeypad() {
        var numKeys = document.querySelectorAll('.num-key');
        var numericInputs = document.querySelectorAll('input[type="number"], input[type="text"].numeric');
        var activeInput = null;

        // Set active input on focus
        for (var i = 0; i < numericInputs.length; i++) {
            numericInputs[i].addEventListener('focus', function () {
                activeInput = this;
            });
        }

        // Handle keypad button clicks
        for (var j = 0; j < numKeys.length; j++) {
            numKeys[j].addEventListener('click', function () {
                if (!activeInput) return;

                var value = this.getAttribute('data-value');

                if (value === 'del') {
                    // Delete last character
                    activeInput.value = activeInput.value.slice(0, -1);
                } else {
                    // Add character
                    activeInput.value += value;
                }

                // Trigger change event for validation
                var event = document.createEvent('Event');
                event.initEvent('change', true, true);
                activeInput.dispatchEvent(event);
            });
        }
    }

    // Set up filter controls
    function setupFilterControls() {
        var filterButtons = document.querySelectorAll('.filter-btn');
        for (var i = 0; i < filterButtons.length; i++) {
            filterButtons[i].addEventListener('click', function () {
                var target = this.getAttribute('data-target');
                var value = this.getAttribute('data-value');

                // Update hidden input
                var input = document.getElementById(target);
                if (input) {
                    input.value = value;

                    // Submit the form
                    var form = input.closest('form');
                    if (form) form.submit();
                }

                // Update active class
                var buttons = document.querySelectorAll('.filter-btn[data-target="' + target + '"]');
                for (var j = 0; j < buttons.length; j++) {
                    buttons[j].classList.remove('active');
                }
                this.classList.add('active');
            });
        }
    }

    // Set up search inputs with autocomplete
    function setupSearchInputs() {
        var searchInputs = document.querySelectorAll('.search-input');

        for (var i = 0; i < searchInputs.length; i++) {
            var input = searchInputs[i];
            var url = input.getAttribute('data-search-url');
            if (!url) continue;

            // Debounce input for performance
            var timer;
            input.addEventListener('input', function () {
                var inputValue = this.value.trim();
                var resultsContainer = this.parentNode.querySelector('.search-results');

                // Clear previous timer
                clearTimeout(timer);

                // Don't search for short terms
                if (inputValue.length < 2) {
                    if (resultsContainer) {
                        resultsContainer.innerHTML = '';
                        resultsContainer.classList.remove('show');
                    }
                    return;
                }

                // Set a new timer
                var self = this;
                timer = setTimeout(function () {
                    // Make AJAX request
                    var xhr = new XMLHttpRequest();
                    xhr.open('GET', url + '?term=' + encodeURIComponent(inputValue), true);
                    xhr.onreadystatechange = function () {
                        if (xhr.readyState === 4 && xhr.status === 200) {
                            try {
                                var response = JSON.parse(xhr.responseText);
                                updateSearchResults(self, response, resultsContainer);
                            } catch (e) {
                                console.error('Error parsing search results:', e);
                            }
                        }
                    };
                    xhr.send();
                }, 300); // 300ms debounce time
            });

            // Show results on focus
            input.addEventListener('focus', function () {
                var resultsContainer = this.parentNode.querySelector('.search-results');
                if (resultsContainer && resultsContainer.innerHTML !== '') {
                    resultsContainer.classList.add('show');
                }
            });

            // Hide results when clicking outside
            document.addEventListener('click', function (e) {
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
        }
    }

    // Update search results container
    function updateSearchResults(inputElement, response, resultsContainer) {
        if (!resultsContainer) return;

        if (!response.success || response.results.length === 0) {
            resultsContainer.innerHTML = '<div class="search-item">No results found</div>';
        } else {
            var html = '';
            for (var i = 0; i < response.results.length; i++) {
                var result = response.results[i];
                html += '<div class="search-item" data-id="' + result.id + '" data-url="' + result.url + '">';
                html += '<strong>' + result.text + '</strong>';
                if (result.info) {
                    html += '<br><small class="text-muted">' + result.info + '</small>';
                }
                html += '</div>';
            }
            resultsContainer.innerHTML = html;

            // Add click handlers to search items
            var searchItems = resultsContainer.querySelectorAll('.search-item');
            for (var j = 0; j < searchItems.length; j++) {
                searchItems[j].addEventListener('click', function () {
                    var url = this.getAttribute('data-url');
                    if (url) {
                        window.location.href = url;
                    }
                });
            }
        }

        resultsContainer.classList.add('show');
    }

    // Set up confirmation prompts
    function setupConfirmPrompts() {
        var confirmButtons = document.querySelectorAll('[data-confirm]');

        for (var i = 0; i < confirmButtons.length; i++) {
            confirmButtons[i].addEventListener('click', function (e) {
                var message = this.getAttribute('data-confirm');
                if (!confirm(message)) {
                    e.preventDefault();
                }
            });
        }
    }

    // Set up table sorting
    function setupTableSorting() {
        var sortHeaders = document.querySelectorAll('th[data-sort]');

        for (var i = 0; i < sortHeaders.length; i++) {
            sortHeaders[i].addEventListener('click', function () {
                var sortBy = this.getAttribute('data-sort');
                var currentSort = getUrlParameter('sortBy');
                var currentDirection = getUrlParameter('sortDescending');

                // Toggle sort direction if same column
                var sortDescending = sortBy === currentSort ?
                    (currentDirection === 'true' ? 'false' : 'true') : 'false';

                // Redirect with new sort parameters
                var url = updateQueryStringParameter(window.location.href, 'sortBy', sortBy);
                url = updateQueryStringParameter(url, 'sortDescending', sortDescending);
                window.location.href = url;
            });
        }
    }

    // Set up auto-refresh for pallet lists
    function setupAutoRefresh() {
        var autoRefreshEnabled = document.getElementById('autoRefreshEnabled');
        var refreshInterval = document.getElementById('refreshInterval');

        if (autoRefreshEnabled && autoRefreshEnabled.value === 'true' && refreshInterval) {
            var interval = parseInt(refreshInterval.value, 10) * 1000; // Convert to milliseconds

            if (!isNaN(interval) && interval > 0) {
                setInterval(function () {
                    // Reload the page without POST data
                    window.location.reload();
                }, interval);
            }
        }
    }

    // Helper: Get URL parameter
    function getUrlParameter(name) {
        name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
        var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
        var results = regex.exec(location.search);
        return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
    }

    // Helper: Update query string parameter
    function updateQueryStringParameter(uri, key, value) {
        var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
        var separator = uri.indexOf('?') !== -1 ? "&" : "?";

        if (uri.match(re)) {
            return uri.replace(re, '$1' + key + "=" + value + '$2');
        } else {
            return uri + separator + key + "=" + value;
        }
    }

    // Helper: Save user preference via AJAX
    function saveUserPreference(key, value) {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', '/Settings/SavePreference', true);
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        // Get the anti-forgery token
        var token = document.querySelector('input[name="__RequestVerificationToken"]');
        var tokenValue = token ? token.value : '';

        xhr.send('key=' + encodeURIComponent(key) +
            '&value=' + encodeURIComponent(value) +
            '&__RequestVerificationToken=' + encodeURIComponent(tokenValue));
    }
})();