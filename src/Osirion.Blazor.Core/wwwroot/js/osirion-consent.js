/*
 * Osirion.Blazor.Core: cookie consent evaluated in the browser (ConsentEvaluation.Browser).
 *
 * A page exported to static files is the same for every visitor, so the server cannot decide whether to show the
 * banner or to run the trackers. OsirionCookieConsent renders the banner hidden, and the trackers render their scripts
 * held: <script type="text/plain" data-osirion-consent="analytics">, with a script's own type kept in
 * data-osirion-type. This module:
 *
 * - shows the banner while there is no valid osirion_cookie_consent cookie;
 * - records Accept, Decline and Save Preferences in that cookie, in the format the consent endpoint writes
 *   (CookieConsentHandler), hides the banner and dispatches "osirion-consent-change" on window, with the consent type
 *   and the categories in event.detail;
 * - runs the held scripts whose category the consent includes, on page load when the cookie already says so and right
 *   after a choice, once per page and in document order;
 * - reopens the banner when an element with data-osirion-consent-open is clicked, so a footer link lets visitors change
 *   or withdraw consent. A running tracker cannot be stopped, so a choice that withdraws a category whose scripts already
 *   ran reloads the page, which then leaves them held.
 *
 * It is a file from the site's own origin and writes no inline script, no event handler attribute and no eval, so it
 * works under a strict Content Security Policy. The scripts it runs need what they would need anyway: their hosts in
 * script-src, and for an inline one its hash (the text is run unchanged) or the nonce it was rendered with.
 * It is loaded by the banner and by every tracker; the flag below keeps a second copy from doing anything.
 */
