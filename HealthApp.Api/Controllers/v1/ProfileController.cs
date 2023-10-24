using HealthApp.Authentication.Models.Dtos.Response;
using HealthApp.Core.IConfiguration;
using HealthApp.Infrastructure.Dtos.Request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthApp.Api.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileController : BaseController
    {

        public ProfileController(IUnityOfWork uow, UserManager<IdentityUser> userManager) : base(uow, userManager)
        { }

        [HttpGet]
        [Route("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var logedUser = await _userManager.GetUserAsync(HttpContext.User);
            if (logedUser == null)
            {
                return BadRequest("User not found.");
            }

            var identity = new Guid(logedUser.Id);
            var profile = await _uow.Users.GetByIdentityId(identity);
            if (profile == null)
            {
                return BadRequest("User not found.");
            }


            return Ok(profile);
        }


        [HttpPost]
        [Route("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto profileUpdateDto)
        {

            if(ModelState.IsValid)
            {
                var logedUser = await _userManager.GetUserAsync(HttpContext.User);
                if (logedUser == null)
                {
                    return BadRequest("User not found.");
                }

                var identity = new Guid(logedUser.Id);
                var profile = await _uow.Users.GetByIdentityId(identity);
                if (profile == null)
                {
                    return BadRequest("User not found.");
                }

                profile.Country = profileUpdateDto.Country;
                profile.Sex = profileUpdateDto.Sex;
                profile.Address = profileUpdateDto.Address;
                profile.MobilePhone = profileUpdateDto.MobilePhone;

                var result = await _uow.Users.UpdateUserProfile(profile);

                if (result)
                {
                    await _uow.CompleteAsync();
                    return Ok(profile);
                }

                return BadRequest("Somenthing went wrong.");

            }
            else
            {
                return BadRequest(new RegistrationResponseDto
                {
                    Success = false,
                    Errors = new List<string>() { "Invalid payload." }
                });
            }
        }
    }
}
