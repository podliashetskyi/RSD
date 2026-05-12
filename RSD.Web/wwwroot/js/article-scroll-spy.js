(function () {
    'use strict';

    const SCROLL_OFFSET = 96;

    function smoothScrollToId(id) {
        const target = document.getElementById(id);
        if (!target) return false;
        const top = target.getBoundingClientRect().top + window.pageYOffset - SCROLL_OFFSET;
        window.scrollTo({ top, behavior: 'smooth' });
        history.replaceState(null, '', '#' + id);
        return true;
    }

    function initScrollSpy() {
        const bodies = document.querySelectorAll('[data-article-body]');
        const tocs = document.querySelectorAll('[data-article-toc]');
        if (!bodies.length || !tocs.length) {
            return;
        }

        bodies.forEach((body, i) => {
            const toc = tocs[i] || tocs[0];
            const links = Array.from(toc.querySelectorAll('a[href^="#"]'));
            if (!links.length) {
                return;
            }

            const headings = Array.from(body.querySelectorAll('[id]'))
                .filter((el) => el.tagName === 'H2' || el.hasAttribute('data-toc-target'));
            // Also include any sections after the body that are referenced by TOC links
            const linkIds = links.map((a) => a.getAttribute('href').slice(1));
            const allTargets = linkIds
                .map((id) => document.getElementById(id))
                .filter((el) => el !== null);
            const trackable = Array.from(new Set([...headings, ...allTargets]));

            if (!trackable.length) {
                return;
            }

            const setActive = (id) => {
                links.forEach((a) => {
                    const isActive = a.getAttribute('href') === '#' + id;
                    a.classList.toggle('is-active', isActive);
                });
            };

            // Default: first link active
            const firstId = trackable[0].id;
            if (firstId) setActive(firstId);

            const observer = new IntersectionObserver((entries) => {
                const visible = entries
                    .filter((e) => e.isIntersecting)
                    .sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top);
                if (visible.length > 0 && visible[0].target.id) {
                    setActive(visible[0].target.id);
                }
            }, {
                root: null,
                rootMargin: '-96px 0px -65% 0px',
                threshold: 0,
            });

            trackable.forEach((el) => { if (el.id) observer.observe(el); });

            // Smooth scroll on click; works around Blazor enhanced-nav intercepting same-page #anchors.
            links.forEach((a) => {
                a.addEventListener('click', (ev) => {
                    const href = a.getAttribute('href');
                    if (!href || !href.startsWith('#')) return;
                    const id = href.slice(1);
                    if (smoothScrollToId(id)) {
                        ev.preventDefault();
                        setActive(id);
                    }
                });
            });
        });
    }

    function bind() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initScrollSpy);
        } else {
            initScrollSpy();
        }
    }

    bind();

    // Re-bind after Blazor enhanced navigation completes
    document.addEventListener('enhancednavigationend', initScrollSpy);
})();
