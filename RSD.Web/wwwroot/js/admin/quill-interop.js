const QUILL_CSS = 'https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.snow.css';
const QUILL_JS = 'https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.js';
const TOOLBAR = [
    ['bold', 'italic', 'underline', 'link'],
    [{ 'header': 2 }, { 'header': 3 }],
    [{ 'list': 'bullet' }, { 'list': 'ordered' }],
    ['clean']
];
const DEBOUNCE_MS = 300;

let quillLoader = null;
function ensureQuill() {
    if (quillLoader) return quillLoader;
    quillLoader = (async () => {
        if (!document.querySelector(`link[data-quill]`)) {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = QUILL_CSS;
            link.dataset.quill = '1';
            document.head.appendChild(link);
        }
        if (window.Quill) return window.Quill;
        await new Promise((resolve, reject) => {
            const s = document.createElement('script');
            s.src = QUILL_JS;
            s.onload = resolve;
            s.onerror = reject;
            document.head.appendChild(s);
        });
        return window.Quill;
    })();
    return quillLoader;
}

export async function attach(container, dotnetRef, initialHtml) {
    const Quill = await ensureQuill();
    container.innerHTML = '';
    const editor = document.createElement('div');
    container.appendChild(editor);
    const quill = new Quill(editor, {
        theme: 'snow',
        modules: { toolbar: TOOLBAR },
        formats: ['bold', 'italic', 'underline', 'link', 'header', 'list']
    });
    if (initialHtml) quill.clipboard.dangerouslyPasteHTML(initialHtml);

    let timer = null;
    let lastEmitted = quill.root.innerHTML;
    const emit = () => {
        const html = quill.root.innerHTML;
        if (html === lastEmitted) return;
        lastEmitted = html;
        dotnetRef.invokeMethodAsync('OnHtmlChangedAsync', html).catch(() => { /* component disposed */ });
    };
    quill.on('text-change', () => {
        if (timer) clearTimeout(timer);
        timer = setTimeout(emit, DEBOUNCE_MS);
    });

    return {
        setValue(html) {
            if (html === lastEmitted) return;
            lastEmitted = html;
            quill.clipboard.dangerouslyPasteHTML(html ?? '');
        },
        destroy() {
            if (timer) clearTimeout(timer);
            container.innerHTML = '';
        }
    };
}
