using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Infrastructure.IRepository
{
    public interface IGenericRepository<T> where T : class
    {

        Task<IEnumerable<T>> GetAll();

        Task<T> GetById(Guid id);

        Task<bool> Add(T entity);

        Task<bool> Delete(Guid id, string userId);

        //update or insert if not exist
        Task<bool> Upsert(T entity);


    }
}
