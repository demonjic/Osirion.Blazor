// Sends the contact form to its endpoint as JSON and says in the status region what happened. When the form carries a
// Turnstile site key, the script renders the widget first and sends its token with the message. The form is rendered
// hidden and this script reveals it, because without script it could send nothing.

// Turnstile's script must be loaded from this exact address, never proxied or cached. Explicit rendering lets this
// script put the widget into the form's own container.
const TURNSTILE_SCRIPT = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';

// Reported with every token, so the endpoint can check that siteverify names this form's action.
const TURNSTILE_ACTION = 'contact';

// Each outcome and the data attribute of the form that holds its text.
const MESSAGES = {
    success: 'successMessage',
    fields: 'fieldsErrorMessage',
    check: 'checkErrorMessage',
    pending: 'checkPendingMessage',
    failed: 'errorMessage'
};

let turnstileLoading;

// The API is recognized by its render method, not by the name: an element with the id or name "turnstile", such as a
// heading's generated id, is also window.turnstile until the script assigns the API.
function loadedTurnstile() {
    return typeof window.turnstile?.render === 'function' ? window.turnstile : undefined;
}

// One script element for the page, however many forms ask for it.
function loadTurnstile() {
    const loaded = loadedTurnstile();
    if (loaded) {
        return Promise.resolve(loaded);
    }

    turnstileLoading ??= new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = TURNSTILE_SCRIPT;
        script.async = true;
        script.addEventListener('load', () => {
            const api = loadedTurnstile();
            if (api) {
                resolve(api);
            } else {
                reject(new Error('Turnstile is not available.'));
            }
        });
        script.addEventListener('error', () => reject(new Error('Turnstile did not load.')));
        document.head.append(script);
    });

    return turnstileLoading;
}

// The widget follows the color mode the page or the component sets, and otherwise the visitor's system setting.
function themeFor(form) {
    const themed = form.closest('[data-theme], [data-bs-theme]');
    const theme = themed?.getAttribute('data-theme')
        ?? themed?.getAttribute('data-bs-theme')
        ?? document.documentElement.getAttribute('data-osirion-theme');
    return theme === 'light' || theme === 'dark' ? theme : 'auto';
}

// The endpoint answers {ok: true} or {ok: false, error: 'fields' | 'check' | 'failed'}; anything else is a failure.
function outcomeOf(answer) {
    if (answer?.ok === true) {
        return 'success';
    }

    return answer?.error === 'fields' || answer?.error === 'check' ? answer.error : 'failed';
}

export default class extends BlazorJSComponents.Component {
    setParameters(refs) {
        const { form, status, submit, turnstile } = refs;

        // A re-render passes the same form again, and it is already wired.
        if (!form || !status || !submit || form === this.form) {
            return;
        }

        this.removeWidget();
        this.form = form;
        this.texts = form.dataset;
        this.status = status;
        this.submit = submit;
        this.needsToken = Boolean(turnstile);
        this.checkUnavailable = false;
        this.token = '';
        this.sending = false;
        this.setEventListener(form, 'submit', event => this.send(event));
        form.hidden = false;

        if (turnstile) {
            // Not awaited on purpose: the form is usable while the check loads, and renderWidget handles its own failures.
            void this.renderWidget(turnstile);
        }
    }

    async renderWidget(container) {
        const form = this.form;
        let api;
        try {
            api = await loadTurnstile();
        } catch {
            this.failCheck();
            return;
        }

        // The component was disposed, or given another form, while the script loaded.
        if (this.form !== form) {
            return;
        }

        try {
            this.widget = api.render(container, {
                sitekey: container.dataset.sitekey,
                action: TURNSTILE_ACTION,
                theme: themeFor(form),
                callback: token => { this.token = token; },
                'expired-callback': () => { this.token = ''; },
                // Turnstile retries on its own after an error, so a token can still arrive.
                'error-callback': () => {
                    this.token = '';
                    this.say('check');
                }
            });
            this.turnstile = api;
        } catch {
            this.failCheck();
        }
    }

    // Without a widget no token can ever arrive, so a send says the check failed instead of asking the visitor to wait.
    failCheck() {
        this.checkUnavailable = true;
        this.say('check');
    }

    async send(event) {
        event.preventDefault();
        if (this.sending || !this.form.reportValidity()) {
            return;
        }

        if (this.needsToken && !this.token) {
            this.say(this.checkUnavailable ? 'check' : 'pending');
            return;
        }

        const form = this.form;
        const data = new FormData(form);
        const body = {
            name: data.get('name'),
            email: data.get('email'),
            ...(data.has('company') ? { company: data.get('company') } : {}),
            ...(data.has('subject') ? { subject: data.get('subject') } : {}),
            message: data.get('message'),
            website: data.get('website'),
            token: this.token
        };

        // A disabled button loses focus to the page, so focus is put back afterwards unless the visitor moved it.
        const hadFocus = document.activeElement === this.submit;
        this.sending = true;
        this.submit.disabled = true;
        this.submit.textContent = this.texts.submittingText;
        // Emptied first, so the same answer twice in a row is announced again.
        this.clearStatus();

        let outcome;
        try {
            const response = await fetch(form.action, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            outcome = outcomeOf(await response.json().catch(() => undefined));
        } catch {
            outcome = 'failed';
        }

        if (outcome === 'success') {
            form.reset();
        }

        // A token is good for one check only, so every attempt needs a fresh one.
        this.token = '';
        if (this.widget !== undefined) {
            this.turnstile.reset(this.widget);
        }

        this.submit.textContent = this.texts.submitText;
        this.submit.disabled = false;
        if (hadFocus && (document.activeElement === null || document.activeElement === document.body)) {
            this.submit.focus();
        }

        this.sending = false;
        this.say(outcome);
    }

    say(outcome) {
        this.status.dataset.state = outcome;
        this.status.textContent = this.texts[MESSAGES[outcome]] ?? '';
    }

    clearStatus() {
        delete this.status.dataset.state;
        this.status.textContent = '';
    }

    removeWidget() {
        if (this.widget !== undefined) {
            this.turnstile.remove(this.widget);
            this.widget = undefined;
        }
    }

    dispose() {
        this.removeWidget();
        this.form = undefined;
    }
}
