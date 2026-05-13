export function attach(dropzone, dotnetRef) {
    if (!dropzone || dropzone.__rsdImageUploaderBound) return;
    dropzone.__rsdImageUploaderBound = true;

    const onDragOver = (e) => {
        e.preventDefault();
        dropzone.classList.add('ring-2', 'ring-gray-500');
    };

    const onDragLeave = () => {
        dropzone.classList.remove('ring-2', 'ring-gray-500');
    };

    const onDrop = (e) => {
        e.preventDefault();
        dropzone.classList.remove('ring-2', 'ring-gray-500');
        const file = e.dataTransfer?.files?.[0];
        if (!file) return;
        forwardFile(dotnetRef, file);
    };

    dropzone.addEventListener('dragover', onDragOver);
    dropzone.addEventListener('dragleave', onDragLeave);
    dropzone.addEventListener('drop', onDrop);

    dropzone.__rsdImageUploaderCleanup = () => {
        dropzone.removeEventListener('dragover', onDragOver);
        dropzone.removeEventListener('dragleave', onDragLeave);
        dropzone.removeEventListener('drop', onDrop);
    };
}

export function detach(dropzone) {
    if (!dropzone || !dropzone.__rsdImageUploaderCleanup) return;
    dropzone.__rsdImageUploaderCleanup();
    delete dropzone.__rsdImageUploaderCleanup;
    delete dropzone.__rsdImageUploaderBound;
}

async function forwardFile(dotnetRef, file) {
    const buffer = await file.arrayBuffer();
    const bytes = new Uint8Array(buffer);
    await dotnetRef.invokeMethodAsync('OnDroppedFileAsync', file.name, file.type, Array.from(bytes));
}
