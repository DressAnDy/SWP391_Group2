//namespace KoiBet.UnitTest
//{
//    using Moq;
//    using KoiBet.Data;
//    using KoiBet.DTO.User;
//    using KoiBet.Entities;
//    using Microsoft.AspNetCore.Mvc;
//    using System.Threading.Tasks;
//    using Microsoft.EntityFrameworkCore;
//    using System;
//    using System.Linq.Expressions;
//    using global::Service.AuthService;
//    using Xunit;

//    public class AuthServiceTests
//    {
//        private readonly Mock<ApplicationDbContext> _contextMock;
//        private readonly AuthService _authService;

//        public AuthServiceTests()
//        {
//            _contextMock = new Mock<ApplicationDbContext>();
//            _authService = new AuthService(_contextMock.Object);
//        }

//        // Tests for HandleLogin

//        [Fact]
//        public async Task HandleLogin_ShouldReturnUnauthorized_WhenUserNotFound()
//        {
//            // Arrange
//            var loginDto = new LoginDTO { Username = "wrongUser", Password = "password" };

//           _contextMock.Setup(c => c.Users.Include(u => u.Role)
//                .FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>(), default))
//                .ReturnsAsync((Users)null);

//            // Act
//            var result = await _authService.HandleLogin(loginDto);

//            // Assert
//            Assert.IsType<UnauthorizedObjectResult>(result);
//            Assert.Equal("Invalid username or password", ((UnauthorizedObjectResult)result).Value);
//        }

//        [Fact]
//        public async Task HandleLogin_ShouldReturnOk_WhenLoginSuccess()
//        {
//            // Arrange
//            var user = new Users
//            {
//                Username = "validUser",
//                Password = BCrypt.Net.BCrypt.HashPassword("password"),
//                Role = new Roles { role_id = "R1", role_name = "customer" }
//            };
//            var loginDto = new LoginDTO { Username = "validUser", Password = "password" };

//            _contextMock.Setup(c => c.Users.Include(u => u.Role)
//                .FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>(), default))
//                .ReturnsAsync(user);

//            // Act
//            var result = await _authService.HandleLogin(loginDto);

//            // Assert
//            Assert.IsType<OkObjectResult>(result);
//        }

//        // Tests for HandleRegister

//        [Fact]
//        public async Task HandleRegister_ShouldReturnBadRequest_WhenUsernameIsDuplicated()
//        {
//            // Arrange
//            var registerDTO = new RegisterDTO { Username = "duplicateUser", Password = "password" };

//            _contextMock.Setup(c => c.Users.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>(), default))
//                .ReturnsAsync(new Users { Username = "duplicateUser" });

//            // Act
//            var result = await _authService.HandleRegister(registerDTO);

//            // Assert
//            Assert.IsType<BadRequestObjectResult>(result);
//            Assert.Equal("User name is duplicated!", ((BadRequestObjectResult)result).Value);
//        }

//        [Fact]
//        public async Task HandleRegister_ShouldReturnOk_WhenRegisterSuccess()
//        {
//            // Arrange
//            var registerDTO = new RegisterDTO
//            {
//                Username = "newUser",
//                Password = "password",
//                fullName = "John Doe",
//                email = "john@example.com",
//                phone = "123456789"
//            };

//            _contextMock.Setup(c => c.Users.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>(), default))
//                .ReturnsAsync((Users)null);

//            // Act
//            var result = await _authService.HandleRegister(registerDTO);

//            // Assert
//            Assert.IsType<OkObjectResult>(result);
//        }
//    }
//}
