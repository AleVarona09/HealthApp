using HealthApp.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Core.IConfiguration
{
    public interface IUnityOfWork
    {
        IUserRepository Users { get; }
        Task CompleteAsync();

    }
}
