using AuthService.Controllers;
using AuthService.Data.DTOS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServiceTest.Controllers
{
    public class AuthControllerTest
    {
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AuthController _controller;

        public AuthControllerTest()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            _mockConfig = new Mock<IConfiguration>();

            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("this_is_a_test_key_1234567890");
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            _controller = new AuthController(_mockUserManager.Object, _mockConfig.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserCreated()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "test", Email = "test@example.com", Password = "Test123$" };
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Usuario registrado exitosamente", okResult.Value);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserCreationFails()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "test", Email = "test@example.com", Password = "badpass" };
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Invalid password" });
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                            .ReturnsAsync(identityResult);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsOk_WithValidCredentials()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "test", Password = "Test123$" };
            var user = new IdentityUser { UserName = "test", Id = "1" };

            _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.Username)).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("token", okResult.Value.ToString());
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WithInvalidCredentials()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "test", Password = "wrongpass" };
            var user = new IdentityUser { UserName = "test" };

            _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.Username)).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(false);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
