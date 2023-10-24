using HealthApp.Authentication.Configuration;
using HealthApp.Authentication.Models.Dtos.Generic;
using HealthApp.Authentication.Models.Dtos.Request;
using HealthApp.Authentication.Models.Dtos.Response;
using HealthApp.Core.IConfiguration;
using HealthApp.Infrastructure.Dtos.Request;
using HealthApp.Infrastructure.Entities.DbSet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HealthApp.Api.Controllers.v1
{
    public class AccountController : BaseController
    {
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AccountController(IUnityOfWork uow, UserManager<IdentityUser> userManager, 
                                JwtConfig jwtConfig, TokenValidationParameters tokenValidationParameters) : base(uow, userManager)
        { 
            _jwtConfig = jwtConfig;
            _tokenValidationParameters = tokenValidationParameters;
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

                var token = await GenerateToken(newUser);


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
                    Token = token.JwtToken,
                    RefreshToken = token.RefreshToken
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
                    var token = await GenerateToken(userExist);

                    return Ok(new LoginResponseDto
                    {
                        Success = true,  
                        Token=token.JwtToken,
                        RefreshToken=token.RefreshToken
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

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
        {
            if (ModelState.IsValid)
            {
                var resultVerif = await VerifyToken(tokenRequestDto);
                
                if (resultVerif == null)
                {
                    return BadRequest(new RegistrationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>() { "Token validation faild." }
                    });
                }

                return Ok(resultVerif);

            }
            else
            {
                return BadRequest(new LoginResponseDto
                {
                    Success = false,
                    Errors = new List<string>() { "Invalid payload." }
                });
            }
        }

        private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequestDto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var validateToken = tokenHandler.ValidateToken(tokenRequestDto.Token, _tokenValidationParameters, out var securityToken);
                
                if(securityToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    
                    if(result == false) 
                        return null;

                }

                var checkExpiryDate = long.Parse(validateToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var expiryDate = dateTime.AddSeconds(checkExpiryDate).ToUniversalTime();

                if(expiryDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "JwtToken has not expired." }
                    };
                }

                var checkRefreshTokenExist = await _uow.RefreshTokens.GetByRefreshToken(tokenRequestDto.RefreshToken);

                if(checkRefreshTokenExist == null)
                {
                    return new AuthResult() 
                    {
                        Success = false,
                        Errors = new List<string>() { "Invalid Refresh Token."}
                    };
                }

                if (checkRefreshTokenExist.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Refresh Token has expired, please Login." }
                    };
                }

                if (checkRefreshTokenExist.IsUsed)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Refresh Token has been used." }
                    };
                }

                if (checkRefreshTokenExist.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Refresh Token has been revoked." }
                    };
                }

                var jti = validateToken.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (jti != checkRefreshTokenExist.JwtId)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>() { "Refresh Token reference does dont match the Jwt Token." }
                    };
                }

                checkRefreshTokenExist.IsUsed = true;
                var updateResult = await _uow.RefreshTokens.MarkRefreshTokenUsed(checkRefreshTokenExist);
                if (updateResult)
                {
                    await _uow.CompleteAsync();
                    //generate new Jwt Token
                    var dbUser = await _userManager.FindByIdAsync(checkRefreshTokenExist.UserId);

                    if (dbUser == null)
                    {
                        return new AuthResult()
                        {
                            Success = false,
                            Errors = new List<string>() { "Error procesing the Refresh Token." }
                        };
                    }

                    var newTokens = await GenerateToken(dbUser);

                    return new AuthResult()
                    {
                        Success = true,
                        Token = newTokens.JwtToken,
                        RefreshToken = newTokens.RefreshToken
                    };
                }

                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>() { "Error procesing the Refresh Token." }
                };

            }
            catch (Exception ex)
            {

                //TODO: fix this, add error handler and add logger.
                return null;
            }

        }

        private async Task<TokenData> GenerateToken(IdentityUser user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            string keyValue = _jwtConfig.Key;
            var key = Encoding.ASCII.GetBytes(keyValue);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

                }),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtHandler.CreateToken(tokenDescription);
            var jwtToken = jwtHandler.WriteToken(token);

            // Generate Refresh Token
            var refreshToken = new RefreshToken() { 
                AddedDate = DateTime.UtcNow,
                Token = $"{RandomStringGenerator(32)}_{Guid.NewGuid()}",
                UserId = user.Id,
                IsUsed = false,
                IsRevoked = false,
                Status = 1,
                JwtId = token.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(3),
            };

            await _uow.RefreshTokens.Add(refreshToken);
            await _uow.CompleteAsync();

            var tokenData = new TokenData() { 
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token
            };

            return tokenData;  
        }

        private string RandomStringGenerator(int length)
        {
            var rnd = new Random();
            const string chars = "ABCDEFGHIKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*.-+";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}
