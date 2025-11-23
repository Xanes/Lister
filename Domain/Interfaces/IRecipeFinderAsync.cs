using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRecipeFinderAsync<T>
    {
        Task<List<List<Recipe>>> FindRecipes(T[] loadedDocument);
    }
}
