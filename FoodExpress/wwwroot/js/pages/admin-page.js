import { uploadPicture } from '../services/catalog.js';
import {
	restaurants,
	categories,
	menu,
	allPages
} from '../services/catalog.js';
import {
	orders
} from '../services/orders.js';
import {
	config
} from '../config.js';
import {
	orderTable
} from '../components.js?v=dish-cards-3';
import {
	escape,
	money,
	headerText,
	field,
	checkbox,
	select,
	pageNumber,
	pagination,
	bindForm,
	action,
	notify,
	confirmAction,
	showDialog,
	empty,
	badge,
	statusName
} from '../utils.js';
import {
	menuFilters
} from './catalog-page.js';
export async function dashboard(view, params, ctx) {
	const [summary, recent] = await Promise.all([orders.summary(), orders.all({
		page: 1,
		pageSize: 6
	})]);
	const total = summary.reduce((s, x) => s + x.count, 0);
	const value = summary.reduce((s, x) => s + x.totalPrice, 0);
	const delivered = summary.filter(x => statusName(x.status) === 'Delivered').reduce((s, x) => s + x.totalPrice, 0);
	const active = summary.filter(x => !['Delivered', 'Cancelled'].includes(statusName(x.status))).reduce((s, x) => s + x.count, 0);
	view.innerHTML = headerText('THE BIG PICTURE', 'Your kitchen, connected.', 'A live snapshot of the orders across your restaurants.', '<button class="button secondary" data-refresh>↻ Refresh</button>') + `<div class="stats"><article><span>Total orders</span><strong>${total}</strong><small>All recorded orders</small></article><article><span>In progress</span><strong>${active}</strong><small>Created through on the way</small></article><article><span>Total order value</span><strong>${money(value)}</strong><small>Includes cancelled orders</small></article><article><span>Delivered order value</span><strong>${money(delivered)}</strong><small>Completed deliveries only</small></article></div><section class="panel"><div class="section-heading"><h2>Where orders stand</h2><span class="muted">All time</span></div>${summary.length ? summary.map(x => `<div class="distribution"><span>${badge(x.status)}</span><div class="bar-track"><div style="width:${total ? x.count / total * 100 : 0}%"></div></div><strong>${x.count}</strong><span>${money(x.totalPrice)}</span></div>`).join('') : empty('A fresh start.', 'Order activity will appear here when customers place orders.', null)}</section><section class="panel"><div class="section-heading"><h2>Recent orders</h2><a class="subtle-link" href="#/admin/orders">View all →</a></div>${recent.length ? orderTable(recent, true) : '<p class="muted">No orders yet.</p>'}</section>`;
	view.querySelector('[data-refresh]').onclick = ctx.refresh;
}

