using System.Collections.Generic;
using System.Linq;
using Domain.Models;
using PDFReader.DTOs;

namespace PDFReader.Extensions
{
    public static class CategoryProductsDtoExtensions
    {
        public static IEnumerable<(int categoryId, List<Product> products)> ToDomainAdditionalProducts(this IEnumerable<CategoryProductsDto> categoryProducts)
        {
            return categoryProducts.Select(cp => (
                categoryId: cp.CategoryId,
                products: cp.Products.Select(p => new Product
                {
                    Name = p.Name,
                    Quantity = p.Quantity,
                    QuantityUnit = p.QuantityUnit,
                    Weight = p.Weight,
                    WeightUnit = p.WeightUnit,
                    IsTemporary = true // Always set to true for additional products
                }).ToList()
            ));
        }
    }
} 