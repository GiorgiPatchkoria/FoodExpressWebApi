using FoodExpress.Core.Models.DTOs;

namespace FoodExpress.Services.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<CategoryDTO>> ListAsync(PageDTO pagination, CancellationToken ct = default);
        Task<CategoryDTO> GetAsync(int id, CancellationToken ct = default);
        Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto, CancellationToken ct = default);
    }
}