(function () {
    'use strict';

    if (window.__osirionConsent) {
        return;
    }
    window.__osirionConsent = true;

    const COOKIE_VERSION = '1.0';
    const DEFAULT_DAYS = 365;
    const SECONDS_PER_DAY = 24 * 60 * 60;
    // The categories Accept and Decline record, as the consent endpoint does.
    const STANDARD_CATEGORIES = ['necessary', 'analytics', 'marketing', 'preferences'];
    const CATEGORY_PREFIX = 'category_';
    const CHOICES = ['accepted', 'declined', 'save-preferences'];

    const BANNER = '[data-osirion-consent-banner]';
    const OPENER = '[data-osirion-consent-open]';
    const CHOICE_BUTTON = 'button[name="consent"]';
    const HELD = 'script[type="text/plain"][data-osirion-consent]';
    const REGION = '.osirion-cookie-consent';
    const PANEL = '.osirion-cookie-consent-customize';
    const CUSTOMIZING = 'osirion-cookie-consent-customizing';
    const CONSENT_ATTRIBUTES = ['type', 'data-osirion-consent', 'data-osirion-type', 'nonce'];

    // Held scripts that already ran in this document, by content: enhanced navigation patches the page in place and can
    // leave a script that ran looking held again, and it must not run twice.
    const started = new Set();
    // Categories whose scripts ran in this document; withdrawing one of them needs a reload.
    const running = new Set();
    let opener = null;

    function readConsent() {
        const match = document.cookie.match(/(?:^|;\s*)osirion_cookie_consent=([^;]*)/);
        if (!match) {
            return null;
        }

        try {
            const consent = JSON.parse(decodeURIComponent(match[1]));
            const categories = consent && consent.Categories;
            return categories && typeof categories === 'object' && !Array.isArray(categories) ? consent : null;
        } catch (error) {
            return null;
        }
    }

    // The endpoint writes the value with Uri.EscapeDataString, which also escapes ! ' ( ) and *.
    function escapeLikeTheEndpoint(value) {
        return encodeURIComponent(value).replace(/[!'()*]/g, function (character) {
            return '%' + character.charCodeAt(0).toString(16).toUpperCase();
        });
    }

    function writeConsent(consentType, categories, days) {
        const consent = {
            Version: COOKIE_VERSION,
            ConsentDate: new Date().toISOString(),
            ConsentType: consentType,
            Categories: categories
        };
        const secure = window.location.protocol === 'https:' ? '; Secure' : '';
        document.cookie = 'osirion_cookie_consent=' + escapeLikeTheEndpoint(JSON.stringify(consent))
            + '; path=/; max-age=' + days * SECONDS_PER_DAY + '; SameSite=Lax' + secure;
        return consent;
    }

    function checkboxes(banner) {
        return Array.prototype.slice.call(banner.querySelectorAll('input[type="checkbox"][name^="' + CATEGORY_PREFIX + '"]'));
    }

    // The endpoint removes every occurrence of the prefix from the field name.
    function categoryOf(checkbox) {
        return checkbox.name.split(CATEGORY_PREFIX).join('');
    }

    function categoriesFor(banner, consentType) {
        const categories = {};
        if (consentType === 'save-preferences') {
            // Necessary plus each ticked category; an unticked one is left out, which counts as no consent.
            categories.necessary = true;
            checkboxes(banner).forEach(function (checkbox) {
                if (checkbox.checked && !checkbox.disabled) {
                    categories[categoryOf(checkbox)] = true;
                }
            });
            return categories;
        }

        STANDARD_CATEGORIES.forEach(function (category) {
            categories[category] = category === 'necessary' || consentType === 'accepted';
        });
        return categories;
    }

    function expiryDays(banner) {
        const days = parseInt(banner.getAttribute('data-osirion-consent-days'), 10);
        return days >= 1 ? days : DEFAULT_DAYS;
    }

    function setPanel(banner, open) {
        const panel = banner.querySelector(PANEL);
        const toggle = banner.querySelector(CHOICE_BUTTON + '[value="customize"]');
        const region = banner.querySelector(REGION);
        if (panel) {
            panel.hidden = !open;
        }
        if (toggle) {
            toggle.setAttribute('aria-expanded', String(open));
        }
        if (region) {
            region.classList.toggle(CUSTOMIZING, open);
        }
    }

    // A reopened panel shows the saved choice for each category; a first visit shows the rendered defaults.
    function showBanner(banner, consent) {
        checkboxes(banner).forEach(function (checkbox) {
            if (!checkbox.disabled) {
                const saved = consent ? consent.Categories[categoryOf(checkbox)] : undefined;
                checkbox.checked = typeof saved === 'boolean' ? saved : checkbox.defaultChecked;
            }
        });
        banner.hidden = false;
    }

    function hideBanner(banner) {
        setPanel(banner, false);
        banner.hidden = true;
    }

    function openBanner(trigger) {
        const banner = document.querySelector(BANNER);
        if (!banner) {
            return;
        }

        opener = trigger;
        showBanner(banner, readConsent());
        // Move focus into the banner, which the visitor asked for, so keyboard and screen reader users land on it.
        const region = banner.querySelector(REGION) || banner;
        if (!region.hasAttribute('tabindex')) {
            region.setAttribute('tabindex', '-1');
        }
        region.focus();
    }

    function returnFocus() {
        if (opener && opener.isConnected) {
            opener.focus();
        }
        opener = null;
    }

    function choose(banner, consentType) {
        const consent = writeConsent(consentType, categoriesFor(banner, consentType), expiryDays(banner));
        hideBanner(banner);
        returnFocus();
        window.dispatchEvent(new CustomEvent('osirion-consent-change', {
            detail: { consentType: consentType, categories: consent.Categories }
        }));

        const withdrawn = Array.from(running).some(function (category) {
            return consent.Categories[category] !== true;
        });
        if (withdrawn) {
            window.location.reload();
            return;
        }

        release(consent.Categories);
    }

    function keyOf(held) {
        const attributes = held.getAttributeNames().filter(function (name) {
            return name !== 'nonce';
        }).sort().map(function (name) {
            return name + '=' + held.getAttribute(name);
        });
        return attributes.join('\n') + '\n' + held.text;
    }

    // As the parser would run them: async scripts start without waiting, defer and module scripts wait for the rest.
    function isDeferred(held) {
        if (held.hasAttribute('async')) {
            return false;
        }
        const type = (held.getAttribute('data-osirion-type') || '').trim().toLowerCase();
        return type === 'module' || (held.hasAttribute('defer') && held.hasAttribute('src'));
    }

    // Replaces the held script with one the browser runs. The nonce is read from the property, because the browser hides
    // the attribute value.
    function run(held) {
        // Enhanced navigation can remove a held script before its turn; the next sync runs the page's copy instead.
        if (!held.isConnected) {
            started.delete(keyOf(held));
            return Promise.resolve();
        }

        const script = document.createElement('script');
        held.getAttributeNames().forEach(function (name) {
            if (CONSENT_ATTRIBUTES.indexOf(name) < 0) {
                script.setAttribute(name, held.getAttribute(name));
            }
        });
        const type = held.getAttribute('data-osirion-type');
        if (type !== null) {
            script.setAttribute('type', type);
        }
        if (held.nonce) {
            script.nonce = held.nonce;
        }
        // A script added from script runs as soon as it loads unless async is turned off; off keeps the page's order.
        script.async = held.hasAttribute('async');
        script.text = held.text;

        return new Promise(function (resolve) {
            if (script.hasAttribute('src') && !script.async) {
                script.addEventListener('load', resolve);
                script.addEventListener('error', resolve);
            } else {
                resolve();
            }
            held.replaceWith(script);
        });
    }

    async function runInOrder(scripts) {
        const later = scripts.filter(isDeferred);
        const now = scripts.filter(function (held) {
            return !isDeferred(held);
        });
        for (const held of now) {
            await run(held);
        }
        for (const held of later) {
            await run(held);
        }
    }

    function release(categories) {
        const scripts = Array.prototype.slice.call(document.querySelectorAll(HELD)).filter(function (held) {
            const category = held.getAttribute('data-osirion-consent');
            if (categories[category] !== true) {
                return false;
            }
            const key = keyOf(held);
            if (started.has(key)) {
                return false;
            }
            started.add(key);
            running.add(category);
            return true;
        });
        if (scripts.length > 0) {
            void runInOrder(scripts);
        }
    }

    // On load and after every enhanced navigation, which renders the banner hidden again.
    function sync() {
        const consent = readConsent();
        const banner = document.querySelector(BANNER);
        if (banner && !consent) {
            showBanner(banner, null);
        }
        if (consent) {
            release(consent.Categories);
        }
    }

    function closestTo(event, selector) {
        return event.target instanceof Element ? event.target.closest(selector) : null;
    }

    // In the capture phase, so the link is not followed: Blazor's enhanced navigation leaves a click alone once its
    // default is prevented, and it listens in the bubble phase.
    document.addEventListener('click', function (event) {
        const trigger = closestTo(event, OPENER);
        if (trigger) {
            event.preventDefault();
            openBanner(trigger);
        }
    }, true);

    document.addEventListener('click', function (event) {
        const button = closestTo(event, CHOICE_BUTTON);
        const banner = button ? button.closest(BANNER) : null;
        if (!banner) {
            return;
        }

        if (button.value === 'customize') {
            const panel = banner.querySelector(PANEL);
            setPanel(banner, Boolean(panel && panel.hidden));
        } else if (CHOICES.indexOf(button.value) >= 0) {
            choose(banner, button.value);
        }
    });

    function listenForEnhancedNavigation() {
        if (window.Blazor && typeof window.Blazor.addEventListener === 'function') {
            window.Blazor.addEventListener('enhancedload', sync);
            return true;
        }
        return false;
    }

    // blazor.web.js normally runs before this module; a host that starts Blazor later is picked up once the page loaded.
    if (!listenForEnhancedNavigation()) {
        window.addEventListener('load', listenForEnhancedNavigation, { once: true });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', sync);
    } else {
        sync();
    }
})();
