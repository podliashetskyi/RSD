// Theme toggle for the admin panel. The first paint is governed by the inline
// /js/theme.js boot script (which respects localStorage.theme + the OS
// prefers-color-scheme media query). This module is loaded interactively by
// the ThemeToggle Blazor component to read and flip the choice at runtime.

export function getResolvedTheme() {
    return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
}

export function setTheme(mode) {
    if (mode === 'dark') {
        document.documentElement.classList.add('dark');
        localStorage.theme = 'dark';
        return;
    }
    if (mode === 'light') {
        document.documentElement.classList.remove('dark');
        localStorage.theme = 'light';
        return;
    }
    // null / 'auto' — clear the persisted choice and follow the OS.
    localStorage.removeItem('theme');
    if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
}
