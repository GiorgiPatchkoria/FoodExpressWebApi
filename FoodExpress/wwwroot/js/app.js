import { recoveryPage } from './pages/recovery-page.js';
import {
	initialize,
	currentUser,
	isAdmin
} from './auth.js';
import {
	session
} from './api.js';
import {
	shell,
	adminLayout,
	cartCount
} from './components.js?v=dish-cards-3';
import {
	cart
} from './services/orders.js';
import {
	loading,
	problem,
	empty,
	link,
	notify
} from './utils.js';
import {
	home,
	restaurantList,
	restaurantDetail,
	categoryDetail
} from './pages/catalog-page.js?v=dish-cards-3';
import {
	authPage
} from './pages/auth-page.js';
import {
	addressPage,
	profilePage
} from './pages/account-page.js';
import {
	cartPage,
	checkoutPage
} from './pages/cart-page.js';
import {
	orderList,
	orderDetail
} from './pages/orders-page.js';
import {
	dashboard,
	manageRestaurants,
	manageCategories,
	manageMenu
} from './pages/admin-page.js';
let version = 0;
const main = document.querySelector('#main');
const context = {
	go(path, query = {}) {
		const destination = link(path, query);
		if (location.hash === destination) {
			render();
		}
		else {
			location.hash = destination;
		}
	},
	refresh() {
		return render();
	}
};
async function render() {
	const current = ++version;
	document.querySelector('#dialog').close();
	document.querySelector('#header').classList.remove('expanded');
	const route = location.hash.slice(1) || document.body.dataset.entry || '/';
	const [path, search = ''] = route.split('?');
	const params = new URLSearchParams(search);
	const admin = path === '/admin' || path.startsWith('/admin/');
	const protectedPage = admin || ['/cart', '/checkout', '/orders', '/addresses', '/profile'].includes(path) || path.startsWith('/order/');
	main.innerHTML = loading();
	main.setAttribute('aria-busy', 'true');
	try {
		if (protectedPage) {
			if (!session()) {
				context.go('/login', {
					next: route
				});
				return;
			}
			if (!currentUser() || admin) {
				await initialize();
			}
			if (!currentUser()) {
				context.go('/login', {
					next: route
				});
				return;
			}
			if (admin && !isAdmin()) {
				main.innerHTML = empty('This table is reserved.', 'Your account does not have administrator access.', '#/', 'Back to FoodExpress');
				return;
			}
		}
		if (current !== version) {
			return;
		}
		const view = document.createElement('div');
		view.className = 'page-container';
		let target = view;
		if (admin) {
			target = adminLayout(view, path.split('/')[2] || '');
		}
		const id = path.split('/').at(-1);
		if (path === '/') {
			await home(target, params, context);
		}
		else if (path === '/login' || path === '/register') {
			await authPage(target, params, context, path.slice(1));
		}
		else if (path === '/forgot-password' || path === '/reset-password') {
			await recoveryPage(target, params, context, path === '/reset-password');
		}
		else if (path === '/restaurants') {
			await restaurantList(target, params, context);
		}
		else if (/^\/restaurant\/\d+$/.test(path)) {
			await restaurantDetail(target, params, context, id);
		}
		else if (/^\/category\/\d+$/.test(path)) {
			await categoryDetail(target, params, context, id);
		}
		else if (path === '/cart') {
			await cartPage(target, params, context);
		}
		else if (path === '/checkout') {
			await checkoutPage(target, params, context);
		}
		else if (path === '/addresses') {
			await addressPage(target, params, context);
		}
		else if (path === '/profile') {
			await profilePage(target, params, context);
		}
		else if (path === '/orders') {
			await orderList(target, params, context);
		}
		else if (/^\/order\/\d+$/.test(path)) {
			await orderDetail(target, params, context, id);
		}
		else if (path === '/admin') {
			await dashboard(target, params, context);
		}
		else if (path === '/admin/restaurants') {
			await manageRestaurants(target, params, context);
		}
		else if (path === '/admin/categories') {
			await manageCategories(target, params, context);
		}
		else if (path === '/admin/menu-items') {
			await manageMenu(target, params, context);
		}
		else if (path === '/admin/orders') {
			await orderList(target, params, context, true);
		}
		else if (/^\/admin\/order\/\d+$/.test(path)) {
			await orderDetail(target, params, context, id, true);
		}
		else {
			target.innerHTML = empty('This page wandered off.', 'Let’s find you something delicious.', '#/', 'Back home');
		}
		if (current !== version) {
			return;
		}
		main.replaceChildren(view);
		document.title = (view.querySelector('h1')?.textContent || 'Discover') + ' — FoodExpress';
		main.focus({
			preventScroll: true
		});
		window.scrollTo({
			top: 0,
			behavior: 'instant'
		});
	} catch (error) {
		if (current !== version) {
			return;
		}
		main.innerHTML = `<div class="page-container">${problem(error)}<div class="actions"><button class="button" id="retry">Try again</button><a href="#/" class="button secondary">Back home</a></div></div>`;
		main.querySelector('#retry').onclick = render;
	} finally {
		if (current === version) {
			main.removeAttribute('aria-busy');
		}
	}
}
window.addEventListener('hashchange', render);
window.addEventListener('auth-changed', () => {
	shell();
	if (currentUser()) {
		cart.get().then(cartCount).catch(() => { });
	}
});
window.addEventListener('session-expired', () => {
	shell();
	notify('Your session has ended. Please sign in again.', true);
	if (!location.hash.startsWith('#/login')) {
		context.go('/login');
	}
});
document.addEventListener('click', event => {
	if (event.target.closest('dialog a')) {
		document.querySelector('#dialog').close();
	}
});
shell();
try {
	await initialize();
} catch (error) {
	notify(error.message, true);
}
await render();