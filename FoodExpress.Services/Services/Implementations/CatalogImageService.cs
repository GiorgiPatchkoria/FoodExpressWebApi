using FoodExpress.Core.Interfaces;
using FoodExpress.Services.Services.Interfaces;
namespace FoodExpress.Services.Services.Implementations;
public class CatalogImageService : ICatalogImageService
{
    private readonly IRestaurantRepository _restaurants;
    private readonly IMenuItemRepository _menu;
    private readonly IFileStorageService _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _current;
    public CatalogImageService(IRestaurantRepository restaurants, IMenuItemRepository menu, IFileStorageService storage, IUnitOfWork unitOfWork, ICurrentUser current)
    {
        _restaurants = restaurants;
        _menu = menu;
        _storage = storage;
        _unitOfWork = unitOfWork;
        _current = current;
    }
    public async Task<string> UploadAsync(int id, bool restaurant, Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        ServiceAccess.RequireAdmin(_current);
        var r = restaurant ? await _restaurants.FindAsync(id, ct) : null;
        var m = restaurant ? null : await _menu.FindAsync(id, ct);
        if (r == null && m == null)
        {
            throw new ServiceException(ServiceError.NotFound, "Catalog item not found.");
        }

        string url;
        using var image = new MemoryStream();
        try
        {
            var chunk = new byte[81920];
            int read;
            while ((read = await stream.ReadAsync(chunk, ct)) > 0)
            {
                if (image.Length + read > 5 * 1024 * 1024)
                {
                    throw new ArgumentException("Images must be 5 MB or smaller.");
                }

                image.Write(chunk, 0, read);
            }
            var bytes = image.ToArray();
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var jpeg = bytes.Length >= 3 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255;
            var png = bytes.Length >= 8 && bytes.Take(8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10});
            var webp = bytes.Length >= 12 && System.Text.Encoding.ASCII.GetString(bytes,0,4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes,8,4) == "WEBP";
            if (!((jpeg && (extension == ".jpg" || extension == ".jpeg") && contentType == "image/jpeg") ||
                  (png && extension == ".png" && contentType == "image/png") ||
                  (webp && extension == ".webp" && contentType == "image/webp")))
            {
                throw new ArgumentException("Upload a JPEG, PNG, or WebP image.");
            }

            image.Position = 0;
            url = await _storage.SaveFileAsync(image, fileName, contentType, restaurant ? "restaurants" : "menu-items", ct);
        }
        catch (ArgumentException exception)
        {
            throw new ServiceException(ServiceError.Validation, exception.Message);
        }
        var oldUrl = r != null ? r.ImageUrl : m!.ImageUrl;
        if (r != null) { r.ImageUrl = url; } else { m!.ImageUrl = url; }
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch
        {
            await _storage.DeleteFileAsync(url);
            throw;
        }
        if (oldUrl != null) { await _storage.DeleteFileAsync(oldUrl); }
        return url;
    }
}