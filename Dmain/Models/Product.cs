namespace Infrastructure.Models
{
    public record Product
    {
        public Product()
        {
        }

        public string? Name { get; set; }
        public double? Quantity { get; set; }
        public string? QuantityUnit { get; set; }
        public double? Weight { get; set; }
        public string? WeightUnit { get; set; }

        public override string ToString()
        {
            return $"{Name} {Quantity} x {QuantityUnit} {Weight} {WeightUnit}";
        }
    }
}