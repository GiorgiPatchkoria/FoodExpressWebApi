import { users } from '../services/user-service.js';
import { logout } from '../auth.js';
import { field, bindForm, headerText, notify, empty, escape } from '../utils.js';
import { bindPasswordConfirmation } from './account-page.js';

export async function recoveryPage(view, params, ctx, reset) {
    const token = params.get('token');
    const email = params.get('email') || '';
    view.innerHTML = headerText('ACCOUNT SECURITY', reset ? 'A fresh start.' : 'Let’s get you back in.', reset ? 'Choose a new password for your account.' : 'We’ll email you a link to reset your password.');
    if (reset && (!token || !email)) {
        view.innerHTML += empty('This link is incomplete.', 'Request a new password reset email.', '#/forgot-password', 'Request a new link');
        return;
    }
    if (reset) {
    	history.replaceState(null, '', location.pathname + '#/reset-password');
    }
    view.innerHTML += `<section class="panel recovery-panel"><form class="stack-form"><div data-errors></div>${reset ? '<p class="muted">Resetting password for ' + escape(email) + '</p>' + field('newPassword', 'New password', { type: 'password', min: 12, max: 128, autocomplete: 'new-password' }) + field('confirmPassword', 'Confirm new password', { type: 'password', min: 12, max: 128, autocomplete: 'new-password' }) : field('email', 'Email address', { type: 'email', max: 256, autocomplete: 'email' })}<button type="submit" class="button">${reset ? 'Save new password' : 'Send reset link'}</button></form><p class="auth-switch"><a href="/login.html">Back to sign in</a></p></section>`;
    const form = view.querySelector('form');
    if (reset) {
    	bindPasswordConfirmation(form);
    }
    bindForm(form, async data => {
        if (reset) {
            await users.resetPassword({ ...data, email, token });
            logout();
            notify('Password reset. Sign in with your new password.');
            ctx.go('/login');
        } else {
            const message = await users.forgotPassword(data.email);
            view.querySelector('.recovery-panel').innerHTML = `<div class="notice" role="status">${escape(message)}</div><p class="field-help">Links expire after 30 minutes. Check your spam folder too. If no email arrives, try again later or contact the site administrator.</p><a class="button secondary" href="/login.html">Back to sign in</a>`;
        }
    });
}
