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
        [AllowAnonymous]
        [HttpGet("DashboardAdminPublic")]
        public IActionResult DashboardAdminPublic()
        {
            return Ok(new
            {
                message = "Welcome to the admin dashboard."
            });
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Customer,Admin")]
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

        [HttpGet("myclaims")]
        [Authorize]
        public ActionResult GetMyClaims()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var emailID = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(r => r.Value).ToList();

            var department = User.FindFirst("Department")?.Value;

            var userName = User.Identity?.Name;
            return Ok(new
            {
                UserId = userId,
                UserName = userName,
                Email = emailID,
                Roles = roles,
                FullName = userName,
                Department = department
            });
        }

        [Authorize(Policy="AdminOnly")]
        [HttpGet("Admin-Data")]
        public IActionResult AdminData()
        {
            return Ok(new
            {
                message = "Welcome to the Dashboard Customer ."
            });
        }
        [Authorize(Policy = "EmployeeManagment")]
        [HttpGet("EmployeeManagment")]
        public IActionResult EmployeeManagment()
        {
            return Ok(new
            {
                message = "Welcome to the EmployeeManagment ."
            });
        }
    }
}
