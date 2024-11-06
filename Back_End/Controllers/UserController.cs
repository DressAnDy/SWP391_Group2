
using KoiBet.Data;
using KoiBet.DTO.User;
using KoiBet.Entities;
using KoiBet.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        public UserController(ApplicationDbContext context, IConfiguration config, IUserService userService)
        {
            _context = context;
            _config = config;
            _userService = userService;
        }

        // POST: user/create
        [Authorize(Roles = "admin,staff")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] ManagerDTO accountDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return await _userService.HandleCreate(accountDTO);
        }

        // GET: user/{id}
        [Authorize]
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser([FromQuery] string userId)
        {
            var currentUser = HttpContext.User;
            var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            // Ki?m tra quy?n truy c?p
            if (currentUserRole != "admin")
            {
                return BadRequest(new { message = "Unauthorized!" });
            }

            return await _userService.HandleGetUser(userId);
        }

        // Get all users by role
        [Authorize]
        [HttpGet("GetAllUsersByRoleId")]
        public async Task<IActionResult> GetAllUsersByRole([FromQuery] string roleId)
        {
            var currentUser = HttpContext.User;
            var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            // Ki?m tra quy?n truy c?p
            if (currentUserRole != "admin" && currentUserRole != "manager")
            {
                return BadRequest(new { message = "Unauthorized!" });
            }

            return await _userService.HandleGetAllUsersByRole(roleId);
        }

        // PUT: user/update-profile
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserDTO _updateUserDTO)
        {
            var currentUser = HttpContext.User;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            };
            return await _userService.HandleUpdateByUsername(currentUser, _updateUserDTO);
        }

        [Authorize]
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            var currentUser = HttpContext.User;
            var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            // Ki?m tra quy?n truy c?p
            if (currentUserRole != "admin")
            {
                return BadRequest(new { message = "Unauthorized!" });
            }

            return await _userService.HandleGetAllUser();
        }

        [Authorize]
        [HttpPost("UpdateUserRole")]
        public async Task<IActionResult> UpdateUserRole([FromQuery] UpdateUserRoleDTO updateUserRoleDTO)
        {
            var currentUser = HttpContext.User;
            var currentUserRole = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            // Ki?m tra quy?n truy c?p
            if (currentUserRole != "admin")
            {
                return BadRequest(new { message = "Unauthorized!" });
            }

            return await _userService.HandleUpdateUserRole(updateUserRoleDTO);
        }

        [Authorize]
        [HttpPost("Change Password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDTO passwordChangeDTO)
        {
            var currentUser = HttpContext.User;
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return await _userService.HandleChangePassword(currentUserId, passwordChangeDTO);
        }
    }
}
