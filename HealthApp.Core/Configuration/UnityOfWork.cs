using HealthApp.Core.IConfiguration;
using HealthApp.Infrastructure.Data;
using HealthApp.Infrastructure.IRepository;
using HealthApp.Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Core.Configuration
{
    public class UnityOfWork : IUnityOfWork, IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public IUserRepository Users { get; private set; }

        public UnityOfWork(AppDbContext dbContext, ILoggerFactory logger)
        {
            _dbContext = dbContext;
            _logger = logger.CreateLogger("db_logs");

            Users = new UserRepository(dbContext,_logger);
        }

        public async Task CompleteAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

    }
}
