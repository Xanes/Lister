using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PDFReader.DTOs
{
    /// <summary>
    /// Request model for adding additional products to a shopping list
    /// </summary>
    public class AdditionalProductRequest
    {
        /// <summary>
        /// The ID of the shopping list to add products to
        /// </summary>
        [Required]
        public int ShoppingListId { get; set; }

        /// <summary>
        /// List of category products to add
        /// </summary>
        [Required]
        public List<CategoryProductsDto> CategoryProducts { get; set; } = new List<CategoryProductsDto>();
    }
} 