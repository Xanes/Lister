namespace Infrastructure.Models
{
    public record ProductCategoryGroup
    {
        public ProductCategoryGroup(string name, List<Product> products)
        {
            Name = name;
            Products = products;
        }

        public string Name { get; set; }
        public List<Product> Products { get; set; }
    }
}