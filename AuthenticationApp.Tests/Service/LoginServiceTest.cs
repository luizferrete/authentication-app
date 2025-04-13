using AuthenticationApp.Business.Services;
using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApp.Tests.Service
{
    public class LoginServiceTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly LoginService _loginService;
        public LoginServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _configurationMock = new Mock<IConfiguration>();
            _httpContextMock = new Mock<HttpContext>();

            var expirationSectionMock = new Mock<IConfigurationSection>();
            expirationSectionMock.Setup(x => x.Value).Returns("60");
            _configurationMock.Setup(x => x.GetSection("JWTToken:ExpirationMinutes"))
                .Returns(expirationSectionMock.Object);
            _configurationMock.Setup(x => x["JWTToken:PrivateKey"])
                .Returns("MySuperSecretPrivateKey1234567890");
            _configurationMock.Setup(x => x["JWTToken:Issuer"])
                .Returns("testissuer");
            _configurationMock.Setup(x => x["JWTToken:Audience"])
                .Returns("testaudience");
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers["Authorization"])
                .Returns("Bearer testtoken");

            _loginService = new LoginService(_userServiceMock.Object, _configurationMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task Login_WhenUserNotFound_ThrowsInvalidCredentialException()
        {
            // Arrange
            var loginDTO = new LoginDTO { Username = "testuser", Password = "password" };
            _userServiceMock.Setup(x => x.GetUserByCredentials(loginDTO.Username, loginDTO.Password))
                .ThrowsAsync(new InvalidCredentialException("Usuário ou senha inválida."));
            // Act and Assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _loginService.Login(loginDTO));
            Assert.Equal("Usuário ou senha inválida.", message.Message);
        }

        [Fact]
        public async Task Login_WhenValidLogin_ShouldReturnLoginResponse()
        {
            //arrange
            var loginDTO = new LoginDTO { Username = "testuser", Password = "password" };
            var userDTO = new UserDTO { Username = "testuser", Email = "test@test.com", RefreshToken = "MockToken" };
            _userServiceMock.Setup(x => x.GetUserByCredentials(loginDTO.Username, loginDTO.Password))
                .ReturnsAsync(userDTO);

            _userServiceMock.Setup(x => x.UpdateRefreshToken(userDTO.Username, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            //act
            int expirationMinutes = _configurationMock.Object.GetValue<int>("JWTToken:ExpirationMinutes");
            string initialRefreshToken = userDTO.RefreshToken;
            var result = await _loginService.Login(loginDTO);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(initialRefreshToken, result.RefreshToken);
            Assert.Equal(userDTO.RefreshToken, result.RefreshToken);
            Assert.Equal(60, expirationMinutes);
            _userServiceMock.Verify(x => x.UpdateRefreshToken(userDTO.Username, result.RefreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_WhenInvalidRefreshToken_ShouldThrowInvalidOperationException()
        {
            //Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "invalidtoken" };
            _userServiceMock.Setup(x => x.GetUserByRefreshToken(refreshTokenRequest.RefreshToken))
                .ThrowsAsync(new InvalidOperationException("Usuário não encontrado."));

            //Act and Assert
            var message = await Assert.ThrowsAsync<InvalidOperationException>(() => _loginService.RefreshToken(refreshTokenRequest));
            Assert.Equal("Usuário não encontrado.", message.Message);
        }

        [Fact]
        public async Task RefreshToken_WhenValidRefreshToken_ShouldGenerateNewLoginResponse()
        {
            //arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "validtoken" };
            var userDTO = new UserDTO { Username = "testuser", Email = "test@test.com", RefreshToken = "validtoken" };

            _userServiceMock.Setup(x => x.GetUserByRefreshToken(refreshTokenRequest.RefreshToken))
                .ReturnsAsync(userDTO);

            //act
            var oldToken = userDTO.RefreshToken;
            var result = await _loginService.RefreshToken(refreshTokenRequest);

            //assert,
            Assert.NotNull(result);
            Assert.NotEqual(oldToken, result.RefreshToken);
            Assert.Equal(userDTO.RefreshToken, result.RefreshToken);
            _userServiceMock.Verify(x => x.UpdateRefreshToken(userDTO.Username, result.RefreshToken), Times.Once);
        }

        [Fact]
        public async Task Logout_WhenUserHasName_ShouldInvalidRefreshTokenWithEmptyValue()
        {
            //arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);

            _userServiceMock.Setup(x => x.UpdateRefreshToken("testuser", string.Empty))
                .Returns(Task.CompletedTask);

            //act
            var result = await _loginService.Logout();

            //assert
            Assert.True(result);
            _userServiceMock.Verify(x => x.UpdateRefreshToken("testuser", string.Empty), Times.Once);
        }

        [Fact]
        public async Task Logout_WhenUserHasNoName_ShouldReturnFalse()
        {
            //arrange
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);

            //act
            var result = await _loginService.Logout();

            //assert
            Assert.False(result);
        }

        [Fact]
        public async Task ChangePassword_WhenSamePassword_ShouldThrowInvalidOperationException()
        {
            //arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);
            var changePasswordRequest = new ChangePasswordRequest
            {
                OldPassword = "oldpassword",
                NewPassword = "oldpassword"
            };
            //act and assert
            var message = await Assert.ThrowsAsync<InvalidOperationException>(() => _loginService.ChangePassword(changePasswordRequest));
            Assert.Equal("A nova senha não pode ser igual à senha atual.", message.Message);
        }
    }
}