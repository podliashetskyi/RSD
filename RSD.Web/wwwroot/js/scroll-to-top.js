// Reset scroll to the top on every Blazor enhanced navigation. The framework
// keeps the previous scroll position by default, which makes routing between
// pages feel broken on long pages. Same-page anchor jumps still work because
// the browser fires hashchange events that we don't touch.
(function () {
    const resetScroll = () => {
        if (location.hash) return;
        window.scrollTo({ top: 0, left: 0, behavior: 'instant' });
    };
    document.addEventListener('enhancedload', resetScroll);
    window.addEventListener('blazor:enhancednavigation', resetScroll);
})();
