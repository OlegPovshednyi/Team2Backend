using BlitzMall_Backend.DTOs.Product;

namespace BlitzMall_Backend.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ProductSearchResultDto> SearchAsync(string? query, int? categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
    }
}
