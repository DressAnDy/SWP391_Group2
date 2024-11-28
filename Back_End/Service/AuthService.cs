namespace Service.AuthService;

using System;
using KoiBet.DTO.User;
using Microsoft.AspNetCore.Mvc;
using KoiBet.Data;
using Microsoft.AspNetCore.Authorization;
using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Service.JwtService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

public interface IAuthService
{
    public Task<IActionResult> HandleLogin(LoginDTO loginDto);
    public Task<IActionResult> HandleRegister(RegisterDTO registerDto);
    public Task<IActionResult> HandleRefreshToken();
    public Task<IActionResult> HandleCheckToken();
}

public class AuthService : ControllerBase, IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly JwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;

    public AuthService(ApplicationDbContext context, IConfiguration config, JwtService jwtService, IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
    {
        _context = context;
        _config = config;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    public async Task<IActionResult> HandleLogin(LoginDTO loginDto)
    {
        try
        {
            // Kiểm tra xem người dùng có tồn tại không
            var user = await _context.Users
                .Include(u => u.Role) // Bao gồm thông tin vai trò
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username); // Chỉ tìm tên người dùng

            if (user == null || !BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized("Invalid username or password");
            }

            // Tạo claims cho token
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.role_name)
        };

            // Tạo access token
            var accessToken = _jwtService.GenerateAccessToken(claims);

            // Tạo refresh token
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Lưu refresh token vào cache
            _cache.Set($"RefreshToken_{user.user_id}", refreshToken, TimeSpan.FromDays(7));

            // Đặt refresh token vào cookie
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            // Lấy thông tin người dùng cùng với tên vai trò
            var userInfo = new
            {
                user.user_id,
                user.Username,
                user.full_name,
                user.Email,
                user.Phone,
                user.Balance,
                RoleId = user.Role.role_id,
                RoleName = user.Role.role_name,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

            return Ok(userInfo);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    public async Task<IActionResult> HandleRegister(RegisterDTO registerDTO)
    {
        try
        {
            // Kiểm tra xem vai trò đã tồn tại chưa
            var roleExists = await _context.Roles.AnyAsync(r => r.role_id == "R1");
            if (!roleExists)
            {
                // Thêm vai trò mặc định nếu chưa tồn tại
                var defaultRole = new Roles
                {
                    role_id = "R1",
                    role_name = "customer" // Đặt tên vai trò là customer
                };
                _context.Roles.Add(defaultRole);
                await _context.SaveChangesAsync();
            }

            var newUserId = Guid.NewGuid().ToString();
            var hashedUserId = BCrypt.HashPassword(newUserId).Substring(0, 50);

            //Kiểm tra xem username có bị trùng lặp không
            var userNameExists = _context.Users.FirstOrDefault(u => u.Username == registerDTO.Username);
            if (userNameExists != null)
            {
                return BadRequest("User name is duplicated!");
            }

            // Tạo người dùng mới
            var newUser = new Users
            {
                user_id = hashedUserId, // Lưu hashed user_id vào cơ sở dữ liệu
                Username = registerDTO.Username,
                Password = BCrypt.HashPassword(registerDTO.Password), // Hash password
                full_name = registerDTO.fullName,
                Email = registerDTO.email,
                Phone = registerDTO.phone,
                role_id = "R1",
                Balance = registerDTO.Balance,
            };

            // Thêm người dùng vào cơ sở dữ liệu
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Trả về thông tin người dùng mới đã đăng ký
            return Ok(newUser);
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    public async Task<IActionResult> HandleRefreshToken()
    {
        try
        {
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return new UnauthorizedObjectResult("Refresh token không được cung cấp");
            }

            ClaimsPrincipal principal;
            try
            {
                principal = _jwtService.GetPrincipalFromExpiredToken(refreshToken);
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult($"Lỗi khi xác thực refresh token: {ex.Message}");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return new UnauthorizedObjectResult("Không tìm thấy ID người dùng trong token");
            }

            if (!_cache.TryGetValue($"RefreshToken_{userId}", out string storedRefreshToken))
            {
                return new UnauthorizedObjectResult("Không tìm thấy refresh token trong cache");
            }

            if (storedRefreshToken != refreshToken)
            {
                return new UnauthorizedObjectResult("Refresh token không khớp với token đã lưu");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Cập nhật refresh token mới trong cache
            _cache.Set($"RefreshToken_{userId}", newRefreshToken, TimeSpan.FromDays(7));

            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return new OkObjectResult(new { accessToken = newAccessToken });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult($"Lỗi không xác định khi làm mới token: {ex.Message}");
        }
    }

    public async Task<IActionResult> HandleCheckToken()
    {
        var user = _httpContextAccessor.HttpContext.User;

        if (!user.Identity.IsAuthenticated)
        {
            return new UnauthorizedObjectResult("Token không hợp lệ hoặc đã hết hạn");
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new BadRequestObjectResult("Không tìm thấy thông tin người dùng trong token");
        }

        var dbUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.user_id == userId);

        if (dbUser == null)
        {
            return new NotFoundObjectResult("Không tìm thấy người dùng trong cơ sở dữ liệu");
        }

        var userInfo = new
        {
            UserId = dbUser.user_id,
            Username = dbUser.Username,
            FullName = dbUser.full_name,
            Email = dbUser.Email,
            Phone = dbUser.Phone,
            RoleId = dbUser.role_id,
            RoleName = dbUser.Role?.role_name,
            Balance = dbUser.Balance
        };

        return new OkObjectResult(userInfo);
    }
}
