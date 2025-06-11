using AuthenticationApp.Business.Services;
using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using StackExchange.Redis;
using System.Net.Http;
using System.Net;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AuthenticationApp.Domain.Models;
using AuthenticationApp.Infra.Interfaces;

namespace AuthenticationApp.Tests.Service
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<IRedisCacheService> _cacheMock;
        private readonly Mock<IQueuePublisher> _queueMock;
        private readonly Mock<IUserContextService> _userContextMock = new Mock<IUserContextService>();
        private readonly AuthService _loginService;
        public AuthServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _configurationMock = new Mock<IConfiguration>();
            _cacheMock = new Mock<IRedisCacheService>();
            _queueMock = new Mock<IQueuePublisher>();
            

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

            var redisSectionMock = new Mock<IConfigurationSection>();
            redisSectionMock.Setup(s => s.Value).Returns("5");
            _configurationMock
               .Setup(c => c.GetSection("RedisSettings:ExpirationMinutes"))
                .Returns(redisSectionMock.Object);


            _userContextMock = new Mock<IUserContextService>();

            _userContextMock.Setup(x => x.UserIpAddress)
                .Returns("123.45.67.89");


            _loginService = new AuthService(_userServiceMock.Object
                , _configurationMock.Object
                , _userContextMock.Object
                , _cacheMock.Object
                , _queueMock.Object);
        }

        [Fact]
        public async Task Login_WhenUserNotFound_ThrowsInvalidCredentialException()
        {
            // Arrange
            var loginDTO = new LoginRequest { Username = "testuser", Password = "password" };
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
            var loginDTO = new LoginRequest { Username = "testuser", Password = "password" };
            var userDTO = new UserDTO { Username = "testuser", Email = "test@test.com", RefreshToken = "MockToken" };
            _userServiceMock.Setup(x => x.GetUserByCredentials(loginDTO.Username, loginDTO.Password))
                .ReturnsAsync(userDTO);

           // _userServiceMock.Setup(x => x.UpdateRefreshToken(userDTO.Username, It.IsAny<string>()))
              //  .Returns(Task.CompletedTask);

            //act
            int expirationMinutes = _configurationMock.Object.GetValue<int>("JWTToken:ExpirationMinutes");
            string initialRefreshToken = userDTO.RefreshToken;
            var result = await _loginService.Login(loginDTO);

            //assert
            Assert.NotNull(result);
            Assert.NotEqual(initialRefreshToken, result.RefreshToken);
            Assert.Equal(userDTO.RefreshToken, result.RefreshToken);
            Assert.Equal(60, expirationMinutes);
            //_userServiceMock.Verify(x => x.UpdateRefreshToken(userDTO.Username, result.RefreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_WhenInvalidRefreshToken_ShouldThrowInvalidOperationException()
        {
            //Arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "invalidtoken" };
            
            var cacheKey = $"refresh:{refreshTokenRequest.RefreshToken}";
            UserDTO? emptyDto = null;
            var serialized = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(emptyDto));
            _cacheMock
                 .Setup( x => x.GetAsync(cacheKey))
                 .ReturnsAsync((string)null);

            //Act and Assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _loginService.RefreshToken(refreshTokenRequest));
            Assert.Equal("Refresh token inválido.", message.Message);
        }

        [Fact]
        public async Task RefreshToken_WhenValidRefreshToken_ShouldGenerateNewLoginResponse()
        {
            //arrange
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "validtoken" };
            var userDTO = new UserDTO { Username = "testuser", Email = "test@test.com", RefreshToken = "validtoken" };
            var userDTOJson = JsonSerializer.Serialize(userDTO);
            _userServiceMock.Setup(x => x.GetUserByRefreshToken(refreshTokenRequest.RefreshToken))
                .ReturnsAsync(userDTO);

            var cacheKey = $"refresh:{refreshTokenRequest.RefreshToken}";
            var serialized = JsonSerializer.Serialize(userDTO);
            _cacheMock
                 .Setup(x => x.GetAsync(cacheKey))
                 .ReturnsAsync((string)serialized);


            //act
            var oldToken = userDTO.RefreshToken;
            var result = await _loginService.RefreshToken(refreshTokenRequest);
            var cacheKeyUpdated = $"refresh:{result.RefreshToken}";
            var cacheKeyUser = $"loggedUser:{userDTO.Email}:{_userContextMock.Object.UserIpAddress}";
            //assert,
            Assert.NotNull(result);
            Assert.NotEqual(oldToken, result.RefreshToken);
            _cacheMock.Verify(c => c.SetAsync(
                cacheKeyUpdated,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()),
              Times.Once);
            _cacheMock.Verify(c => c.SetAsync(
                cacheKeyUser,
                It.IsAny<string>(),
                It.IsAny<TimeSpan?>()),
              Times.Once);
        }

        [Fact]
        public async Task Logout_WhenUserHasName_ShouldInvalidRefreshTokenWithEmptyValue()
        {
            //arrange
            RefreshTokenRequest request = new RefreshTokenRequest
            {
                RefreshToken = "validtoken"
            };

            var cacheKey = $"refresh:{request.RefreshToken}";
            var cacheKeyUser = $"loggedUser:testuser:{_userContextMock.Object.UserIpAddress}";

            _userContextMock.Setup(x => x.UserName)
                .Returns( "testuser");

            _userServiceMock.Setup(x => x.GetUserByUsername("testuser"))
              .ReturnsAsync(new UserDTO { Email = "testuser"});

            //act
            var result = await _loginService.Logout(request);

            //assert
            Assert.True(result);
            _cacheMock.Verify(c => c.RemoveAsync(
               cacheKey),
             Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(
               cacheKeyUser),
             Times.Once);
        }

        [Fact]
        public async Task MassLogout_WhenUserLoggedIn_ShouldInvokeCacheServiceMassLogout()
        {
            // Arrange
            var username = "testuser";
            var email = "test@test.com";
            var ip = "123.45.67.89";
            var userDto = new UserDTO { Email = email };

            _userContextMock
                .Setup(x => x.UserName)
                .Returns(username);

            _userServiceMock
                .Setup(x => x.GetUserByUsername(username))
                .ReturnsAsync(userDto);

            // Prepara o mock para não lançar exceção
            _cacheMock
                .Setup(c => c.MassLogoutAsync(email, ip))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _loginService.MassLogout();

            // Assert
            Assert.True(result);
            _cacheMock.Verify(
                c => c.MassLogoutAsync(email, ip),
                Times.Once,
                "Deve chamar exatamente uma vez o método MassLogoutAsync do cache"
            );
        }

    }
}