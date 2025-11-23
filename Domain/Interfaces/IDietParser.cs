using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDietParser<T>
    {
        Task<List<Diet>> ParseDietAsync(T[] pdfLoadedDocument);
    }
}
