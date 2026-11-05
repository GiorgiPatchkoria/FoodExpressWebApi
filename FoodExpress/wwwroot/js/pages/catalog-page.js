import {
	restaurants,
	categories,
	menu,
	allPages
} from '../services/catalog.js';
import {
	cart
} from '../services/orders.js';
import {
	currentUser
} from '../auth.js';
import {
	photos,
	config
} from '../config.js';
import {
	restaurantCard,
	foodCard,
	cartCount
} from '../components.js?v=dish-cards-3';
import {
	escape,
	money,
	headerText,
	field,
	select,
	pageNumber,
	pagination,
	link,
	bindForm,
	action,
	notify,
	showDialog,
	empty,
	problem
} from '../utils.js';
export function bindFood(view, ctx) {
	view.querySelectorAll('[data-add]').forEach(button => button.addEventListener('click', () => action(button, async () => {
		if (!currentUser()) {
			ctx.go('/login', {
				next: location.hash.slice(1)
			});
			return;
		}
		const result = await cart.add(Number(button.dataset.add));
		cartCount(result);
		notify('Added to your Cart.');
	})));
	view.querySelectorAll('[data-item]').forEach(button => button.addEventListener('click', () => action(button, async () => {
		const item = await menu.get(button.dataset.item);
		showDialog(item.name, `<p>${escape(item.description || 'No description provided by the restaurant.')}</p><p class="price">${money(item.price)}</p><p>${item.isAvailable ? 'Available' : 'Currently unavailable'}</p><a class="button" href="#/restaurant/${item.restaurantId}">View restaurant menu</a>`);
	})));
}
export async function home(view, params, ctx) {
	view.innerHTML = `<section class="hero"><div class="hero-copy"><p class="eyebrow"><span class="tiny-line"></span> GOOD FOOD. YOUR PLACE.</p><h1>Your next<br>favorite meal,<br><em>delivered.</em></h1><p>From the neighborhood kitchens you love<br class="desktop"> to the flavors you haven’t met yet.</p><form id="hero-search" class="hero-search"><label class="sr-only" for="hero-query">Search restaurants</label><span aria-hidden="true">⌕</span><input id="hero-query" name="search" placeholder="What are you craving?" maxlength="150"><button class="button" type="submit">Find my food →</button></form><div class="hero-note"><span class="little-star">✳</span> A fresh way to find your favorites.</div></div><div class="hero-image"><img src="${photos.hero}" alt="A colorful fresh bowl with vegetables and grains" fetchpriority="high"><div class="image-label"><span class="label-star">✳</span><div><strong>Made with appetite.</strong><small>Something good is on the menu.</small></div></div><span class="hero-ring" aria-hidden="true">FRESH FINDS · EVERY DAY ·</span></div></section><section class="section"><div class="section-heading"><div><p class="eyebrow">FOLLOW YOUR APPETITE</p><h2>What sounds good?</h2></div><a class="subtle-link" href="#/restaurants">Explore restaurants →</a></div><div id="home-categories" class="category-strip"></div></section><section class="section"><div class="section-heading"><div><p class="eyebrow">LOCAL KITCHENS, BIG FLAVORS</p><h2>Meet your next favorite.</h2></div><a class="subtle-link" href="#/restaurants">View all restaurants →</a></div><div id="home-restaurants" class="card-grid"></div></section><section class="editorial"><div><p class="eyebrow">A GOOD DAY STARTS WITH GOOD FOOD</p><h2>Less deciding.<br>More <em>delighting.</em></h2><p>Find a kitchen. Fill your Cart.<br>We’ll take it from there.</p><a class="button dark" href="#/restaurants">Find something delicious →</a></div><img src="${photos.dining}" alt="A warmly lit restaurant dining room" loading="lazy"></section><section class="section"><div class="section-heading"><div><p class="eyebrow">A TASTE OF WHAT’S COOKING</p><h2>On the menu.</h2></div></div><div id="home-food" class="food-grid"></div></section>`;
	bindForm(view.querySelector('#hero-search'), data => ctx.go('/restaurants', data));
	const results = await Promise.allSettled([allPages(categories.list), restaurants.list({
		page: 1,
		pageSize: 6,
		isActive: true
	})]);
	const cats = view.querySelector('#home-categories');
	const rests = view.querySelector('#home-restaurants');
	const foods = view.querySelector('#home-food');
	if (results[0].status === 'fulfilled') {
		cats.innerHTML = results[0].value.length ? results[0].value.map((c, i) => `<a class="category-tile" href="#/category/${c.id}"><span class="category-icon tone-${i % 4}" aria-hidden="true">${['◒', '✳', '◈', '◓'][i % 4]}</span><strong>${escape(c.name)}</strong><span>Explore →</span></a>`).join('') : '<p class="muted">Categories will appear here when available.</p>';
	} else {
		cats.innerHTML = problem(results[0].reason);
	}
	if (results[1].status === 'fulfilled') {
		const rows = results[1].value;
		rests.innerHTML = rows.length ? rows.map(restaurantCard).join('') : empty('The kitchens are getting ready.', 'No active restaurants are available yet.', null);
    } else {
        rests.innerHTML = problem(results[1].reason);
    }
    if (results[0].status !== 'fulfilled') {
        foods.innerHTML = problem(results[0].reason);
        return;
    }
    try {
        const includedCategories = new Set(['main dishes']);
        const foodCategories = results[0].value.filter(category => includedCategories.has(category.name.trim().toLowerCase()));
        const groups = await Promise.all(foodCategories.map(category => allPages(menu.list, { categoryId: category.id })));
        const dishes = groups.flat().sort((a, b) => a.name.localeCompare(b.name) || a.id - b.id);
        foods.innerHTML = dishes.length ? dishes.map(item => foodCard(item)).join('') : empty('Fresh finds coming soon.', 'Main dishes will appear here when added.', null);
        bindFood(foods, ctx);
    } catch (error) {
        foods.innerHTML = problem(error);
    }
}

