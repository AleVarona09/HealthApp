using HealthApp.Authentication.Configuration;
using HealthApp.Authentication.Models.Dtos.Request;
using HealthApp.Authentication.Models.Dtos.Response;
using HealthApp.Core.IConfiguration;
using HealthApp.Infrastructure.Dtos.Request;
using HealthApp.Infrastructure.Entities.DbSet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HealthApp.Api.Controllers.v1
{
    public class AccountController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        public AccountController(IUnityOfWork uow, UserManager<IdentityUser> userManager, JwtConfig jwtConfig) : base(uow)
        { 
            _userManager = userManager;
            _jwtConfig = jwtConfig;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult> Register([FromBody] RegistrationRequestDto registrationDto)
        {
            if (ModelState.IsValid)
            {

                var userExist = await _userManager.FindByEmailAsync(registrationDto.Email);
                if (userExist != null) 
                {
                    return BadRequest(new RegistrationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>() { "Email already in use." }
                    });
                }

                var newUser = new IdentityUser()
                {
                    Email = registrationDto.Email,
                    UserName = registrationDto.Email,
                    EmailConfirmed = true
                };

                var isCreated = await _userManager.CreateAsync(newUser,registrationDto.Password);

                if (!isCreated.Succeeded)
                {
                    return BadRequest(new RegistrationResponseDto
                    {
                        Success = false,
                        Errors = isCreated.Errors.Select(e=>e.Description).ToList()
                    });
                }

                var token = GenerateToken(newUser);


                var user = new User()
                {
                    IdentityId = new Guid(newUser.Id),
                    FirstName = registrationDto.FirstName,
                    LastName = registrationDto.LastName,
                    Email = registrationDto.Email,
                    DateOfBirth = DateTime.UtcNow, // Convert.ToDateTime(userDto.DateOfBirth),
                    Country = "",
                    Phone = "",
                    Status = 1
                };

                await _uow.Users.Add(user);
                await _uow.CompleteAsync();


                return Ok(new RegistrationResponseDto
                {
                    Success= true,
                    Token = token
                });
            }
            else
            {
                return BadRequest(new RegistrationResponseDto{
                    Success=false,
                    Errors = new List<string>() { "Invalid payload."}
                });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (ModelState.IsValid)
            {
                var userExist = await _userManager.FindByEmailAsync(loginDto.Email);

                if (userExist == null)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Errors = new List<string>() { "Invalid authentication request." }
                    });
                }

                var passCorrect = await _userManager.CheckPasswordAsync(userExist, loginDto.Password);

                if (passCorrect)
                {
                    var token = GenerateToken(userExist);

                    return Ok(new LoginResponseDto
                    {
                        Success = true,  
                        Token=token
                    });
                }
                else
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Errors = new List<string>() { "Invalid authentication request." }
                    });
                }


            }
            else
            {
                return BadRequest(new LoginResponseDto
                { 
                Success=false,
                Errors = new List<string>() {"Invalid payload."}
                });
            }
        }


        private string GenerateToken(IdentityUser user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            string keyValue = _jwtConfig.Key;
            var key = Encoding.ASCII.GetBytes(keyValue);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtHandler.CreateToken(tokenDescription);
            var jwtToken = jwtHandler.WriteToken(token);

            return jwtToken;  
        }
    }
}
