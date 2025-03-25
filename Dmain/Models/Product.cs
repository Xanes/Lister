namespace Domain.Models
{
    public record Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double? Quantity { get; set; }
        public double? ChangedQuantity { get; set; }
        public string? QuantityUnit { get; set; }
        public double? Weight { get; set; }
        public double? ChangedWeight { get; set; }
        public string? WeightUnit { get; set; }
        public bool IsChecked { get; set; }
        public bool IsTemporary { get; set; } = false;
    }
}