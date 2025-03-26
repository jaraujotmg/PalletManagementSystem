// Modal Helper Functions for IE11 Compatibility
(function () {
    'use strict';

    // Show a modal dialog
    window.showModal = function (modalId) {
        var modal = document.getElementById(modalId);
        if (!modal) return;

        // Using jQuery for IE11 compatibility
        $('#' + modalId).modal('show');
    };

    // Hide a modal dialog
    window.hideModal = function (modalId) {
        var modal = document.getElementById(modalId);
        if (!modal) return;

        // Using jQuery for IE11 compatibility
        $('#' + modalId).modal('hide');
    };

    // Setup modal event handlers
    window.setupModalHandlers = function () {
        // Confirm action buttons in modals
        var confirmButtons = document.querySelectorAll('[data-confirm-action]');
        for (var i = 0; i < confirmButtons.length; i++) {
            var button = confirmButtons[i];
            addEvent(button, 'click', function () {
                var action = this.getAttribute('data-confirm-action');
                var modalId = this.closest('.modal').id;

                // Hide modal
                hideModal(modalId);

                // Execute action if specified
                if (action === 'submit-form') {
                    var formId = this.getAttribute('data-form-id');
                    if (formId) {
                        var form = document.getElementById(formId);
                        if (form) {
                            form.submit();
                        }
                    }
                } else if (action === 'navigate') {
                    var url = this.getAttribute('data-url');
                    if (url) {
                        window.location.href = url;
                    }
                } else if (action === 'callback') {
                    var callback = this.getAttribute('data-callback');
                    if (callback && window[callback]) {
                        window[callback]();
                    }
                }
            });
        }

        // Cancel buttons in modals
        var cancelButtons = document.querySelectorAll('[data-dismiss="modal"]');
        for (var i = 0; i < cancelButtons.length; i++) {
            var button = cancelButtons[i];
            addEvent(button, 'click', function () {
                var modalId = this.closest('.modal').id;
                hideModal(modalId);
            });
        }
    };

    // Add event handler to initialize modal handlers when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        setupModalHandlers();
    });
})();