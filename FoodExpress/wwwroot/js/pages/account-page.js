import { users } from '../services/user-service.js';
import { logout } from '../auth.js';
import {
	addresses
} from '../services/orders.js';
import {
	initialize
} from '../auth.js';
import {
	escape,
	headerText,
	field,
	bindForm,
	notify,
	empty
} from '../utils.js';
export function addressForm() {
	return `<form id="address-form" class="stack-form"><div data-errors></div>${field('streetAddress', 'Street address', { max: 250, autocomplete: 'street-address' })}<div class="form-grid">${field('city', 'City', { max: 100, autocomplete: 'address-level2' })}${field('postalCode', 'Postal code (optional)', { max: 20, required: false, autocomplete: 'postal-code' })}</div>${field('countryCode', 'Country code', { min: 2, max: 2, pattern: '[A-Za-z]{2}', autocomplete: 'country' })}<p class="field-help">Two letters, for example GE, GB, or US.</p><button class="button" type="submit">Save address →</button></form>`;
}
export function bindAddress(form, done) {
	bindForm(form, async data => {
		data.countryCode = data.countryCode.toUpperCase();
		data.postalCode = data.postalCode.trim() || null;
		await addresses.create(data);
		notify('Your address has been saved.');
		await done();
	});
}
export function addressCard(a) {
	return `<article class="address-card"><span class="eyebrow">DELIVER TO</span><h3>${escape(a.streetAddress)}</h3><p>${escape(a.city)}${a.postalCode ? ', ' + escape(a.postalCode) : ''}</p><span>${escape(a.countryCode)}</span></article>`;
}
export async function addressPage(view, params, ctx) {
	const rows = await addresses.list();
	view.innerHTML = headerText('YOUR PLACES', 'A good meal starts at home.', 'Save your delivery addresses for a quicker checkout.') + `<div class="two-column"><section><h2>Saved addresses</h2><div class="address-grid">${rows.length ? rows.map(addressCard).join('') : empty('Where should we meet you?', 'Add your first delivery address using the form.', null)}</div></section><section class="panel"><h2>Add an address</h2>${addressForm()}</section></div>`;
	bindAddress(view.querySelector('#address-form'), ctx.refresh);
}
export async function profilePage(view, params, ctx) {
	const user = await initialize();
	view.innerHTML = headerText('YOUR ACCOUNT', 'Hello, ' + (user.fullName || user.userName) + '.', 'All the details that make this your table.') + `<div class="profile-layout"><div class="profile-avatar">${escape((user.fullName || user.userName).slice(0, 1))}</div><div class="panel"><dl class="details-list">${[['Full name', user.fullName], ['Username', user.userName], ['Email', user.email], ['Phone number', user.phoneNumber || 'Not provided'], ['Account role', user.role]].map(([label, value]) => `<div><dt>${label}</dt><dd>${escape(value)}</dd></div>`).join('')}</dl><div class="actions"><a class="button secondary" href="#/addresses">My addresses</a><a class="button" href="#/orders">My orders →</a></div></div></div>`;
    renderProfileForms(view, user, ctx);
}
export function bindPasswordConfirmation(form) {
    const password = form.elements.newPassword;
    const confirmation = form.elements.confirmPassword;
    function validate() {
        confirmation.setCustomValidity(password.value !== confirmation.value ? 'Passwords must match.' : '');
    }
    password.addEventListener('input', validate);
    confirmation.addEventListener('input', validate);
}

function renderProfileForms(view, user, ctx) {
    view.insertAdjacentHTML('beforeend', `<div class="two-column"><section class="panel"><h2>Edit your profile</h2><form id="profile-form" class="stack-form"><div data-errors></div>${field('fullName', 'Full name', { value: user.fullName, max: 150, autocomplete: 'name' })}${field('userName', 'Username', { value: user.userName, min: 3, max: 50, pattern: '[a-zA-Z0-9_.\\-]+', autocomplete: 'username' })}${field('email', 'Email address', { value: user.email, type: 'email', max: 256, autocomplete: 'email' })}${field('phoneNumber', 'Phone number (optional)', { value: user.phoneNumber, type: 'tel', max: 30, required: false, autocomplete: 'tel' })}${field('currentPassword', 'Current password (required to change email)', { type: 'password', max: 128, required: false, autocomplete: 'current-password' })}<button type="submit" class="button">Save profile</button></form></section><section class="panel"><h2>Change your password</h2><p class="field-help">Choose 12–128 characters. Changing your password signs out all active sessions.</p><form id="password-form" class="stack-form"><div data-errors></div>${field('currentPassword', 'Current password', { type: 'password', max: 128, autocomplete: 'current-password' })}${field('newPassword', 'New password', { type: 'password', min: 12, max: 128, autocomplete: 'new-password' })}${field('confirmPassword', 'Confirm new password', { type: 'password', min: 12, max: 128, autocomplete: 'new-password' })}<button type="submit" class="button">Change password</button></form><p class="auth-switch"><a href="#/forgot-password">Forgot your password?</a></p></section></div>`);
    bindForm(view.querySelector('#profile-form'), async data => {
        data.phoneNumber = data.phoneNumber.trim() || null;
        data.currentPassword = data.currentPassword || null;
        await users.update(data);
        await initialize();
        notify('Your profile has been updated.');
        ctx.refresh();
    });
    const passwordForm = view.querySelector('#password-form');
    bindPasswordConfirmation(passwordForm);
    bindForm(passwordForm, async data => {
        await users.changePassword(data);
        logout();
        notify('Password changed. Sign in with your new password.');
        ctx.go('/login');
    });
}
