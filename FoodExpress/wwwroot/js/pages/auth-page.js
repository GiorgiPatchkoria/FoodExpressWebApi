import {
	authenticate,
	isAdmin,
	currentUser
} from '../auth.js';
import {
	field,
	bindForm,
	notify
} from '../utils.js';
export async function authPage(view, params, ctx, kind) {
	if (currentUser()) {
		ctx.go(isAdmin() ? '/admin' : '/restaurants');
		return;
	}
	const register = kind === 'register';
	view.innerHTML = `<div class="auth-layout"><section class="auth-art"><p class="eyebrow">YOUR NEXT FAVORITE, DELIVERED</p><h1>A little more<br><em>delicious.</em></h1><div class="auth-plate"><span>Good food.<br>Great company.<br>Your place.</span></div><p>Discover the kitchens around you.<br>Make room for something wonderful.</p></section><section class="auth-panel"><a class="subtle-link" href="#/">← Back to FoodExpress</a><h1>${register ? 'A seat at the table.' : 'Welcome back.'}</h1><p class="muted">${register ? 'Your next favorite meal starts here.' : 'Good food is only a few clicks away.'}</p><form id="auth-form"><div data-errors></div>${register ? field('fullName', 'Full name', { max: 150, autocomplete: 'name' }) + field('userName', 'Username', { min: 3, max: 50, pattern: '[a-zA-Z0-9_.\-]+', autocomplete: 'username' }) : ''}${field('email', 'Email address', { type: 'email', max: 256, autocomplete: 'email' })}${register ? field('phoneNumber', 'Phone number (optional)', { type: 'tel', max: 30, required: false, autocomplete: 'tel' }) : ''}${field('password', 'Password', { type: 'password', min: register ? 12 : 1, max: 128, autocomplete: register ? 'new-password' : 'current-password' })}${register ? '<p class="field-help">Use 12–128 characters.</p>' + field('confirmPassword', 'Confirm password', { type: 'password', min: 12, max: 128, autocomplete: 'new-password' }) : ''}<button class="button full" type="submit">${register ? 'Create my account' : 'Sign in'} <span>→</span></button></form>${!register ? '<p class="auth-switch"><a href="#/forgot-password">Forgot your password?</a></p>' : ''}<p class="auth-switch">${register ? 'Already part of the table? <a href="/login.html">Sign in</a>' : 'New here? <a href="/register.html">Create an account</a>'}</p></section></div>`;
	const form = view.querySelector('form');
	const confirm = form.elements.confirmPassword;
	if (confirm) {
		const validate = () => confirm.setCustomValidity(confirm.value !== form.elements.password.value ? 'Passwords must match.' : '');
		confirm.addEventListener('input', validate);
		form.elements.password.addEventListener('input', validate);
	}
	bindForm(form, async data => {
		delete data.confirmPassword;
		if (register && !data.phoneNumber.trim()) {
			data.phoneNumber = null;
		}
		await authenticate(kind, data);
		notify(register ? 'Your account is ready. Welcome to FoodExpress!' : 'Welcome back!');
		const next = params.get('next');
		ctx.go(next?.startsWith('/') && !next.startsWith('//') && !next.includes('://') ? next : isAdmin() ? '/admin' : '/restaurants');
	});
}