function pictureField(item = {}) {
    return `<section class="picture-picker" aria-label="Upload picture">
        <div class="picture-preview"><img data-picture-preview alt="Picture preview" ${item.imageUrl ? `src="${escape(item.imageUrl)}"` : 'hidden'}><span data-picture-placeholder ${item.imageUrl ? 'hidden' : ''}>Your photo goes here</span></div>
        <label class="picture-label">Restaurant or dish picture
            <input type="file" name="picture" accept="image/jpeg,image/png,image/webp">
        </label>
        <p class="picture-help">Choose a JPEG, PNG or WebP image, up to 5 MB. It will upload when you save.</p>
        <p data-picture-status role="status" class="picture-help">${item.imageUrl ? 'Current picture. Choose a file to replace it.' : 'Adding a picture is optional.'}</p>
        <button type="button" class="text-button" data-picture-clear hidden>Clear selection</button>
    </section>`;
}
function bindPicturePicker(dialog, item = {}) {
    const input = dialog.querySelector('[name=picture]');
    const preview = dialog.querySelector('[data-picture-preview]');
    const placeholder = dialog.querySelector('[data-picture-placeholder]');
    const status = dialog.querySelector('[data-picture-status]');
    const clear = dialog.querySelector('[data-picture-clear]');
    let previewUrl;
    function resetPreview() {
        if (previewUrl) {
        	URL.revokeObjectURL(previewUrl);
        }
        previewUrl = null;
        preview.hidden = !item.imageUrl;
        placeholder.hidden = !!item.imageUrl;
        if (item.imageUrl) {
        	preview.src = item.imageUrl;
        }
        else {
        	preview.removeAttribute('src');
        }
        clear.hidden = true;
    }
    input.addEventListener('change', () => {
        resetPreview();
        input.setCustomValidity('');
        const file = input.files[0];
        if (!file) { status.textContent = 'No new picture selected.'; return; }
        const error = pictureError(file);
        if (error) {
            input.setCustomValidity(error);
            status.textContent = error;
            clear.hidden = false;
            input.reportValidity();
            return;
        }
        previewUrl = URL.createObjectURL(file);
        preview.src = previewUrl;
        preview.hidden = false;
        placeholder.hidden = true;
        clear.hidden = false;
        status.textContent = file.name + ' — ready to upload';
    });
    clear.onclick = () => {
        input.value = '';
        input.setCustomValidity('');
        resetPreview();
        status.textContent = item.imageUrl ? 'Current picture will be kept.' : 'No picture selected.';
    };
    dialog.addEventListener('close', () => { if (previewUrl) {
    	URL.revokeObjectURL(previewUrl);
    } }, { once: true });
}
function pictureError(file) {
    if (!file) {
    	return '';
    }
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
    	return 'Choose a JPEG, PNG or WebP image.';
    }
    if (!file.size || file.size > 5 * 1024 * 1024) {
    	return 'Choose a nonempty image up to 5 MB.';
    }
    return '';
}
async function savePicture(dialog, resource, id, picture) {
    if (!picture) {
    	return;
    }
    dialog.querySelector('[type=submit]').textContent = 'Uploading picture…';
    try { await uploadPicture(resource, id, picture); }
    catch (error) { throw new Error('Details saved, but the picture could not be uploaded. Select a picture and save again to retry. ' + error.message); }
}
function restaurantForm(r = {}) {
	return `<form class="stack-form"><div data-errors></div>${pictureField(r)}${field('name', 'Restaurant name', { value: r.name, max: 150 })}${field('streetAddress', 'Street address', { value: r.streetAddress, max: 250 })}${field('city', 'City', { value: r.city, max: 100 })}${field('phoneNumber', 'Phone number', { value: r.phoneNumber, type: 'tel', max: 30 })}${checkbox('isActive', 'Taking orders', r.isActive ?? true)}<button type="submit" class="button">Save restaurant</button></form>`;
}
export async function manageRestaurants(view, params, ctx) {
	const query = {
		search: params.get('search') || '',
		city: params.get('city') || '',
		isActive: params.get('isActive') ?? '',
		page: pageNumber(params),
		pageSize: config.pageSize
	};
	const rows = await restaurants.list(query);
	view.innerHTML = headerText('YOUR RESTAURANTS', 'Good kitchens start here.', 'Create and manage the places your customers love.', '<button class="button" data-create>+ Add restaurant</button>') + `<form class="filter-bar">${field('search', 'Name', { value: query.search, required: false, max: 150 })}${field('city', 'City', { value: query.city, required: false, max: 100 })}${select('isActive', 'Availability', [['', 'All'], ['true', 'Active'], ['false', 'Inactive']], query.isActive)}<button class="button secondary" type="submit">Search</button></form><div class="panel table-scroll">${rows.length ? `<table><thead><tr><th>Restaurant</th><th>Location</th><th>Status</th><th>Actions</th></tr></thead><tbody>${rows.map(r => `<tr><td>${r.imageUrl ? `<img class="catalog-thumbnail" src="${escape(r.imageUrl)}" alt="" loading="lazy">` : ''}<strong>${escape(r.name)}</strong><small>${escape(r.phoneNumber)}</small></td><td>${escape(r.city)}<small>${escape(r.streetAddress)}</small></td><td><span class="badge ${r.isActive ? 'status-Delivered' : 'status-Cancelled'}">${r.isActive ? 'Active' : 'Inactive'}</span></td><td><div class="row-actions"><a href="#/restaurant/${r.id}">View</a><a href="#/admin/menu-items?restaurantId=${r.id}">Menu</a><button class="text-button" data-edit="${r.id}">Edit</button><button class="text-button danger-text" data-delete="${r.id}">Delete</button></div></td></tr>`).join('')}</tbody></table>` : empty('Add your first restaurant.', 'Give your next great kitchen a home.', null)}</div>${pagination('/admin/restaurants', query, rows.length)}`;
	bindForm(view.querySelector('.filter-bar'), data => ctx.go('/admin/restaurants', data));
	async function edit(id) {
		const data = id ? await restaurants.get(id) : {};
		const dialog = showDialog(id ? 'Edit restaurant' : 'Add restaurant', restaurantForm(data));
        bindPicturePicker(dialog, data);
		bindForm(dialog.querySelector('form'), async body => {
			const picture = dialog.querySelector('[name=picture]').files[0];
            delete body.picture;
            const error = pictureError(picture);
            if (error) {
            	throw new Error(error);
            }
            const saved = id ? await restaurants.update(id, body) : await restaurants.create(body);
            id = saved.id;
            await savePicture(dialog, 'restaurants', id, picture);
			dialog.close();
			notify('Restaurant saved.');
			ctx.refresh();
		});
	}
	view.querySelector('[data-create]').onclick = event => action(event.currentTarget, () => edit());
	view.querySelectorAll('[data-edit]').forEach(b => b.onclick = () => action(b, () => edit(b.dataset.edit)));
	view.querySelectorAll('[data-delete]').forEach(b => b.onclick = () => action(b, async () => {
		if (!await confirmAction('Delete this restaurant?', 'This cannot be undone. Restaurants referenced by menu items or orders may not be deleted.')) {
			return;
		}
		await restaurants.remove(b.dataset.delete);
		notify('Restaurant deleted.');
		ctx.refresh();
	}));
}
export async function manageCategories(view, params, ctx) {
	const query = {
		page: pageNumber(params),
		pageSize: config.pageSize
	};
	const rows = await categories.list(query);
	view.innerHTML = headerText('ORGANIZE THE MENU', 'A place for every flavor.', 'Categories help customers discover what they are craving.', '<button class="button" data-create>+ Add category</button>') + `<div class="category-grid">${rows.length ? rows.map(c => `<article class="panel"><p class="eyebrow">CATEGORY #${c.id}</p><h2>${escape(c.name)}</h2><button class="text-button" data-view="${c.id}">View details →</button></article>`).join('') : empty('No categories yet.', 'Add one to organize your menu.', null)}</div>${pagination('/admin/categories', query, rows.length)}`;
	view.querySelector('[data-create]').onclick = () => {
		const dialog = showDialog('Add a category', `<form class="stack-form"><div data-errors></div>${field('name', 'Category name', { max: 100 })}<button type="submit" class="button">Save category</button></form>`);
		bindForm(dialog.querySelector('form'), async body => {
			await categories.create(body);
			dialog.close();
			notify('Category created.');
			ctx.refresh();
		});
	};
	view.querySelectorAll('[data-view]').forEach(b => b.onclick = () => action(b, async () => {
		const c = await categories.get(b.dataset.view);
		showDialog(c.name, `<p>Category #${c.id}</p><a class="button" href="#/category/${c.id}">Explore this category →</a>`);
	}));
}
export async function manageMenu(view, params, ctx) {
	const [rests, cats] = await Promise.all([allPages(restaurants.list), allPages(categories.list)]);
	const restaurantId = Number(params.get('restaurantId')) || rests[0]?.id;
	if (!restaurantId) {
		view.innerHTML = headerText('MENU MANAGEMENT', 'First, open a kitchen.') + empty('No restaurants yet.', 'Create a restaurant before adding its menu.', '#/admin/restaurants', 'Manage restaurants');
		return;
	}
	const query = {
		...Object.fromEntries(params),
		restaurantId,
		page: pageNumber(params),
		pageSize: config.pageSize
	};
	const rows = await menu.list(query);
	view.innerHTML = headerText('MENU MANAGEMENT', 'Make the menu yours.', 'Keep dishes, prices, and availability up to date.', '<button class="button" data-create>+ Add menu item</button>') + menuFilters(query, select('restaurantId', 'Restaurant', rests.map(r => [r.id, r.name]), restaurantId)) + `<div class="panel table-scroll">${rows.length ? `<table><thead><tr><th>Dish</th><th>Category</th><th>Price</th><th>Availability</th><th>Actions</th></tr></thead><tbody>${rows.map(item => `<tr><td>${item.imageUrl ? `<img class="catalog-thumbnail" src="${escape(item.imageUrl)}" alt="" loading="lazy">` : ''}<strong>${escape(item.name)}</strong><small>${escape(item.description || '')}</small></td><td>${escape(cats.find(c => c.id === item.categoryId)?.name || 'Category #' + item.categoryId)}</td><td>${money(item.price)}</td><td><span class="badge ${item.isAvailable ? 'status-Delivered' : 'status-Cancelled'}">${item.isAvailable ? 'Available' : 'Unavailable'}</span></td><td><div class="row-actions"><button class="text-button" data-edit="${item.id}">Edit</button><button class="text-button danger-text" data-delete="${item.id}">Delete</button></div></td></tr>`).join('')}</tbody></table>` : empty('A blank menu, endless possibilities.', 'Add a dish or change the filters.', null)}</div>${pagination('/admin/menu-items', query, rows.length)}`;
	view.querySelector('#category-filter').innerHTML = select('categoryId', 'Category', [
		['', 'All categories'], ...cats.map(c => [c.id, c.name])
	], query.categoryId || '');
	bindForm(view.querySelector('.filter-bar'), data => {
		if (data.minPrice !== '' && data.maxPrice !== '' && Number(data.minPrice) > Number(data.maxPrice)) {
			throw new Error('Minimum price cannot exceed maximum price.');
		}
		ctx.go('/admin/menu-items', data);
	});
	async function edit(id) {
		if (!cats.length) {
			notify('Create a category before adding a menu item.', true);
			return;
		}
		const item = id ? await menu.get(id) : {
			restaurantId,
			isAvailable: true
		};
		const dialog = showDialog(id ? 'Edit menu item' : 'Add menu item', `<form class="stack-form"><div data-errors></div>${id ? `<p class="muted">Restaurant: ${escape(rests.find(r => r.id === item.restaurantId)?.name || item.restaurantId)}</p>` : select('restaurantId', 'Restaurant', rests.map(r => [r.id, r.name]), item.restaurantId)}${pictureField(item)}${field('name', 'Dish name', { value: item.name, max: 150 })}<label>Description (optional)<textarea name="description" maxlength="2000" rows="3">${escape(item.description || '')}</textarea></label>${select('categoryId', 'Category', cats.map(c => [c.id, c.name]), item.categoryId || cats[0].id)}${field('price', 'Price', { type: 'number', value: item.price ?? '', min: 0, max: 999999.99, step: '.01' })}${checkbox('isAvailable', 'Available to order', item.isAvailable)}<button class="button" type="submit">Save menu item</button></form>`);
		bindPicturePicker(dialog, item);
		bindForm(dialog.querySelector('form'), async data => {
			data.price = Number(data.price);
			data.categoryId = Number(data.categoryId);
			data.description = data.description.trim() || null;
			const picture = dialog.querySelector('[name=picture]').files[0];
            delete data.picture;
            const error = pictureError(picture);
            if (error) {
            	throw new Error(error);
            }
            if (id) {
            	await menu.update(id, data);
            }
			else {
				data.restaurantId = Number(data.restaurantId);
				const saved = await menu.create(data);
                id = saved.id;
			}
			await savePicture(dialog, 'menu-items', id, picture);
            dialog.close();
            notify('Menu item saved.');
			ctx.refresh();
		});
	}
	view.querySelector('[data-create]').onclick = event => action(event.currentTarget, () => edit());
	view.querySelectorAll('[data-edit]').forEach(b => b.onclick = () => action(b, () => edit(b.dataset.edit)));
	view.querySelectorAll('[data-delete]').forEach(b => b.onclick = () => action(b, async () => {
		if (!await confirmAction('Delete this menu item?', 'This cannot be undone. Items referenced by carts or orders may not be deleted; you can mark them unavailable instead.')) {
			return;
		}
		await menu.remove(b.dataset.delete);
		notify('Menu item deleted.');
		ctx.refresh();
	}));
}