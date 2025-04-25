using Domain.Models;
namespace Domain.Interfaces
{
    public interface IPDFProductsFinder<T>
    {
        /// <summary>
        /// Finds and extracts product categories and products from shopping list pages of a PDF.
        /// </summary>
        /// <param name="loadedDocument">The loaded PDF document.</param>
        /// <returns>A list of product category groups found in the document.</returns>
        List<ProductCategoryGroup> FindProducts(T loadedDocument);
    }
}