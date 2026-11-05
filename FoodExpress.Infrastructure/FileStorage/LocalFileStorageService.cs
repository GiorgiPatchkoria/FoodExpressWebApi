using FoodExpress.Core.Interfaces;
namespace FoodExpress.Infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;

    public LocalFileStorageService(string webRootPath)
    {
        _root = Path.GetFullPath(Path.Combine(webRootPath, "uploads"));
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        if (folder != "restaurants" && folder != "menu-items")
        {
            throw new ArgumentException("Invalid folder.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        Directory.CreateDirectory(Path.Combine(_root, folder));
        var name = Guid.NewGuid().ToString("N") + extension;
        var path = Path.Combine(_root, folder, name);
        try
        {
            await using var file = new FileStream(path, FileMode.CreateNew);
            await stream.CopyToAsync(file, ct);
        }
        catch
        {
            File.Delete(path);
            throw;
        }
        return "/uploads/" + folder + "/" + name;
    }
    public Task DeleteFileAsync(string filePath, CancellationToken ct = default)
    {
        var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(_root)!, filePath.TrimStart('/')));
        if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid upload path.");
        }

        File.Delete(path);
        return Task.CompletedTask;
    }
}