using KoiBet.Data;
using KoiBet.DTO.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KoiBet.Entities;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using BCrypt.Net;
using KoiBet.DTO.Competition;

namespace KoiBet.Service
{
    public interface IUserService
    {
        Task<IActionResult> HandleCreate(ManagerDTO managerDTO);
        Task<IActionResult> HandleUpdateByUsername(ClaimsPrincipal currentUser, UpdateUserDTO _updateDTO);
        Task<IActionResult> HandleGetUser(string user_id);
        Task<IActionResult> HandleGetAllUser();
        Task<IActionResult> HandleUpdateUserRole(UpdateUserRoleDTO updateUserRoleDTO);
        Task<IActionResult> HandleChangePassword(string userId, PasswordChangeDTO passwordChangeDTO);
        Task<IActionResult> HandleGetAllUsersByRole(string roleId);
        Task<IActionResult> HandleGetUserStatistics();
    }

    public class UserService : ControllerBase, IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tạo user mới
        public async Task<IActionResult> HandleCreate(ManagerDTO managerDTO)
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.role_id == managerDTO.role_id);

            if (!roleExists)
            {
                return BadRequest("Role does not exist");
            }

            var newUser = new Users
            {
                user_id = Guid.NewGuid().ToString(), // Sử dụng Guid
                Username = managerDTO.user_name,
                full_name = managerDTO.full_name,
                Password = BCrypt.Net.BCrypt.HashPassword(managerDTO.password), // Hash mật khẩu
                Email = managerDTO.email,
                Phone = managerDTO.phone,
                role_id = managerDTO.role_id,
                Balance = 0
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(newUser);
        }

        // Cập nhật user
        public async Task<IActionResult> HandleUpdateByUsername(ClaimsPrincipal currentUser, UpdateUserDTO updateDTO)
        {
            var currentUserId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Find the user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == currentUserId);

            if (user == null)
            {
                return NotFound("User not found!");
            }

            // Update user information only if the new value is provided
            if (!string.IsNullOrWhiteSpace(updateDTO.full_name))
            {
                user.full_name = updateDTO.full_name;
            }

            if (!string.IsNullOrWhiteSpace(updateDTO.email))
            {
                user.Email = updateDTO.email;
            }

            if (!string.IsNullOrWhiteSpace(updateDTO.phone))
            {
                user.Phone = updateDTO.phone;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // Lấy thông tin user theo ID hoặc Username
        public async Task<IActionResult> HandleGetUser(string user_id)
        {
            if (string.IsNullOrEmpty(user_id))
            {
                return BadRequest("ID must be provided");
            }

            //Users user = null;

            //if (Guid.TryParse(user_id, out var userIdGuid))
            //{
            //    user = await _context.Users
            //        .Include(u => u.Role) // Thêm dòng này nếu bạn muốn lấy thông tin vai trò
            //        .FirstOrDefaultAsync(u => u.user_id == userIdGuid.ToString());
            //}

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.user_id == user_id);

            if (user == null)
            {
                return new NotFoundObjectResult("Không tìm thấy người dùng");
            }

            return new OkObjectResult(new
            {
                UserId = user.user_id,
                Username = user.Username,
                FullName = user.full_name,
                Email = user.Email,
                Phone = user.Phone,
                RoleId = user.role_id,
                RoleName = user.Role?.role_name, // Thêm dòng này nếu bạn đã include Role
                Balance = user.Balance
            });
        }

        public async Task<IActionResult> HandleGetAllUser()
        {
            var user = _context.Users
                .Include(u => u.Role);

            var result = user
                .OrderByDescending(c => c.Username)
                .ToList();

            if (result == null)
            {
                return new NotFoundObjectResult("Không tìm thấy người dùng");
            }

            return new OkObjectResult(result);
        }

        public async Task<IActionResult> HandleGetAllUsersByRole(string roleId)
        {
            try
            {
                var userList = _context.Users
                    .Where(c => c.role_id == roleId.ToUpper())
                    .OrderByDescending(c => c.Username)
                    .ToList();

                if (userList == null)
                {
                    return BadRequest("Không tìm thấy người dùng");
                }

                return new OkObjectResult(userList);
            }
            catch(Exception ex) {
            {
                return BadRequest($"Error deleting referee: {ex.Message}");
            }
        }
        }

        public async Task<IActionResult> HandleUpdateUserRole(UpdateUserRoleDTO updateUserRoleDTO)
        {
            // Find the user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.user_id == updateUserRoleDTO.user_id);

            if (user == null)
            {
                return NotFound("User not found!");
            }

            // Update user information only if the new value is provided
            if (!string.IsNullOrWhiteSpace(updateUserRoleDTO.role_id))
            {
                user.role_id = updateUserRoleDTO.role_id;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        public async Task<IActionResult> HandleChangePassword(string userId, PasswordChangeDTO passwordChangeDTO)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("ID must be provided");
            }

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.user_id == userId);

            if (user == null)
            {
                return new NotFoundObjectResult("User Not Found!");
            }

            if (user == null || !BCrypt.Net.BCrypt.Verify(passwordChangeDTO.OldPassword, user.Password))
            {
                return Unauthorized("Invalid username or password");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(passwordChangeDTO.NewPassword);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        public async Task<IActionResult> HandleGetUserStatistics()
        {
            try
            {
                var userQuery = _context.Users
                    .AsQueryable();

                var userStatics = userQuery
                    .GroupBy(c => c.Role.role_name)
                    .ToDictionary(c => c.Key, c => c.Count());

                var result = new UserStatisticsDTO
                {
                    UserRoles = userStatics
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving user statistics: {ex.Message}");
            }
        }
    }
}
