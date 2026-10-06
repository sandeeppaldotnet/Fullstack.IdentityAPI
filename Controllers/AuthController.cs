using Fullstack.IdentityAPI.Data;
using Fullstack.IdentityAPI.Models;
using Fullstack.IdentityAPI.Models.DTOs;
using Fullstack.IdentityAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fullstack.IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(
       UserManager<ApplicationUser> userManager,
       SignInManager<ApplicationUser> signInManager,
       RoleManager<IdentityRole> roleManager, JwtTokenService jwtTokenService,ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtTokenService = jwtTokenService;
            _dbContext = applicationDbContext;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(
    RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser is not null)
            {
                return BadRequest(new
                {
                    message = "A user with this email already exists."
                });
            }

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email,
                AdharNumber = request.AdharNumber
               
            };

            var result = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email

            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(
    LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            //    var result = await _signInManager.PasswordSignInAsync(
            //user,
            //request.Password,
            //isPersistent: false,
            //lockoutOnFailure: true);


            if (!result.Succeeded)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

          //  var ( Token,ExpiresAtUtc) = await _jwtTokenService.CreateAccessTokenAsync(user);
            var tokenResult =
        await _jwtTokenService
            .CreateAccessTokenAsync(user);

            var refreshToken=_jwtTokenService.CreateRefreshToken();

            var refeshTokenHahs = _jwtTokenService.HashRefreshToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                TokenHash = refeshTokenHahs,
                UserId = user.Id,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();


            return Ok(new LoginResponse
            {
                AccessToken = tokenResult.Token,

                TokenType = "Bearer",
                RefreshToken = refreshToken,

                ExpiresIn =
            (int)(
                tokenResult.ExpiresAtUtc
                - DateTime.UtcNow)
                .TotalSeconds,

                User = new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!
                }
            });

        }

        [HttpPost("roles/{roleName}")]
        public async Task<IActionResult> CreateRole(
    string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest(new
                {
                    message = "Role already exists."
                });
            }

            var result = await _roleManager.CreateAsync(
                new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new
            {
                message = $"Role '{roleName}' created successfully."
            });
        }



        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(
    string email,
    string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest(new
                {
                    message = "Role does not exist."
                });
            }

            var result = await _userManager.AddToRoleAsync(
                user,
                roleName);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new
            {
                message = $"User assigned to role '{roleName}'."
            });
        }

        // Refresh token endpoint



        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
    RefreshTokenRequest request)
        {
            //check if the refresh token is valid/Exists in the
            //database and not expired or revoked
            var tokenHash =
                _jwtTokenService.HashRefreshToken(
                    request.RefreshToken);

            //check if the refresh token exists in the database

            var storedToken =
                await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(
                        x => x.TokenHash == tokenHash);

            if (storedToken is null)
            {
                return Unauthorized(new
                {
                    message = "Invalid refresh token."
                });
            }


            if (storedToken.RevokedAtUtc.HasValue)
            {
                return Unauthorized(new
                {
                    message = "Refresh token has been revoked."
                });
            }


            if (storedToken.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return Unauthorized(new
                {
                    message = "Refresh token has expired."
                });
            }


            var user =
                await _userManager.FindByIdAsync(
                    storedToken.UserId);

            if (user is null)
            {
                return Unauthorized(new
                {
                    message = "User not found."
                });
            }

            //generate a new access token and refresh token, revoke the old refresh token,
            //and save the new refresh token in the database
            var newAccessToken =
                await _jwtTokenService
                    .CreateAccessTokenAsync(user);

            //generate a new refresh token

            var newRefreshToken =
                _jwtTokenService.CreateRefreshToken();

            //hash the new refresh token
            var newRefreshTokenHash =
                _jwtTokenService.HashRefreshToken(
                    newRefreshToken);

            //revoke the old refresh token and set the replaced by
            //property to the new refresh token hash

            storedToken.RevokedAtUtc =
                DateTime.UtcNow;

            storedToken.ReplacedByTokenHash =
                newRefreshTokenHash;

            var newRefreshTokenEntity =
                new RefreshToken
                {
                    UserId = user.Id,

                    TokenHash =
                        newRefreshTokenHash,

                    CreatedAtUtc =
                        DateTime.UtcNow,

                    ExpiresAtUtc =
                        DateTime.UtcNow.AddDays(7)
                };

            _dbContext.RefreshTokens.Add(
                newRefreshTokenEntity);

            await _dbContext.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                AccessToken =
                    newAccessToken.Token,

                TokenType = "Bearer",

                ExpiresIn =
                    (int)(
                        newAccessToken.ExpiresAtUtc
                        - DateTime.UtcNow)
                        .TotalSeconds,

                RefreshToken =
                    newRefreshToken,

                User = new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!
                }
            });
        }


    }
}