export async function restaurantList(view, params, ctx) {
	const query = {
		search: params.get('search') || '',
		city: params.get('city') || '',
		isActive: params.get('isActive') ?? 'true',
		page: pageNumber(params),
		pageSize: config.pageSize
	};
	const rows = await restaurants.list(query);
	view.innerHTML = headerText('FIND YOUR FLAVOR', 'Good food, close to you.', 'Discover the restaurants ready to make your day.') + `<form class="filter-bar">${field('search', 'Restaurant name', { value: query.search, required: false, max: 150 })}${field('city', 'City', { value: query.city, required: false, max: 100 })}${select('isActive', 'Availability', [['true', 'Taking orders'], ['', 'All restaurants'], ['false', 'Currently closed']], query.isActive)}<button class="button" type="submit">Find restaurants</button></form><div class="card-grid">${rows.length ? rows.map(restaurantCard).join('') : empty('No tables on this street.', 'Try another name, city, or availability filter.', null)}</div>${pagination('/restaurants', query, rows.length)}`;
	bindForm(view.querySelector('form'), data => ctx.go('/restaurants', data));
}
export function menuFilters(query, extra = '') {
	return `<form class="filter-bar menu-filters">${extra}${field('search', 'Find a dish', { value: query.search || '', required: false, max: 150 })}<div id="category-filter"></div>${field('minPrice', 'Min price', { type: 'number', value: query.minPrice || '', min: 0, max: 999999.99, step: '.01', required: false })}${field('maxPrice', 'Max price', { type: 'number', value: query.maxPrice || '', min: 0, max: 999999.99, step: '.01', required: false })}${select('sort', 'Sort by', [['name', 'Name'], ['price', 'Price: low to high'], ['price-desc', 'Price: high to low']], query.sort || 'name')}${select('isAvailable', 'Availability', [['', 'All dishes'], ['true', 'Available'], ['false', 'Unavailable']], query.isAvailable ?? '')}<button class="button" type="submit">Apply filters</button><div data-errors></div></form>`;
}
export async function restaurantDetail(view, params, ctx, id) {
	const query = {
		...Object.fromEntries(params),
		page: pageNumber(params),
		pageSize: config.pageSize
	};
	const [restaurant, rows, cats] = await Promise.all([restaurants.get(id), menu.forRestaurant(id, query), allPages(categories.list)]);
	view.innerHTML = `<a class="subtle-link" href="#/restaurants">← All restaurants</a><section class="restaurant-banner tone-${Number(id) % 4}"><div><p class="eyebrow">${escape(restaurant.city)}</p><h1>${escape(restaurant.name)}</h1><p>${escape(restaurant.streetAddress)} · ${escape(restaurant.phoneNumber)}</p><span class="badge">${restaurant.isActive ? 'Taking orders' : 'Currently closed'}</span></div><div class="banner-symbol" aria-hidden="true">✳</div></section><div class="section-heading"><h2>Made for your appetite.</h2><a href="#/cart" class="subtle-link">View your Cart →</a></div>${menuFilters(query)}<div class="food-grid">${rows.length ? rows.map(x => foodCard(x, restaurant.isActive)).join('') : empty('Nothing on this menu yet.', 'Try changing your filters.', null)}</div>${pagination('/restaurant/' + id, query, rows.length)}`;
	view.querySelector('#category-filter').innerHTML = select('categoryId', 'Category', [
		['', 'All categories'], ...cats.map(c => [c.id, c.name])
	], query.categoryId || '');
	bindForm(view.querySelector('form'), data => {
		if (data.minPrice !== '' && data.maxPrice !== '' && Number(data.minPrice) > Number(data.maxPrice)) {
			throw new Error('Minimum price cannot exceed maximum price.');
		}
		ctx.go('/restaurant/' + id, data);
	});
	bindFood(view, ctx);
}
export async function categoryDetail(view, params, ctx, id) {
	const category = await categories.get(id);
	const rows = await restaurants.list({
		page: pageNumber(params),
		pageSize: config.pageSize,
		isActive: true
	});
	view.innerHTML = headerText('EXPLORE A CATEGORY', category.name, 'Choose a restaurant to see its dishes in this category.') + `<div class="card-grid">${rows.length ? rows.map(r => restaurantCard(r).replaceAll('#/restaurant/' + r.id, link('/restaurant/' + r.id, { categoryId: id }))).join('') : empty('No active restaurants.', 'Please check again later.', null)}</div>${pagination('/category/' + id, { page: pageNumber(params) }, rows.length)}`;
}