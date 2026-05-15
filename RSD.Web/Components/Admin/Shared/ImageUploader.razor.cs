#pragma warning disable S1144, S4487, S2933

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using RSD.Web.Data;
using RSD.Web.Data.Entities;
using RSD.Web.Services.Imaging;

namespace RSD.Web.Components.Admin.Shared;

public partial class ImageUploader(
    IImageProcessor Processor,
    AppDbContext Db,
    AuthenticationStateProvider AuthState,
    IJSRuntime Js,
    ILogger<ImageUploader> Log) : ComponentBase, IAsyncDisposable
{
    [Parameter, EditorRequired] public string Subfolder { get; set; } = "";
    [Parameter] public UploadedFile? CurrentFile { get; set; }
    [Parameter] public EventCallback<UploadedFile?> CurrentFileChanged { get; set; }
    [Parameter] public long MaxBytes { get; set; } = 8 * 1024 * 1024;
    [Parameter] public string Alt { get; set; } = "";
    [Parameter] public EventCallback<string> AltChanged { get; set; }

    private string InputId { get; } = $"upload-{Guid.NewGuid():N}";
    private string AltId { get; } = $"upload-alt-{Guid.NewGuid():N}";

    private Task OnAltInput(ChangeEventArgs e) => AltChanged.InvokeAsync(e.Value?.ToString() ?? "");
    private bool IsUploading { get; set; }
    private string Error { get; set; } = "";
    private UploadedFile? Preview { get; set; }
    private ElementReference DropzoneRef { get; set; }
    private IJSObjectReference? JsModule { get; set; }
    private DotNetObjectReference<ImageUploader>? Self { get; set; }

    protected override void OnInitialized()
    {
        if (CurrentFile is not null) Preview = CurrentFile;
    }

    protected override void OnParametersSet()
    {
        if (CurrentFile is not null && Preview is null) Preview = CurrentFile;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        JsModule = await Js.InvokeAsync<IJSObjectReference>("import", "/js/admin/image-uploader.js");
        Self = DotNetObjectReference.Create(this);
        await JsModule.InvokeVoidAsync("attach", DropzoneRef, Self);
    }

    private async Task HandleInputAsync(InputFileChangeEventArgs e)
    {
        var file = e.File;
        await using var stream = file.OpenReadStream(MaxBytes);
        await ProcessAsync(stream, file.Name, file.ContentType);
    }

    [JSInvokable]
    public async Task OnDroppedFileAsync(string name, string contentType, byte[] bytes)
    {
        if (bytes.LongLength > MaxBytes) { Error = "File is too large."; await InvokeAsync(StateHasChanged); return; }
        await using var stream = new MemoryStream(bytes);
        await InvokeAsync(() => ProcessAsync(stream, name, contentType));
    }

    private async Task ProcessAsync(Stream stream, string name, string contentType)
    {
        Error = "";
        IsUploading = true;
        StateHasChanged();
        var result = await TryProcessAsync(stream, name, contentType);
        IsUploading = false;
        if (result is not null)
        {
            Preview = result;
            await CurrentFileChanged.InvokeAsync(result);
        }
        StateHasChanged();
    }

    private async Task<UploadedFile?> TryProcessAsync(Stream stream, string name, string contentType)
    {
        try
        {
            var processed = await Processor.ProcessAsync(Subfolder, stream, name, contentType, CancellationToken.None);
            return await PersistAsync(processed, name, contentType);
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "ImageUploader: failed to process file '{Name}'.", name);
            Error = "Could not process the file. Try a different image.";
            return null;
        }
    }

    private async Task<UploadedFile> PersistAsync(ProcessedUpload processed, string name, string contentType)
    {
        var uploader = await ResolveUploaderIdAsync();
        var entity = new UploadedFile
        {
            Path = processed.OriginalFile.Path,
            OriginalName = name,
            ContentType = contentType,
            Bytes = processed.OriginalFile.Bytes,
            UploadedByUserId = uploader,
            Variants = processed.Variants.ToList(),
        };
        Db.UploadedFiles.Add(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    private async Task<string> ResolveUploaderIdAsync()
    {
        var state = await AuthState.GetAuthenticationStateAsync();
        return state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    }

    private Task ClearAsync()
    {
        Preview = null;
        return CurrentFileChanged.InvokeAsync(null);
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:0.#} KB";
        return $"{bytes / (1024.0 * 1024.0):0.##} MB";
    }

    public async ValueTask DisposeAsync()
    {
        if (JsModule is not null)
        {
            try { await JsModule.InvokeVoidAsync("detach", DropzoneRef); }
            catch { /* ignore disposal races */ }
            await JsModule.DisposeAsync();
        }
        Self?.Dispose();
    }
}
