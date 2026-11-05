namespace FoodExpress.Core.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct = default);
        Task DeleteFileAsync(string filePath, CancellationToken ct = default);
    }
}