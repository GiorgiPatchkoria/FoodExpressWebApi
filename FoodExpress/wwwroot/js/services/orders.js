import {
	api
} from '../api.js';
export const cart = {
	get() {
		return api.get('/cart', {
			auth: true
		});
	},
	add(menuItemId, quantity = 1) {
		return api.post('/cart/items', {
			menuItemId,
			quantity
		}, {
			auth: true
		});
	},
	update(id, quantity) {
		return api.put(`/cart/items/${encodeURIComponent(id)}`, {
			quantity
		}, {
			auth: true
		});
	},
	remove(id) {
		return api.delete(`/cart/items/${encodeURIComponent(id)}`, {
			auth: true
		});
	}
};
export const addresses = {
	list() {
		return api.get('/addresses', {
			auth: true
		});
	},
	create(body) {
		return api.post('/addresses', body, {
			auth: true
		});
	}
};
export const orders = {
	create(addressId) {
		return api.post('/orders', {
			addressId
		}, {
			auth: true
		});
	},
	mine(query) {
		return api.get('/orders/my', {
			query,
			auth: true
		});
	},
	all(query) {
		return api.get('/orders', {
			query,
			auth: true
		});
	},
	get(id) {
		return api.get(`/orders/${encodeURIComponent(id)}`, {
			auth: true
		});
	},
	cancel(id) {
		return api.patch(`/orders/${encodeURIComponent(id)}/cancel`, undefined, {
			auth: true
		});
	},
	status(id, status) {
		return api.patch(`/orders/${encodeURIComponent(id)}/status`, {
			status
		}, {
			auth: true
		});
	},
	summary() {
		return api.get('/orders/summary', {
			auth: true
		});
	}
};