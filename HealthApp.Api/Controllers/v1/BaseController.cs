using Asp.Versioning;
using HealthApp.Core.IConfiguration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthApp.Api.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class BaseController : ControllerBase
    {
        public IUnityOfWork _uow;

        public BaseController(IUnityOfWork uow)
        {
            _uow = uow;
        }

    }
}
