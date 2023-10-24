using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthApp.Infrastructure.Entities.DbSet;

namespace HealthApp.Infrastructure.IRepository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByEmail(string email);

        Task<User> GetByIdentityId(Guid id);

        Task<bool> UpdateUserProfile(User user);
    }
}
