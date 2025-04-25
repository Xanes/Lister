using Domain.Models;
using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IShoppingListMerger
    {
        /// <summary>
        /// Merges two shopping lists (represented as lists of product category groups).
        /// </summary>
        /// <param name="shoppingListA">The first shopping list.</param>
        /// <param name="shoppingListB">The second shopping list.</param>
        /// <returns>A merged shopping list containing combined quantities and weights for common products.</returns>
        List<ProductCategoryGroup> MergeShoppingLists(List<ProductCategoryGroup> shoppingListA, List<ProductCategoryGroup> shoppingListB);
    }
} 