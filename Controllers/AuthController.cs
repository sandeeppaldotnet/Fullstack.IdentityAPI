using Fullstack.IdentityAPI.Models;
using Fullstack.IdentityAPI.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fullstack.IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthController(
       UserManager<ApplicationUser> userManager,
       SignInManager<ApplicationUser> signInManager,RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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

            //var result = await _signInManager.CheckPasswordSignInAsync(
            //    user,
            //    request.Password,
            //    lockoutOnFailure: true);

            var result = await _signInManager.PasswordSignInAsync(
        user,
        request.Password,
        isPersistent: false,
        lockoutOnFailure: true);


            if (!result.Succeeded)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            return Ok(new
            {
                message = "Login successful.",
                user = new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email
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




    }
}
