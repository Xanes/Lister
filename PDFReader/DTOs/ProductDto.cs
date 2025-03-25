using System.ComponentModel.DataAnnotations;

namespace PDFReader.DTOs
{
    /// <summary>
    /// DTO for a new product without ID and IsTemporary flag
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Name of the product
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Quantity of the product (optional if weight is specified)
        /// </summary>
        public double? Quantity { get; set; }

        /// <summary>
        /// Unit of quantity measurement (e.g., pieces, kg)
        /// </summary>
        public string? QuantityUnit { get; set; }

        /// <summary>
        /// Weight of the product (optional if quantity is specified)
        /// </summary>
        public double? Weight { get; set; }

        /// <summary>
        /// Unit of weight measurement (e.g., g, kg)
        /// </summary>
        public string? WeightUnit { get; set; }
    }
} 