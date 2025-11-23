using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IPDFProductFinderAsync<T>
    {
        /// <summary>
        /// Finds and extracts product categories and products from shopping list pages of a PDF.
        /// </summary>
        /// <param name="loadedDocument">The loaded PDF document.</param>
        /// <returns>A list of product category groups found in the document.</returns>
        Task<List<List<ProductCategoryGroup>>> FindProductsAsync(T[] loadedDocument);
    }
}
