using System.ComponentModel.DataAnnotations;

namespace BlitzMall_Backend.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
        public string? BrandName { get; set; }
        public string? CategoryName { get; set; }
        public string? SellerName { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> ImgUrls { get; set; } = new();
        public double? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public string? Badge { get; set; }
    }
}
