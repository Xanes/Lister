using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PDFReader.DTOs
{
    /// <summary>
    /// DTO for products grouped by category
    /// </summary>
    public class CategoryProductsDto
    {
        /// <summary>
        /// The ID of the category to add products to
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// List of products to add to the category
        /// </summary>
        [Required]
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
} 