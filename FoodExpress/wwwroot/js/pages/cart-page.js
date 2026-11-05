import {
	cart,
	addresses,
	orders
} from '../services/orders.js';
import {
	cartCount
} from '../components.js?v=dish-cards-3';
import {
	escape,
	money,
	empty,
	headerText,
	action,
	notify,
	bindForm,
	confirmAction
} from '../utils.js';
import {
	addressForm,
	bindAddress
} from './account-page.js';

function lines(data, editable) {
	return data.items.map(item => `<article class="Cart-line"><div class="Cart-initial" aria-hidden="true">${escape(item.name.slice(0, 1))}</div><div class="Cart-description"><h3>${escape(item.name)}</h3><p>${money(item.unitPrice)} each</p>${!item.isAvailable ? '<span class="badge status-Cancelled">Unavailable — remove to continue</span>' : ''}</div>${editable ? `<div class="quantity"><button data-quantity="${item.menuItemId}" data-value="${item.quantity - 1}" ${item.quantity <= 1 ? 'disabled' : ''} aria-label="Decrease ${escape(item.name)} quantity">−</button><span>${item.quantity}</span><button data-quantity="${item.menuItemId}" data-value="${item.quantity + 1}" ${item.quantity >= 99 ? 'disabled' : ''} aria-label="Increase ${escape(item.name)} quantity">+</button></div>` : `<span>× ${item.quantity}</span>`}<strong>${money(item.lineTotal)}</strong>${editable ? `<button class="text-button remove" data-remove="${item.menuItemId}" aria-label="Remove ${escape(item.name)}">Remove</button>` : ''}</article>`).join('');
}
export async function cartPage(view, params, ctx) {
	const data = await cart.get();
	cartCount(data);
	view.innerHTML = headerText('SOMETHING GOOD IS COMING', 'Your Cart', 'A little of what you love. All from one kitchen.');
	if (!data.items.length) {
		view.innerHTML += empty('Your Cart is waiting.', 'Find something delicious and make it yours.');
		return;
	}
	view.innerHTML += `<div class="checkout-layout"><section class="panel Cart-lines">${lines(data, true)}<a href="#/restaurant/${data.items[0].restaurantId}" class="subtle-link">← Add something else</a></section><aside class="panel order-summary"><p class="eyebrow">THE GOOD STUFF</p><h2>Order summary</h2><div class="summary-row"><span>Items</span><span>${data.items.reduce((s, i) => s + i.quantity, 0)}</span></div><div class="summary-row total"><span>Subtotal</span><strong>${money(data.subtotal)}</strong></div><p class="field-help">Your final total is confirmed by the restaurant when you place your order.</p>${data.items.every(i => i.isAvailable) ? '<a class="button full" href="#/checkout">Continue to checkout →</a>' : '<p class="notice">Remove unavailable dishes before checking out.</p>'}</aside></div>`;
	view.querySelectorAll('[data-quantity]').forEach(b => b.onclick = () => action(b, async () => {
		const result = await cart.update(b.dataset.quantity, Number(b.dataset.value));
		cartCount(result);
		ctx.refresh();
	}));
	view.querySelectorAll('[data-remove]').forEach(b => b.onclick = () => action(b, async () => {
		const result = await cart.remove(b.dataset.remove);
		cartCount(result);
		notify('Item removed.');
		ctx.refresh();
	}));
}
export async function checkoutPage(view, params, ctx) {
	const [data, places] = await Promise.all([cart.get(), addresses.list()]);
	cartCount(data);
	view.innerHTML = headerText('ALMOST AT YOUR TABLE', 'The final little details.', 'Choose where your good food should go.');
	if (!data.items.length) {
		view.innerHTML += empty('Your Cart is empty.', 'Add a dish before checking out.');
		return;
	}
	view.innerHTML += `<div class="checkout-layout"><section><div class="panel"><h2>Delivery address</h2><form id="checkout-form"><div data-errors></div><div class="address-options">${places.map((a, i) => `<label class="address-option"><input type="radio" name="addressId" value="${a.id}" ${i === 0 ? 'checked' : ''} required><span><strong>${escape(a.streetAddress)}</strong><small>${escape(a.city)} · ${escape(a.countryCode)}</small></span></label>`).join('')}</div>${!places.length ? '<p class="notice">Add a delivery address below to continue.</p>' : ''}<button class="button full" type="submit" ${!places.length || !data.items.every(i => i.isAvailable) ? 'disabled' : ''}>Place my order →</button></form></div><details class="panel"><summary>Add a new delivery address</summary>${addressForm()}</details></section><aside class="panel order-summary"><h2>Your order</h2>${lines(data, false)}<div class="summary-row total"><span>Subtotal</span><strong>${money(data.subtotal)}</strong></div><p class="field-help">Prices and availability are checked again when you order. The confirmation shows your final total.</p><a class="subtle-link" href="#/cart">Edit your Cart →</a></aside></div>`;
	bindAddress(view.querySelector('#address-form'), ctx.refresh);
	bindForm(view.querySelector('#checkout-form'), async values => {
		if (!await confirmAction('Ready to order?', 'Send this order to the restaurant using your selected delivery address?')) {
			return;
		}
		const result = await orders.create(Number(values.addressId));
		cartCount({
			items: []
		});
		notify('Your order has been placed.');
		ctx.go('/order/' + result.order.id, {
			confirmed: 'true'
		});
	});
}