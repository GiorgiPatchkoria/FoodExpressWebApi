import {
	api
} from '../api.js';
export const restaurants = {
	list(query) {
		return api.get('/restaurants', {
			query
		});
	},
	get(id) {
		return api.get(`/restaurants/${encodeURIComponent(id)}`);
	},
	create(body) {
		return api.post('/restaurants', body, {
			auth: true
		});
	},
	update(id, body) {
		return api.put(`/restaurants/${encodeURIComponent(id)}`, body, {
			auth: true
		});
	},
	remove(id) {
		return api.delete(`/restaurants/${encodeURIComponent(id)}`, {
			auth: true
		});
	}
};
export const menu = {
	list(query) {
		return api.get('/menu-items', {
			query
		});
	},
	forRestaurant(id, query) {
		return api.get(`/restaurants/${encodeURIComponent(id)}/menu`, {
			query
		});
	},
	get(id) {
		return api.get(`/menu-items/${encodeURIComponent(id)}`);
	},
	create(body) {
		return api.post('/menu-items', body, {
			auth: true
		});
	},
	update(id, body) {
		return api.put(`/menu-items/${encodeURIComponent(id)}`, body, {
			auth: true
		});
	},
	remove(id) {
		return api.delete(`/menu-items/${encodeURIComponent(id)}`, {
			auth: true
		});
	}
};
export const categories = {
	list(query) {
		return api.get('/categories', {
			query
		});
	},
	get(id) {
		return api.get(`/categories/${encodeURIComponent(id)}`);
	},
	create(body) {
		return api.post('/categories', body, {
			auth: true
		});
	}
};
export async function allPages(loader, query = {}) {
	const results = [];
	for (let page = 1; page <= 10000; page++) {
		const rows = await loader({
			...query,
			page,
			pageSize: 100
		});
		results.push(...rows);
		if (rows.length < 100) {
			return results;
		}
	}
	throw new Error('Too many results. Narrow your selection.');
}
export async function uploadPicture(resource, id, file) {
    if (!file || !file.size) {
    	return;
    }
    if (file.size > 5 * 1024 * 1024) {
    	throw new Error('Images must be 5 MB or smaller.');
    }
    const form = new FormData();
    form.append('image', file);
    return api.post('/' + resource + '/' + id + '/image', form, { auth: true });
}
