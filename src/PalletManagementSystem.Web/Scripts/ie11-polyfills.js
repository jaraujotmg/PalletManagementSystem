// IE11 Polyfills and Helper Functions
(function () {
    'use strict';

    // Polyfill for Element.closest
    if (!Element.prototype.closest) {
        Element.prototype.closest = function (selector) {
            var element = this;
            do {
                if (element.matches(selector)) return element;
                element = element.parentElement || element.parentNode;
            } while (element !== null && element.nodeType === 1);
            return null;
        };
    }

    // Polyfill for Element.matches
    if (!Element.prototype.matches) {
        Element.prototype.matches =
            Element.prototype.msMatchesSelector ||
            Element.prototype.webkitMatchesSelector;
    }

    // Polyfill for forEach on NodeList
    if (window.NodeList && !NodeList.prototype.forEach) {
        NodeList.prototype.forEach = Array.prototype.forEach;
    }

    // Utility to add event listener with proper IE support
    window.addEvent = function (element, eventName, handler) {
        if (element.addEventListener) {
            element.addEventListener(eventName, handler, false);
        } else if (element.attachEvent) {
            element.attachEvent('on' + eventName, handler);
        }
    };

    // Utility to remove event listener with proper IE support
    window.removeEvent = function (element, eventName, handler) {
        if (element.removeEventListener) {
            element.removeEventListener(eventName, handler, false);
        } else if (element.detachEvent) {
            element.detachEvent('on' + eventName, handler);
        }
    };

    // Debounce function for search inputs
    window.debounce = function (func, wait, immediate) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    };
})();