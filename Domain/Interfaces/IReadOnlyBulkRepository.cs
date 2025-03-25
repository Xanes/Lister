using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IReadOnlyBulkRepository<T> : IReadOnlyRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
    }
} 