import {
	config
} from './config.js';
export function escape(value) {
	return String(value ?? '').replace(/[&<>"']/g, c => ({
		'&': '&amp;',
		'<': '&lt;',
		'>': '&gt;',
		'"': '&quot;',
		"'": '&#39;'
	}[c]));
}
export function money(value) {
	return new Intl.NumberFormat(config.locale, {
		style: 'currency',
		currency: config.currency
	}).format(Number(value) || 0);
}
export function date(value) {
	const d = new Date(value);
	return Number.isNaN(d.getTime()) ? 'Date unavailable' : new Intl.DateTimeFormat(config.locale, {
		dateStyle: 'medium',
		timeStyle: 'short'
	}).format(d);
}
export function notify(message, error = false) {
	const el = document.createElement('div');
	el.className = `toast ${error ? 'error' : ''}`;
	el.textContent = message;
	document.querySelector('#notifications').append(el);
	setTimeout(() => el.remove(), 6000);
}
export function problem(error) {
    const details = Object.entries(error.errors || {}).map(([field, values]) => {
        return `<p>${escape(field)}: ${escape([].concat(values).join(' '))}</p>`;
    }).join('');
    return `<div class="notice error" role="alert"><strong>${escape(error.message || 'Something went wrong.')}</strong>${details}</div>`;
}
export function empty(title, message, link = '#/restaurants', label = 'Explore restaurants') {
    const button = link ? `<a class="button" href="${escape(link)}">${escape(label)}</a>` : '';
    return `<div class="empty"><span class="empty-icon" aria-hidden="true">◇</span><h2>${escape(title)}</h2><p>${escape(message)}</p>${button}</div>`;
}
export function loading() {
	return '<div class="loading" role="status"><span class="spinner"></span> Bringing things to the table…</div>';
}
export function pageNumber(params) {
	return Math.max(1, Math.min(10000, Number(params.get('page')) || 1));
}
export function link(path, query = {}) {
	const p = new URLSearchParams();
	for (const [k, v] of Object.entries(query)) {
		if (v !== '' && v != null) {
			p.set(k, v);
		}
	}
	return `#${path}${p.size ? '?' + p : ''}`;
}
export function pagination(path, query, count, pageSize = config.pageSize) {
	const page = Number(query.page || 1);
	return `<nav class="pagination" aria-label="Pagination">${page > 1 ? `<a class="button secondary" href="${escape(link(path, { ...query, page: page - 1 }))}">← Previous</a>` : '<span></span>'}<span>Page ${page}</span>${count === pageSize && page < 10000 ? `<a class="button secondary" href="${escape(link(path, { ...query, page: page + 1 }))}">Next →</a>` : '<span></span>'}</nav>`;
}
export function values(form) {
	const data = Object.fromEntries(new FormData(form));
	for (const input of form.querySelectorAll('[type=checkbox]')) {
		data[input.name] = input.checked;
	}
	return data;
}
export function bindForm(form, handler) {
	form.addEventListener('submit', async event => {
		event.preventDefault();
		if (!form.reportValidity() || form.dataset.busy) {
			return;
		}
		const errorBox = form.querySelector('[data-errors]');
		if (errorBox) {
			errorBox.innerHTML = '';
		}
		form.dataset.busy = 'true';
		const button = form.querySelector('[type=submit]');
		const label = button?.textContent;
		if (button) {
			button.disabled = true;
			button.textContent = 'Please wait…';
		}
		try {
			await handler(values(form));
		} catch (error) {
			if (errorBox) {
				errorBox.innerHTML = problem(error);
				errorBox.scrollIntoView({
					block: 'nearest'
				});
			} else {
				notify(error.message, true);
			}
		} finally {
			delete form.dataset.busy;
			if (button) {
				button.disabled = false;
				button.textContent = label;
			}
		}
	});
}
export async function action(button, work) {
	if (button.disabled) {
		return;
	}
	button.disabled = true;
	try {
		await work();
	} catch (error) {
		notify(error.message, true);
	} finally {
		if (button.isConnected) {
			button.disabled = false;
		}
	}
}
export function field(name, label, {
	type = 'text',
	value = '',
	required = true,
	max,
	min,
	step,
	pattern,
	autocomplete
} = {}) {
	return `<label>${escape(label)}<input name="${escape(name)}" type="${type}" value="${escape(value ?? '')}" ${required ? 'required' : ''} ${max != null ? (type === 'number' ? `max="${max}"` : `maxlength="${max}"`) : ''} ${min != null ? (type === 'number' ? `min="${min}"` : `minlength="${min}"`) : ''} ${step ? `step="${step}"` : ''} ${pattern ? `pattern="${escape(pattern)}"` : ''} ${autocomplete ? `autocomplete="${autocomplete}"` : ''}></label>`;
}
export function checkbox(name, label, checked) {
	return `<label class="check"><input type="checkbox" name="${name}" ${checked ? 'checked' : ''}>${escape(label)}</label>`;
}
export function select(name, label, options, value = '') {
	return `<label>${escape(label)}<select name="${name}">${options.map(([v, t]) => `<option value="${escape(v)}" ${String(v) === String(value) ? 'selected' : ''}>${escape(t)}</option>`).join('')}</select></label>`;
}
export function showDialog(title, content) {
	const dialog = document.querySelector('#dialog');
	dialog.innerHTML = `<div class="dialog-head"><h2 id="dialog-title">${escape(title)}</h2><button type="button" class="icon-button" data-close aria-label="Close dialog">×</button></div>${content}`;
	dialog.querySelector('[data-close]').onclick = () => dialog.close();
	if (!dialog.open) {
		dialog.showModal();
	}
	return dialog;
}
export function confirmAction(title, message) {
	return new Promise(resolve => {
		const dialog = showDialog(title, `<p>${escape(message)}</p><div class="actions"><button class="button secondary" data-no>Keep it</button><button class="button danger" data-yes>Confirm</button></div>`);
		let settled = false;
		const finish = answer => {
			if (!settled) {
				settled = true;
				dialog.close();
				resolve(answer);
			}
		};
		dialog.querySelector('[data-yes]').onclick = () => finish(true);
		dialog.querySelector('[data-no]').onclick = () => finish(false);
		dialog.addEventListener('close', () => finish(false), {
			once: true
		});
	});
}
export const statuses = ['Created', 'Confirmed', 'Preparing', 'OnTheWay', 'Delivered', 'Cancelled'];
export function statusName(value) {
	return typeof value === 'number' ? statuses[value] : value;
}
export function badge(status) {
	const value = statusName(status);
	return `<span class="badge status-${escape(value)}">${escape(value === 'OnTheWay' ? 'On the way' : value)}</span>`;
}
export function headerText(eyebrow, title, text = '', actionHTML = '') {
	return `<div class="page-heading"><div><p class="eyebrow">${escape(eyebrow)}</p><h1>${escape(title)}</h1>${text ? `<p class="muted">${escape(text)}</p>` : ''}</div>${actionHTML}</div>`;
}