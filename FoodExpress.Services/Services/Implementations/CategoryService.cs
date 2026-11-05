using AutoMapper;
using FoodExpress.Core.Interfaces;
using FoodExpress.Core.Models.DTOs;
using FoodExpress.Core.Models.Entities;
using FoodExpress.Services.Services.Interfaces;

namespace FoodExpress.Services.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categories;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestValidator _validator;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _current;

        public CategoryService(ICategoryRepository categories, IUnitOfWork unitOfWork,
            IRequestValidator validator, IMapper mapper, ICurrentUser current)
        {
            _categories = categories;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _mapper = mapper;
            _current = current;
        }

        public async Task<IReadOnlyList<CategoryDTO>> ListAsync(PageDTO pagination, CancellationToken ct = default)
        {
            _validator.Validate(pagination);
            var categories = await _categories.ListAsync((pagination.Page - 1) * pagination.PageSize, pagination.PageSize, ct);
            return _mapper.Map<List<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO> GetAsync(int id, CancellationToken ct = default)
        {
            _validator.PositiveId(id);
            var category = await _categories.FindAsync(id, ct);
            if (category == null)
            {
                throw new ServiceException(ServiceError.NotFound, "Category not found.");
            }
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto, CancellationToken ct = default)
        {
            ServiceAccess.RequireAdmin(_current);
            _validator.Validate(dto);
            var name = dto.Name.Trim();
            if (await _categories.NameExistsAsync(name, ct))
            {
                throw new ServiceException(ServiceError.Conflict, "A category with this name already exists.");
            }
            var category = _mapper.Map<Category>(dto);
            category.Name = name;
            await _categories.AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return _mapper.Map<CategoryDTO>(category);
        }
    }
}