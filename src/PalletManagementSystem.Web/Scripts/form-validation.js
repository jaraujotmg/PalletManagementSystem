// Form Validation for IE11 Compatibility
(function () {
    'use strict';

    // Validate a form
    window.validateForm = function (formElement) {
        if (!formElement) return false;

        var isValid = true;
        var firstInvalidField = null;

        // Required fields
        var requiredFields = formElement.querySelectorAll('[required]');
        for (var i = 0; i < requiredFields.length; i++) {
            var field = requiredFields[i];
            var value = field.value.trim();

            if (value === '') {
                markFieldAsInvalid(field, 'This field is required');
                isValid = false;
                if (!firstInvalidField) firstInvalidField = field;
            } else {
                markFieldAsValid(field);
            }
        }

        // Numeric fields
        var numericFields = formElement.querySelectorAll('input[type="number"], .numeric');
        for (var i = 0; i < numericFields.length; i++) {
            var field = numericFields[i];
            var value = field.value.trim();

            if (value !== '') {
                var numValue = parseFloat(value);
                var min = field.getAttribute('min');
                var max = field.getAttribute('max');

                // Check if it's a number
                if (isNaN(numValue)) {
                    markFieldAsInvalid(field, 'Please enter a valid number');
                    isValid = false;
                    if (!firstInvalidField) firstInvalidField = field;
                }
                // Check min value
                else if (min !== null && numValue < parseFloat(min)) {
                    markFieldAsInvalid(field, 'Value must be at least ' + min);
                    isValid = false;
                    if (!firstInvalidField) firstInvalidField = field;
                }
                // Check max value
                else if (max !== null && numValue > parseFloat(max)) {
                    markFieldAsInvalid(field, 'Value must be at most ' + max);
                    isValid = false;
                    if (!firstInvalidField) firstInvalidField = field;
                }
                else {
                    markFieldAsValid(field);
                }
            }
        }

        // Focus first invalid field
        if (firstInvalidField) {
            firstInvalidField.focus();
        }

        return isValid;
    };

    // Mark a field as invalid
    function markFieldAsInvalid(field, message) {
        field.classList.add('is-invalid');

        // Add or update error message
        var errorDiv = field.parentNode.querySelector('.invalid-feedback');
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.className = 'invalid-feedback';
            field.parentNode.appendChild(errorDiv);
        }
        errorDiv.textContent = message;
    }

    // Mark a field as valid
    function markFieldAsValid(field) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
    }

    // Setup form validation on submit
    function setupFormValidation() {
        var forms = document.querySelectorAll('form.needs-validation');

        for (var i = 0; i < forms.length; i++) {
            addEvent(forms[i], 'submit', function (e) {
                if (!validateForm(this)) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            });
        }
    }

    // Add event handler to initialize form validation when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        setupFormValidation();
    });
})();