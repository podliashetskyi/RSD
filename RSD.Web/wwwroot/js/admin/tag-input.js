// Tag input keydown helper: swallow Enter (and comma) so that pressing them
// inside the tag <input> doesn't submit the surrounding <form>. Blazor's
// @onkeydown handler still fires and commits the tag; this just prevents the
// browser's default form-submit behaviour.
export function attach(element) {
    if (!element) return;
    const handler = (e) => {
        if (e.key === 'Enter' || e.key === ',') {
            e.preventDefault();
        }
    };
    element.addEventListener('keydown', handler);
    element._tagInputKeydown = handler;
}

export function detach(element) {
    if (!element || !element._tagInputKeydown) return;
    element.removeEventListener('keydown', element._tagInputKeydown);
    delete element._tagInputKeydown;
}
