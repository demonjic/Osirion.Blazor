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

// One script element for the page, however many forms ask for it.
function loadTurnstile() {
    if (window.turnstile) {
        return Promise.resolve(window.turnstile);
    }

    turnstileLoading ??= new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = TURNSTILE_SCRIPT;
        script.async = true;
        script.addEventListener('load', () => window.turnstile ? resolve(window.turnstile) : reject(new Error('Turnstile is not available.')));
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
        this.token = '';
        this.sending = false;
        this.setEventListener(form, 'submit', event => this.send(event));
        form.hidden = false;

        if (turnstile) {
            this.renderWidget(turnstile);
        }
    }

    async renderWidget(container) {
        const form = this.form;
        let api;
        try {
            api = await loadTurnstile();
        } catch {
            this.say('check');
            return;
        }

        // The component was disposed, or given another form, while the script loaded.
        if (this.form !== form) {
            return;
        }

        this.turnstile = api;
        this.widget = api.render(container, {
            sitekey: container.dataset.sitekey,
            action: TURNSTILE_ACTION,
            theme: themeFor(form),
            callback: token => { this.token = token; },
            'expired-callback': () => { this.token = ''; },
            'error-callback': () => {
                this.token = '';
                this.say('check');
            }
        });
    }

    async send(event) {
        event.preventDefault();
        if (this.sending || !this.form.reportValidity()) {
            return;
        }

        if (this.needsToken && !this.token) {
            this.say('pending');
            return;
        }

        const form = this.form;
        const data = new FormData(form);
        const body = {
            name: data.get('name'),
            email: data.get('email'),
            ...(data.has('company') ? { company: data.get('company') } : {}),
            message: data.get('message'),
            website: data.get('website'),
            token: this.token
        };

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
