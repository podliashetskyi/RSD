// Off-canvas drawer toggle for the admin sidebar at narrow viewports.
// On `lg` and wider the sidebar is in normal flow (handled by Tailwind),
// so this script only matters on small/medium screens.

function setOpen(open) {
    const root = document.querySelector('[data-admin-shell]');
    if (!root) return;
    root.setAttribute('data-sidebar-open', String(open));
    const toggle = document.querySelector('[data-sidebar-toggle]');
    if (toggle) toggle.setAttribute('aria-expanded', String(open));
}

document.addEventListener('click', (e) => {
    if (e.target.closest('[data-sidebar-toggle]')) {
        const root = document.querySelector('[data-admin-shell]');
        const isOpen = root?.getAttribute('data-sidebar-open') === 'true';
        setOpen(!isOpen);
        return;
    }
    if (e.target.closest('[data-sidebar-backdrop]')) {
        setOpen(false);
        return;
    }
    // Tapping a nav link inside the drawer should close it on mobile.
    if (window.innerWidth < 1024 && e.target.closest('[data-admin-sidebar] a')) {
        setOpen(false);
    }
});

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') setOpen(false);
});
