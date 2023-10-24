using HealthApp.Infrastructure.Data;
using HealthApp.Infrastructure.Entities.DbSet;
using HealthApp.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Infrastructure.Repository
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext dbContext, ILogger logger) : base(dbContext, logger)
        {
    
        }
        public override async Task<IEnumerable<RefreshToken>> GetAll()
        {
            try
            {
                return await dbSet.Where(x => x.Status == 1).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetAll method", typeof(UserRepository));
                return new List<RefreshToken>();
            }
        }

        public async Task<RefreshToken> GetByRefreshToken(string refreshToken)
        {
            try
            {
                return await dbSet.Where(x => x.Token == refreshToken).AsNoTracking().FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByRefreshToken", typeof(UserRepository));
                return null;
            }
        }

        public async Task<bool> MarkRefreshTokenUsed(RefreshToken refreshToken)
        {
            try
            {
                var token =  await dbSet.Where(x => x.Token == refreshToken.Token).AsNoTracking().FirstOrDefaultAsync();
                
                if (token == null) return false;

                token.IsUsed = refreshToken.IsUsed;
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} MarkRefreshTokenUsed", typeof(UserRepository));
                return false;
            }
        }
    }
}
