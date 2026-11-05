import {
	api,
	session,
	saveSession,
	clearSession
} from './api.js';
let user = null;
let expiryTimer;
export function currentUser() {
	return user;
}
export function isAdmin() {
	return user?.role === 'Admin';
}

function scheduleExpiry() {
	clearTimeout(expiryTimer);
	const saved = session();
	if (saved) {
		expiryTimer = setTimeout(() => {
			logout();
			window.dispatchEvent(new CustomEvent('session-expired'));
		}, Math.min(Date.parse(saved.expiresAt) - Date.now(), 2147483647));
	}
}
export async function initialize() {
	const startingSession = session();
	if (!startingSession) {
		user = null;
		return null;
	}
	const profile = await api.get('/users/me', {
		auth: true
	});
	if (session()?.accessToken !== startingSession.accessToken) {
		return null;
	}
	user = profile;
	scheduleExpiry();
	window.dispatchEvent(new CustomEvent('auth-changed'));
	return user;
}
export async function authenticate(kind, body) {
	const result = await api.post(`/auth/${kind}`, body);
	saveSession(result);
	user = null;
	return initialize();
}
export function logout() {
	clearTimeout(expiryTimer);
	clearSession();
	user = null;
	window.dispatchEvent(new CustomEvent('auth-changed'));
}
window.addEventListener('session-expired', () => {
	user = null;
	clearTimeout(expiryTimer);
});