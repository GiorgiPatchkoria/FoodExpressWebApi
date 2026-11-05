// Use the same origin as the API. Change only for a deployment with an explicitly configured CORS policy.
export const config = Object.freeze({
	apiBase: '/api',
	currency: 'GEL',
	locale: 'en-GB',
	pageSize: 12,
	timeout: 20000
});
export const photos = Object.freeze({
	hero: 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=1200&q=85',
	dining: 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?auto=format&fit=crop&w=900&q=80'
});