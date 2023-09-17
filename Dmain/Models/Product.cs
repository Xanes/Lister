namespace Domain.Models
{
    public record Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double? Quantity { get; set; }
        public string? QuantityUnit { get; set; }
        public double? Weight { get; set; }
        public string? WeightUnit { get; set; }
    }
}