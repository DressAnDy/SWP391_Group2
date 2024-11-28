using KoiBet.DTO.User;
using KoiBet.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Service.JwtService;
using Service.AuthService;
using Service.Payload;
using Microsoft.AspNetCore.Identity;
using KoiBet.Data;

namespace KoiBet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly JwtService _jwtService;
        private readonly IAuthService _authService;

        public AuthController(ApplicationDbContext context, IConfiguration config, JwtService jwtService, IAuthService authService)
        {
            _context = context;
            _config = config;
            _jwtService = jwtService;
            _authService = authService;
        }

        // POST: auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            return await _authService.HandleLogin(loginDTO);
        }

        // POST: auth/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            return await _authService.HandleRegister(registerDTO);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            return await _authService.HandleRefreshToken();
        }

        [Authorize]
        [HttpPost("check-token")]
        public async Task<IActionResult> CheckToken()
        {
            return await _authService.HandleCheckToken();
        }
    }
}
