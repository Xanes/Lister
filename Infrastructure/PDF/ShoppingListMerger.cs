using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.PDF
{
    public class ShoppingListMerger
    {
        public List<ProductCategoryGroup> MergeShoppingLists(List<ProductCategoryGroup> shoppingListA, List<ProductCategoryGroup> shoppingListB)
        {
            var ListA = shoppingListA.ToList();
            var ListB = shoppingListB.ToList();

            List<ProductCategoryGroup> mergedShoppingList = new List<ProductCategoryGroup>();

            foreach (var category in shoppingListA)
            {
                var mergedCategory = new ProductCategoryGroup()
                {
                    Category = new Category 
                    {
                        Name = category.Category?.Name
                    }, 
                    Products = new List<Product>()
                };
                var categoryB = ListB.FirstOrDefault(x => x.Category?.Name == category.Category?.Name);
                var productsB = categoryB?.Products?.ToList();

                foreach (var product in category.Products ?? new List<Product>())
                {
                    var mergedProduct = new Product()
                    {
                        Name = product.Name,
                        Quantity = product.Quantity,
                        QuantityUnit = product.QuantityUnit,
                        Weight = product.Weight,
                        WeightUnit = product.WeightUnit
                    };
                    var productB = productsB?.FirstOrDefault(x => x.Name == product.Name);
                    if (productB != null)
                    {
                        mergedProduct.Quantity += productB.Quantity;
                        mergedProduct.Weight += productB.Weight;
                        mergedProduct.Quantity = mergedProduct.Quantity != null ? Math.Round(mergedProduct.Quantity.Value, 2) : null;
                        mergedProduct.Weight = mergedProduct.Weight != null ? Math.Round(mergedProduct.Weight.Value, 2) : null;
                        productsB?.Remove(productB);
                    }
                    mergedCategory.Products.Add(mergedProduct);
                }
                if (productsB?.Any() ?? false && categoryB != null)
                {
                    mergedCategory.Products.AddRange(productsB);

                }
                if (categoryB != null)
                {
                    ListB.Remove(categoryB);
                }
                mergedShoppingList.Add(mergedCategory);
            }

            if (ListB.Any())
            {
                mergedShoppingList.AddRange(ListB);
            }
            return mergedShoppingList;
        }
    }
}
