import {
	config
} from './config.js';
const sessionKey = 'foodexpress.session';
let credentials = null;
try {
	credentials = JSON.parse(sessionStorage.getItem(sessionKey));
} catch {
	/* Storage may be blocked. */
}
export class ApiError extends Error {
	constructor(message, status = 0, errors = {}) {
		super(message);
		this.status = status;
		this.errors = errors;
	}
}
export function clearSession() {
	credentials = null;
	try {
		sessionStorage.removeItem(sessionKey);
	} catch {
		/* Memory-only mode. */
}
}
export function saveSession(data) {
	if (!data?.accessToken || !Number.isFinite(Date.parse(data.expiresAt)) || Date.parse(data.expiresAt) <= Date.now()) {
		throw new ApiError('The server returned an invalid session. Please sign in again.');
	}
	credentials = {
		accessToken: data.accessToken,
		expiresAt: data.expiresAt
	};
	try {
		sessionStorage.setItem(sessionKey, JSON.stringify(credentials));
	} catch {
		/* The session remains usable in memory. */
}
}
export function session() {
	if (!credentials?.accessToken || !Number.isFinite(Date.parse(credentials.expiresAt)) || Date.parse(credentials.expiresAt) <= Date.now()) {
		clearSession();
		return null;
	}
	return credentials;
}

function expire() {
	clearSession();
	window.dispatchEvent(new CustomEvent('session-expired'));
}
export async function request(method, path, {
	query,
	body,
	auth = false,
	signal
} = {}) {
	const current = session();
	if (auth && !current) {
		expire();
		throw new ApiError('Your session has ended. Please sign in again.', 401);
	}
	const url = new URL(config.apiBase.replace(/\/$/, '') + path, window.location.origin);
	for (const [key, value] of Object.entries(query || {})) {
		if (value !== '' && value !== null && value !== undefined) {
			url.searchParams.set(key, String(value));
		}
	}
	const controller = new AbortController();
	const abort = () => controller.abort();
	signal?.addEventListener('abort', abort, {
		once: true
	});
	if (signal?.aborted) {
		controller.abort();
	}
	const timer = setTimeout(abort, config.timeout);
	try {
		const headers = {
			Accept: 'application/json'
		};
		if (body !== undefined && !(body instanceof FormData)) {
			headers['Content-Type'] = 'application/json';
		}
		if (auth && current) {
			headers.Authorization = `Bearer ${current.accessToken}`;
		}
		const response = await fetch(url, {
			method,
			headers,
			body: body instanceof FormData ? body : body === undefined ? undefined : JSON.stringify(body),
			signal: controller.signal,
			cache: 'no-store',
			credentials: 'omit'
		});
		let data = null;
		if (response.status !== 204) {
			const text = await response.text();
			if (text) {
				try {
					data = JSON.parse(text);
				} catch {
					throw new ApiError('The server returned an unreadable response.', response.status);
				}
			}
		}
		if (response.status === 401 && auth) {
			expire();
		}
		if (!response.ok || data?.success === false) {
			const message = data?.detail || data?.message || data?.title || (response.status === 403 ? 'Your account does not have permission to do this.' : `Request failed (${response.status}).`);
			throw new ApiError(message, response.status, data?.errors || {});
		}
		if (response.status === 204) {
			return null;
		}
		if (!data || data.success !== true || !Object.hasOwn(data, 'data')) {
			throw new ApiError('The API response did not match the expected format.', response.status);
		}
		return data.data;
	} catch (error) {
		if (error instanceof ApiError) {
			throw error;
		}
		if (error.name === 'AbortError') {
			throw new ApiError('The request was interrupted or timed out. Refresh to check the latest result before trying again.');
		}
		throw new ApiError('Unable to reach FoodExpress. Check your connection and try again.');
	} finally {
		clearTimeout(timer);
		signal?.removeEventListener('abort', abort);
	}
}
export const api = {
	get(path, options) {
		return request('GET', path, options);
	},
	post(path, body, options = {}) {
		return request('POST', path, {
			...options,
			body
		});
	},
	put(path, body, options = {}) {
		return request('PUT', path, {
			...options,
			body
		});
	},
	patch(path, body, options = {}) {
		return request('PATCH', path, {
			...options,
			body
		});
	},
	delete(path, options) {
		return request('DELETE', path, options);
	}
};
