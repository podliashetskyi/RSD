const stack = [];
const selector = [
    'a[href]',
    'button:not([disabled])',
    'input:not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[tabindex]:not([tabindex="-1"])',
].join(',');

export function attach(root, dotNetRef, escapeMethod) {
    if (!root) return;
    detach(root);

    const entry = {
        root,
        dotNetRef,
        escapeMethod,
        previous: document.activeElement instanceof HTMLElement ? document.activeElement : null,
    };

    entry.onKeyDown = (event) => handleKeyDown(event, entry);
    stack.push(entry);
    document.addEventListener('keydown', entry.onKeyDown, true);
    requestAnimationFrame(() => focusFirst(root));
}

export function detach(root) {
    const index = stack.findIndex((entry) => entry.root === root);
    if (index < 0) return;

    const [entry] = stack.splice(index, 1);
    document.removeEventListener('keydown', entry.onKeyDown, true);

    if (entry.previous?.isConnected) {
        entry.previous.focus();
    }
}

function handleKeyDown(event, entry) {
    if (stack[stack.length - 1] !== entry) return;

    if (event.key === 'Escape') {
        event.preventDefault();
        event.stopPropagation();
        entry.dotNetRef?.invokeMethodAsync(entry.escapeMethod);
        return;
    }

    if (event.key === 'Tab') {
        trapTab(event, entry.root);
    }
}

function trapTab(event, root) {
    const items = focusable(root);
    if (items.length === 0) {
        event.preventDefault();
        root.focus();
        return;
    }

    const first = items[0];
    const last = items[items.length - 1];
    const active = document.activeElement;

    if (event.shiftKey && active === first) {
        event.preventDefault();
        last.focus();
    } else if (!event.shiftKey && active === last) {
        event.preventDefault();
        first.focus();
    }
}

function focusFirst(root) {
    const items = focusable(root);
    (items[0] ?? root).focus();
}

function focusable(root) {
    return Array.from(root.querySelectorAll(selector))
        .filter((item) => item instanceof HTMLElement)
        .filter((item) => item.offsetParent !== null || item === document.activeElement);
}
