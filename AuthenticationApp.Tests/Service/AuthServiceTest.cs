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

namespace AuthenticationApp.Tests.Service
{
    public class AuthServiceTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly AuthService _loginService;
        public AuthServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _configurationMock = new Mock<IConfiguration>();
            _httpContextMock = new Mock<HttpContext>();
            _redisMock = new Mock<IConnectionMultiplexer>();
            _cacheMock = new Mock<IDistributedCache>();

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



            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers["Authorization"])
                .Returns("Bearer testtoken");
            _httpContextAccessorMock.Setup(x => x.HttpContext.Connection.RemoteIpAddress)
                .Returns(IPAddress.Parse("123.45.67.89"));


            _loginService = new AuthService(_userServiceMock.Object, _configurationMock.Object, _httpContextAccessorMock.Object, _cacheMock.Object, _redisMock.Object);
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
                 .Setup(x => x.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((byte[])null);

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
            var serialized = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userDTO));
            _cacheMock
                 .Setup(x => x.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(serialized);


            //act
            var oldToken = userDTO.RefreshToken;
            var result = await _loginService.RefreshToken(refreshTokenRequest);
            var cacheKeyUpdated = $"refresh:{result.RefreshToken}";
            var cacheKeyUser = $"loggedUser:{userDTO.Email}:{_httpContextAccessorMock.Object.HttpContext.Connection.RemoteIpAddress}";
            //assert,
            Assert.NotNull(result);
            Assert.NotEqual(oldToken, result.RefreshToken);
            _cacheMock.Verify(c => c.SetAsync(
                cacheKeyUpdated,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
              Times.Once);
            _cacheMock.Verify(c => c.SetAsync(
                cacheKeyUser,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
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
            var cacheKeyUser = $"loggedUser:testuser:{_httpContextAccessorMock.Object.HttpContext.Connection.RemoteIpAddress}";

            _httpContextAccessorMock.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.Name))
                .Returns(new Claim(ClaimTypes.Name, "testuser"));

            _userServiceMock.Setup(x => x.GetUserByUsername("testuser"))
              .ReturnsAsync(new UserDTO { Email = "testuser"});

            //act
            var result = await _loginService.Logout(request);

            //assert
            Assert.True(result);
            _cacheMock.Verify(c => c.RemoveAsync(
               cacheKey,
               It.IsAny<CancellationToken>()),
             Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(
               cacheKeyUser,
               It.IsAny<CancellationToken>()),
             Times.Once);
        }

        [Fact]
        public async Task MassLogout_WhenUserLoggedIn_ShouldDislogEveryDevice()
        {
            // Arrange
            var name = "testuser";
            var userDto = new UserDTO { Email = "test@test.com" };

            // Mock do HttpContext
            _httpContextAccessorMock.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.Name))
                .Returns(new Claim(ClaimTypes.Name, name));

            // Mock do UserService
            _userServiceMock
                .Setup(x => x.GetUserByUsername(name))
                .ReturnsAsync(userDto);

            // Mock do Redis
            var endpoint = new DnsEndPoint("localhost", 6379);
            _redisMock
                .Setup(r => r.GetEndPoints(false))
                .Returns(new EndPoint[] { endpoint });

            var serverMock = new Mock<IServer>();
            _redisMock
                .Setup(r => r.GetServer(endpoint, null))
                .Returns(serverMock.Object);

            // Chave completa que o server.Keys vai retornar
            var fullKey = $"AuthenticationApp:loggedUser:{userDto.Email}:myRefreshToken";
            serverMock
                .Setup(s => s.Keys(
                    It.IsAny<int>(),
                    It.Is<RedisValue>(v => v == $"AuthenticationApp:loggedUser:{userDto.Email}:*"),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>()))
                .Returns(new[] { (RedisKey)fullKey });

            // Mock do cache para retornar o "refresh token" salvo
            var completeKey = fullKey.Substring("AuthenticationApp:".Length);
            var refreshToken = "myRefreshToken";
            _cacheMock
                .Setup(c => c.GetAsync(completeKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(refreshToken));

            // Act
            var result = await _loginService.MassLogout();

            // Assert
            Assert.True(result);
            // Verifica que buscamos a chave exata
            _cacheMock.Verify(c => c.GetAsync(completeKey, It.IsAny<CancellationToken>()), Times.Once);
            // Verifica que removemos o refresh token armazenado
            _cacheMock.Verify(c => c.RemoveAsync($"refresh:{refreshToken}", It.IsAny<CancellationToken>()), Times.Once);
            // Verifica que removemos também a chave de usuário logado
            _cacheMock.Verify(c => c.RemoveAsync(completeKey, It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}