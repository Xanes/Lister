namespace Domain.Models
{
    public record ProductCategoryGroup
    {
        public Category? Category { get; set; }
        public int Id { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }
}