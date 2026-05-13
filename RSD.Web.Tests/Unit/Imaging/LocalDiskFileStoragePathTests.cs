using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using RSD.Web.Services.Storage;

namespace RSD.Web.Tests.Unit.Imaging;

public sealed class LocalDiskFileStoragePathTests : IDisposable
{
    private readonly string TempRoot = Path.Combine(Path.GetTempPath(), $"rsd-test-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveAsync_PlacesFileUnderUploadsSubfolderYearMonth()
    {
        var env = new StubEnv(TempRoot);
        Directory.CreateDirectory(TempRoot);
        var sut = new LocalDiskFileStorage(env);
        using var ms = new MemoryStream(new byte[] { 1, 2, 3 });

        var stored = await sut.SaveAsync("blog", ms, "hello.png", "image/png", CancellationToken.None);

        var year = DateTime.UtcNow.Year.ToString("0000");
        var month = DateTime.UtcNow.Month.ToString("00");
        stored.Path.Should().StartWith($"uploads/blog/{year}/{month}/");
        stored.Path.Should().EndWith("-hello.png");
        stored.Bytes.Should().Be(3);
        File.Exists(Path.Combine(TempRoot, stored.Path.Replace('/', Path.DirectorySeparatorChar))).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_SanitizesSuggestedFilename()
    {
        var env = new StubEnv(TempRoot);
        Directory.CreateDirectory(TempRoot);
        var sut = new LocalDiskFileStorage(env);
        using var ms = new MemoryStream(new byte[] { 0 });

        var stored = await sut.SaveAsync("test", ms, "../etc/../weird name!!.png", "image/png", CancellationToken.None);

        stored.Path.Should().NotContain("..");
        stored.Path.Should().NotContain(" ");
        stored.Path.Should().EndWith(".png");
    }

    [Fact]
    public async Task SaveAsync_DifferentCallsProduceUniquePaths()
    {
        var env = new StubEnv(TempRoot);
        Directory.CreateDirectory(TempRoot);
        var sut = new LocalDiskFileStorage(env);

        using var a = new MemoryStream(new byte[] { 1 });
        using var b = new MemoryStream(new byte[] { 1 });
        var first = await sut.SaveAsync("test", a, "same.png", "image/png", CancellationToken.None);
        var second = await sut.SaveAsync("test", b, "same.png", "image/png", CancellationToken.None);

        first.Path.Should().NotBe(second.Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(TempRoot)) Directory.Delete(TempRoot, recursive: true);
    }

    private sealed class StubEnv(string webRoot) : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = webRoot;
        public IFileProvider WebRootFileProvider { get; set; } = default!;
        public string ApplicationName { get; set; } = "RSD.Web.Tests";
        public string ContentRootPath { get; set; } = webRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = default!;
        public string EnvironmentName { get; set; } = "Test";
    }
}
