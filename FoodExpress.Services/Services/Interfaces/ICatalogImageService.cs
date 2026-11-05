namespace FoodExpress.Services.Services.Interfaces;

public interface ICatalogImageService
{
    Task<string> UploadAsync(int id, bool restaurant, Stream stream, string fileName, string contentType, CancellationToken ct = default);
}