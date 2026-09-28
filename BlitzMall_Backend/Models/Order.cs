namespace BlitzMall_Backend.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public decimal TotalAmount { get; set; }
        public string? OrderStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int AddressId { get; set; }
        public Address? Address { get; set; }
        public string? Phone { get; set; }
        public string? Comment { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
