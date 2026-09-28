namespace BlitzMall_Backend.DTOs.Product
{
    public class ProductSearchResultDto
    {
        public List<ProductDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
