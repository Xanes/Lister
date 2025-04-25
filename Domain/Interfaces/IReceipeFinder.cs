using Domain.Models;
using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IReceipeFinder<T>
    {
        /// <summary>
        /// Extracts recipes from a PDF document.
        /// </summary>
        /// <param name="pdfLoadedDocument">The loaded PDF document to extract recipes from</param>
        /// <returns>A list of recipes found in the document</returns>
        List<Recipe> GetReceipes(T pdfLoadedDocument);
    }
} 