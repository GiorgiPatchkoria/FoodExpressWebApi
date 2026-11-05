import {
	escape,
	money,
	link,
	badge
} from './utils.js';
import {
	currentUser,
	isAdmin,
	logout
} from './auth.js';
export function shell() {
	const user = currentUser();
	document.querySelector('#header').innerHTML = `<div class="nav-wrap"><a class="brand" href="#/"><img src="/assets/logo.svg" alt="" width="38" height="38">Food<span>Express</span><i class="brand-dot"></i></a><button class="mobile-toggle icon-button" aria-label="Toggle navigation" aria-expanded="false">☰</button><nav class="main-nav" aria-label="Main navigation"><a href="#/restaurants">Discover</a>${user ? `<a href="#/orders">My orders</a><a href="#/addresses">Addresses</a>${isAdmin() ? '<a href="#/admin">Admin studio</a>' : ''}` : '<a href="#/restaurants?isActive=true">Find your next meal</a>'}</nav><div class="nav-account">${user ? `<a class="account-name" href="#/profile">${escape(user.fullName || user.userName)}</a><button class="text-button" id="logout">Log out</button>` : '<a class="text-link" href="/login.html">Log in</a><a class="button small" href="/register.html">Get started</a>'}<a class="cart-link" href="#/cart" aria-label="View shopping cart">Cart<span id="cart-count">0</span></a></div></div>`;
	const toggle = document.querySelector('.mobile-toggle');
	toggle.onclick = () => {
		const opened = document.querySelector('#header').classList.toggle('expanded');
		toggle.setAttribute('aria-expanded', String(opened));
	};
	document.querySelector('#logout')?.addEventListener('click', () => {
		logout();
		location.hash = '#/';
	});
	document.querySelector('#footer').innerHTML = `<div class="footer-inner"><a class="brand" href="#/">Food<span>Express</span></a><p>Good food. Your place.</p><a href="#/restaurants">Explore the menu ↗</a><span>Made for everyday appetites.</span></div>`;
}
export function cartCount(cart) {
	const node = document.querySelector('#cart-count');
	if (node) {
		node.textContent = String(cart?.items?.reduce((sum, item) => sum + item.quantity, 0) || 0);
	}
}
export function restaurantCard(r) {
	return `<article class="restaurant-card"><a class="restaurant-art tone-${r.id % 4}" href="#/restaurant/${r.id}" aria-label="View ${escape(r.name)}">${r.imageUrl ? `<img class="restaurant-photo" src="${escape(r.imageUrl)}" alt="" loading="lazy">` : ''}<span class="art-circle"></span><span class="restaurant-initial">${escape(r.name.slice(0, 1))}</span><span class="art-label">THE NEIGHBORHOOD TABLE</span><span class="open-pill">${r.isActive ? 'Taking orders' : 'Currently closed'}</span></a><div class="card-body"><div class="card-title"><h3><a href="#/restaurant/${r.id}">${escape(r.name)}</a></h3><span aria-hidden="true">↗</span></div><p>${escape(r.city)} <span class="dot">·</span> ${escape(r.streetAddress)}</p><a class="subtle-link" href="#/restaurant/${r.id}">Explore menu <span>→</span></a></div></article>`;
}
export function foodCard(item, canOrder = true) {
	return `<article class="food-card"><div class="food-mark tone-${item.categoryId % 4}" aria-hidden="true">${item.imageUrl ? `<img class="food-photo" src="${escape(item.imageUrl)}" alt="" loading="lazy">` : escape(item.name.slice(0, 1))}</div><div class="food-content"><button type="button" class="title-button dish-details-button" data-item="${item.id}" aria-label="View ${escape(item.name)} details">${escape(item.name)}</button><span class="dish-restaurant">From ${escape(item.restaurantName || 'Restaurant #' + item.restaurantId)}</span><p>${escape(item.description || 'Prepared by the restaurant. Explore the details for more.')}</p><div class="food-bottom"><strong>${money(item.price)}</strong><button class="add-button" data-add="${item.id}" ${!item.isAvailable || !canOrder ? 'disabled' : ''} aria-label="Add ${escape(item.name)} to cart">${item.isAvailable && canOrder ? '+' : 'Unavailable'}</button></div></div></article>`;
}
export function orderTable(rows, admin = false) {
	return `<div class="table-scroll"><table><thead><tr><th>Order</th><th>Placed</th><th>Restaurant</th><th>Status</th><th>Total</th><th><span class="sr-only">Details</span></th></tr></thead><tbody>${rows.map(o => `<tr><td><strong>#${o.id}</strong></td><td>${escape(new Date(o.createdAt).toLocaleDateString())}</td><td><a href="#/restaurant/${o.restaurantId}">Restaurant #${o.restaurantId}</a></td><td>${badge(o.status)}</td><td>${money(o.totalPrice)}</td><td><a class="subtle-link" href="${admin ? '#/admin/order/' : '#/order/'}${o.id}">View →</a></td></tr>`).join('')}</tbody></table></div>`;
}
export function adminLayout(view, section) {
	view.className = 'admin-layout';
	view.innerHTML = `<aside class="admin-sidebar"><p class="eyebrow">FOODEXPRESS STUDIO</p><h2>Make good<br>things happen.</h2><nav aria-label="Administration">${[['', 'Overview', '◈'], ['restaurants', 'Restaurants', '⌂'], ['categories', 'Categories', '▦'], ['menu-items', 'Menu items', '≡'], ['orders', 'Orders', '▤']].map(([path, label, icon]) => `<a class="${section === path ? 'active' : ''}" href="#/admin${path ? '/' + path : ''}"><span aria-hidden="true">${icon}</span>${label}</a>`).join('')}</nav><a class="back-store" href="#/">← Back to storefront</a></aside><section class="admin-content"></section>`;
	return view.querySelector('.admin-content');
}