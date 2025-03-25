using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRepository<T, TChange> : IReadOnlyBulkRepository<T> where T : IEntity 
    {
        Task<T> CreateAsync(T entity);
        Task DeleteAsync(int id);
        Task UpdateAsync(List<TChange> entity);
        Task ResetAsync(int shoppingListId);
    }
}
