using HealthApp.Infrastructure.Entities.DbSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Infrastructure.IRepository
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetByRefreshToken(string refreshToken);
        Task<bool> MarkRefreshTokenUsed(RefreshToken refreshToken);
    }
}
