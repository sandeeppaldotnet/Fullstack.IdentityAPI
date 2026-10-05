using Fullstack.IdentityAPI.Models;
using Fullstack.IdentityAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fullstack.IdentityAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet("dashboard")]
        public IActionResult DashboardAdmin()
        {
            return Ok(new
            {
                message = "Welcome to the admin dashboard."
            });
        }
        /// <summary>
        /// Get All Users
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users
                .Select(user => new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!
                })
                .ToList();

            return Ok(users);
        }
        /// <summary>
        /// Get User Roles by Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>

        [HttpGet("users/{email}/roles")]
        public async Task<IActionResult> GetUserRoles(
    string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                email = user.Email,
                roles
            });
        }


        [Authorize(Roles = "Customer")]
        [HttpGet("customerdashboard")]
        public IActionResult DashboardCustomer()
        {
            return Ok(new
            {
                message = "Welcome to the Dashboard Customer ."
            });
        }
    }
}
