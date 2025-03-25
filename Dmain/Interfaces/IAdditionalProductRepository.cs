using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAdditionalProductRepository
    {
        /// <summary>
        /// Adds additional products to specific categories in a shopping list with the IsTemporary flag set to true
        /// </summary>
        /// <param name="shoppingListId">ID of the shopping list to add products to</param>
        /// <param name="categoryProducts">Collection of tuples with category ID and products to add to that category</param>
        /// <returns>The updated shopping list with added products</returns>
        Task<ShoppingList> AddProductsToCategoriesAsync(int shoppingListId, IEnumerable<(int categoryId, List<Product> products)> categoryProducts);
    }
}
