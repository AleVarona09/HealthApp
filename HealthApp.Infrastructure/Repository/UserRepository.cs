using HealthApp.Infrastructure.Data;
using HealthApp.Infrastructure.Entities.DbSet;
using HealthApp.Infrastructure.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Infrastructure.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext dbContext, ILogger logger) : base(dbContext, logger)
        {
            
        }
        
        public override async Task<IEnumerable<User>> GetAll()
        {
            try
            {
                return await dbSet.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetAll method", typeof(UserRepository));
                return new List<User>();
            }
        }

        public async Task<User> GetByEmail(string email)
        {
            try
            {
                return await dbSet.FirstOrDefaultAsync(x => x.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByEmail method", typeof(UserRepository));
                return new User();
            }
        }
    }
}
