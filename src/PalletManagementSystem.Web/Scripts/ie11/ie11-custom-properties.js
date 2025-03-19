/**
 * IE11 CSS Custom Properties Fallback
 * This script provides a limited fallback for CSS custom properties (variables) in IE11
 * 
 * Note: This is a simplified version that only handles a specific set of variables
 * For a complete solution, consider a library like css-vars-ponyfill
 */

(function () {
    // Only run in IE 11
    if (!/Trident\/|MSIE/.test(window.navigator.userAgent)) {
        return;
    }

    console.log('Applying IE11 CSS custom properties fallback');

    // Define our color scheme variables
    var cssVars = {
        // Primary colors
        '--primary-color': '#003366',
        '--primary-hover-color': '#5e87b0',
        '--primary-light-color': '#e6eef5',

        // Status colors
        '--success-color': '#2e7d32',
        '--warning-color': '#f57c00',
        '--danger-color': '#c62828',

        // Typography
        '--font-family': '\'Segoe UI\', sans-serif',

        // Spacing
        '--spacing-sm': '0.5rem',
        '--spacing-md': '1rem',
        '--spacing-lg': '1.5rem',

        // Border radius
        '--border-radius': '0.25rem',
        '--card-border-radius': '0.5rem'
    };

    // Apply the variables as inline styles where needed
    function applyStyles() {
        // Primary button styles
        var primaryButtons = document.querySelectorAll('.btn-primary');
        for (var i = 0; i < primaryButtons.length; i++) {
            primaryButtons[i].style.backgroundColor = cssVars['--primary-color'];
            primaryButtons[i].style.borderColor = cssVars['--primary-color'];
        }

        // Card header styles
        var cardHeaders = document.querySelectorAll('.card-header');
        for (var i = 0; i < cardHeaders.length; i++) {
            cardHeaders[i].style.backgroundColor = cssVars['--primary-light-color'];
            cardHeaders[i].style.color = cssVars['--primary-color'];
        }

        // Badge styles
        var successBadges = document.querySelectorAll('.badge-success');
        for (var i = 0; i < successBadges.length; i++) {
            successBadges[i].style.backgroundColor = cssVars['--success-color'];
        }

        var orangeBadges = document.querySelectorAll('.badge-orange');
        for (var i = 0; i < orangeBadges.length; i++) {
            orangeBadges[i].style.backgroundColor = cssVars['--warning-color'];
            orangeBadges[i].style.color = 'white';
        }

        // Table header styles
        var tableHeaders = document.querySelectorAll('.table th');
        for (var i = 0; i < tableHeaders.length; i++) {
            tableHeaders[i].style.backgroundColor = cssVars['--primary-light-color'];
            tableHeaders[i].style.color = cssVars['--primary-color'];
        }

        // Header backgrounds
        var headerAreas = document.querySelectorAll('.pallet-header, .create-header, .edit-header');
        for (var i = 0; i < headerAreas.length; i++) {
            headerAreas[i].style.backgroundColor = cssVars['--primary-light-color'];
        }

        // Header text
        var headerTexts = document.querySelectorAll('.pallet-header h4, .create-header h4, .edit-header h4');
        for (var i = 0; i < headerTexts.length; i++) {
            headerTexts[i].style.color = cssVars['--primary-color'];
        }

        // Info label styles
        var infoLabels = document.querySelectorAll('.pallet-info-label');
        for (var i = 0; i < infoLabels.length; i++) {
            infoLabels[i].style.color = cssVars['--primary-color'];
        }

        // Navbar styles
        var navbars = document.querySelectorAll('.navbar');
        for (var i = 0; i < navbars.length; i++) {
            navbars[i].style.backgroundColor = cssVars['--primary-color'];
        }
    }

    // Apply on document ready
    document.addEventListener('DOMContentLoaded', applyStyles);

    // Also apply when DOM changes - simplified mutation observer
    // This is a basic implementation - for production, consider debouncing this
    if (window.MutationObserver) {
        var observer = new MutationObserver(function (mutations) {
            applyStyles();
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    } else {
        // Fallback for older browsers without MutationObserver
        // Periodically check for changes
        setInterval(applyStyles, 1000);
    }
})();