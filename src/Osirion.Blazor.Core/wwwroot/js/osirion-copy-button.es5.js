'use strict';

(function () {
    'use strict';

    // Define global copy function for onclick usage
    window.osirionCopyCode = function (button) {
        // Find the wrapper
        var wrapper = button.closest('.osirion-code-wrapper');
        if (!wrapper) {
            console.error('Could not find code wrapper');
            return;
        }

        // Look for code element - it might be in a pre tag
        var codeElement = wrapper.querySelector('pre code') || wrapper.querySelector('code');
        if (!codeElement) {
            console.error('Could not find code element in wrapper');
            return;
        }

        var text = codeElement.textContent || codeElement.innerText || '';
        var textSpan = button.querySelector('.osirion-copy-text') || button;
        var originalText = textSpan.textContent;
        // The renderer puts the language in the label ("Copy C# code to clipboard"), so put that one back, not a generic one
        var originalLabel = button.getAttribute('aria-label');

        // Copy function
        function copyWithFallback() {
            // Try modern clipboard API
            if (navigator.clipboard && window.isSecureContext) {
                navigator.clipboard.writeText(text).then(function () {
                    showSuccess();
                })['catch'](function (err) {
                    console.warn('Clipboard API failed:', err);
                    fallbackCopy();
                });
            } else {
                fallbackCopy();
            }
        }

        // Fallback copy method
        function fallbackCopy() {
            var textarea = document.createElement('textarea');
            textarea.value = text;
            textarea.style.position = 'fixed';
            textarea.style.left = '-999999px';
            textarea.style.top = '-999999px';
            document.body.appendChild(textarea);

            try {
                textarea.select();
                document.execCommand('copy');
                showSuccess();
            } catch (err) {
                console.error('Copy failed:', err);
                showError();
            }

            document.body.removeChild(textarea);
        }

        // Show success state
        function showSuccess() {
            textSpan.textContent = 'Copied!';
            button.classList.add('copied');
            button.setAttribute('aria-label', 'Code copied to clipboard');

            setTimeout(function () {
                textSpan.textContent = originalText;
                button.classList.remove('copied');
                if (originalLabel === null) {
                    button.removeAttribute('aria-label');
                } else {
                    button.setAttribute('aria-label', originalLabel);
                }
            }, 2000);
        }

        // Show error state
        function showError() {
            textSpan.textContent = 'Error';
            button.classList.add('error');

            setTimeout(function () {
                textSpan.textContent = originalText;
                button.classList.remove('error');
            }, 2000);
        }

        // Execute copy
        copyWithFallback();
    };

    // Also set up event delegation for dynamically added buttons
    document.addEventListener('DOMContentLoaded', function () {
        // Add keyboard support
        document.addEventListener('keydown', function (event) {
            if (event.key !== 'Enter' && event.key !== ' ') return;

            var button = event.target;
            if (button && button.classList && button.classList.contains('osirion-copy-button')) {
                event.preventDefault();
                window.osirionCopyCode(button);
            }
        });
    });
})();

