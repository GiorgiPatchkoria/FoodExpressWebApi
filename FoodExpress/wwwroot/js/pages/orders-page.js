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
	date,
	headerText,
	empty,
	pagination,
	pageNumber,
	badge,
	statusName,
	statuses,
	action,
	confirmAction,
	notify,
	select,
	bindForm
} from '../utils.js';
export async function orderList(view, params, ctx, admin = false) {
	const query = {
		page: pageNumber(params),
		pageSize: config.pageSize
	};
	const rows = await (admin ? orders.all(query) : orders.mine(query));
	const status = params.get('status') || '';
	const filtered = status ? rows.filter(o => statusName(o.status) === status) : rows;
	view.innerHTML = headerText(admin ? 'ORDER MANAGEMENT' : 'YOUR FAVORITE MOMENTS', admin ? 'Every order, in view.' : 'Good food. Good memories.', admin ? 'Manage orders and keep your customers in the loop.' : 'Follow what’s cooking and revisit your past orders.', '<button class="button secondary" data-refresh>↻ Refresh</button>') + (admin ? `<form class="filter-bar">${select('status', 'Filter this page by status', [['', 'All statuses'], ...statuses.map(s => [s, s])], status)}<button class="button" type="submit">Filter page</button><p class="field-help">Filters the current page of results.</p></form>` : '') + (filtered.length ? orderTable(filtered, admin) : empty('No orders to show.', admin ? 'Try another page or status filter.' : 'Your next great meal is a few clicks away.', admin ? null : '#/restaurants')) + pagination(admin ? '/admin/orders' : '/orders', {
		...query,
		status
	}, rows.length);
	view.querySelector('[data-refresh]').onclick = ctx.refresh;
	if (admin) {
		bindForm(view.querySelector('form'), data => ctx.go('/admin/orders', {
			...query,
			...data
		}));
	}
}
export async function orderDetail(view, params, ctx, id, admin = false) {
	const details = await orders.get(id);
	const order = details.order;
	const status = statusName(order.status);
	const address = details.deliveryAddress;
	const next = {
		Created: 'Confirmed',
		Confirmed: 'Preparing',
		Preparing: 'OnTheWay',
		OnTheWay: 'Delivered'
	}[status];
	const cancellable = ['Created', 'Confirmed'].includes(status);
	view.innerHTML = `<a class="subtle-link" href="${admin ? '#/admin/orders' : '#/orders'}">← All orders</a>` + headerText(params.get('confirmed') ? 'THANK YOU. YOUR ORDER IS IN.' : 'FROM KITCHEN TO YOUR DOOR', 'Order #' + order.id, date(order.createdAt), '<button class="button secondary" data-refresh>↻ Refresh status</button>') + `<div class="panel tracking"><div class="section-heading"><h2>${status === 'Delivered' ? 'Enjoy every bite.' : status === 'Cancelled' ? 'This order was cancelled.' : 'Good things take a little time.'}</h2>${badge(status)}</div><ol class="timeline">${statuses.filter(s => s !== 'Cancelled').map((s, i) => `<li class="${status !== 'Cancelled' && statuses.indexOf(status) >= i ? 'complete' : ''}"><span>${i + 1}</span>${s === 'OnTheWay' ? 'On the way' : s}</li>`).join('')}</ol></div><div class="two-column"><section class="panel"><h2>What’s in your order</h2>${details.items.map(item => `<div class="order-line"><div><strong>${escape(item.name)}</strong><p>${item.quantity} × ${money(item.unitPrice)}</p></div><strong>${money(item.lineTotal)}</strong></div>`).join('')}<div class="summary-row"><span>Subtotal</span><strong>${money(order.subtotal)}</strong></div><div class="summary-row total"><span>Total</span><strong>${money(order.totalPrice)}</strong></div></section><aside><div class="panel"><p class="eyebrow">DELIVERY ADDRESS</p><h2>${escape(address.streetAddress)}</h2><p>${escape(address.city)} ${escape(address.postalCode || '')}<br>${escape(address.countryCode)}</p><a class="subtle-link" href="#/restaurant/${order.restaurantId}">Visit restaurant →</a></div><div class="actions">${admin && next ? `<button class="button" data-next="${next}">Mark ${next === 'OnTheWay' ? 'on the way' : next.toLowerCase()}</button>` : ''}${cancellable ? '<button class="button secondary danger-text" data-cancel>Cancel order</button>' : ''}</div></aside></div>`;
	view.querySelector('[data-refresh]').onclick = ctx.refresh;
	const change = async (b, cancel) => {
		if (!await confirmAction(cancel ? 'Cancel this order?' : 'Update order status?', cancel ? 'The restaurant will stop processing this order.' : 'Confirm the next step in this order’s progress.')) {
			return;
		}
		if (cancel) {
			if (admin) {
				await orders.status(id, 'Cancelled');
			}
			else {
				await orders.cancel(id);
			}
		} else {
			await orders.status(id, b.dataset.next);
		}
		notify(cancel ? 'Order cancelled.' : 'Order status updated.');
		ctx.refresh();
	};
	view.querySelector('[data-cancel]')?.addEventListener('click', event => action(event.currentTarget, () => change(event.currentTarget, true)));
	view.querySelector('[data-next]')?.addEventListener('click', event => {
		const b = event.currentTarget;
		action(b, () => change(b, false));
	});
}