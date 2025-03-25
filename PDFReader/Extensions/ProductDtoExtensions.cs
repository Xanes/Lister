using PDFReader.DTOs;

namespace PDFReader.Extensions
{
    public static class ProductDtoExtensions
    {
        public static bool IsValid(this ProductDto product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                return false;

            // All props are mandatory
            if (!product.Quantity.HasValue || !product.Weight.HasValue || string.IsNullOrWhiteSpace(product.QuantityUnit) || string.IsNullOrWhiteSpace(product.WeightUnit))
                return false;

            return true;
        }
    }
} 