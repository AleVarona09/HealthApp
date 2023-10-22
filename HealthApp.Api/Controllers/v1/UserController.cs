using Asp.Versioning;
using HealthApp.Core.IConfiguration;
using HealthApp.Infrastructure.Data;
using HealthApp.Infrastructure.Dtos.Request;
using HealthApp.Infrastructure.Entities.DbSet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthApp.Api.Controllers.v1
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : BaseController
    {

        public UserController(IUnityOfWork uow):base(uow)
        {}

        #region Get

        [HttpGet]
        [Route("GetByEmail")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _uow.Users.GetByEmail(email);
            if (user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("GetUser", Name = "GetUser")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _uow.Users.GetById(id);
            if (user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _uow.Users.GetAll();
            return Ok(users);
        }


        #endregion

        #region Post

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser(UserDto userDto)
        {
            if (ModelState.IsValid)
            {
                var user = new User()
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    DateOfBirth = Convert.ToDateTime(userDto.DateOfBirth),
                    Country = userDto.Country,
                    Phone = userDto.Phone,
                    Status = 1
                };

                await _uow.Users.Add(user);
                await _uow.CompleteAsync();

                return CreatedAtRoute("GetUser", new { id = user.Id }, user);
            }
            return BadRequest();
        }

        #endregion


    }
}
