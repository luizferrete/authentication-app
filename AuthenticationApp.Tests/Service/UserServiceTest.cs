﻿using AuthenticationApp.Business.Services;
using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Models;
using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using AuthenticationApp.Interfaces.DataAccess;
using Microsoft.AspNetCore.Http;
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
    public class UserServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly UserService _userService;

        public UserServiceTest() 
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContextMock = new Mock<HttpContext>();
            _userService = new UserService(_userRepositoryMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task CreateUser_WhenUserAlreadyExists_ThrowsInvalidCredentialException()
        {
            // Arrange
            var userDTO = new CreateUserDTO { Username = "testuser", Password = "password", Email = "testemail@gmail.com" };
            var existingUser = new LoginUserDTO { Username = "testuser" };
            _userRepositoryMock.Setup(x => x.GetUserByCredentials(userDTO.Username))
                .ReturnsAsync(existingUser);

            //act and assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _userService.CreateUser(userDTO));
            _userRepositoryMock.Verify(repo => repo.CreateUser(It.IsAny<CreateUserDTO>()), Times.Never);
            Assert.Equal("Usuário já existe.", message.Message);
        }

        [Fact]
        public async Task CreateUser_WhenValidUser_ShouldCreateUser()
        {
            //arrange
            var userDTO = new CreateUserDTO { Username = "testuser", Password = "password", Email = "testemail@gmail.com" };
            _userRepositoryMock.Setup(x => x.GetUserByCredentials(userDTO.Username))
                .ReturnsAsync((LoginUserDTO)null);
            //act
            await _userService.CreateUser(userDTO);

            //assert
            _userRepositoryMock.Verify(repo => repo.CreateUser(
            It.Is<CreateUserDTO>(
                dto => dto.Username == "testuser" && dto.Password != "password"
            )), Times.Once);
        }

        [Fact]
        public async Task GetUserByCredentials_WhenUserNotFound_ThrowsInvalidCredentialException()
        {
            //arrange
            _userRepositoryMock.Setup(x => x.GetUserByCredentials("nonexistentuser"))
                .ReturnsAsync((LoginUserDTO)null);
            //act and assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _userService.GetUserByCredentials("nonexistentuser", "password"));
            Assert.Equal("Usuário ou senha inválida.", message.Message);
        }

        [Fact]
        public async Task GetUserByCredentials_WhenPasswordInvalid_ThrowsInvalidCredentialException()
        {
            //arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
            var user = new LoginUserDTO { Username = "testuser", Password = hashedPassword };
            _userRepositoryMock.Setup(x => x.GetUserByCredentials("testuser"))
                .ReturnsAsync(user);

            //act and assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _userService.GetUserByCredentials("testuser", "wrongpassword"));
            Assert.Equal("Usuário ou senha inválida.", message.Message);
        }

        [Fact]
        public async Task GetUserByCredentials_WhenUserAndPasswordAreValid_ShouldReturnLoginUserDTO()
        {
            //arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
            var user = new LoginUserDTO { Username = "testuser", Password = hashedPassword };
            _userRepositoryMock.Setup(x => x.GetUserByCredentials("testuser"))
                .ReturnsAsync(user);
            //act
            var result = await _userService.GetUserByCredentials("testuser", "correctpassword");

            //assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task UpdateRefreshToken_WhenUserNotFound_ThrowsInvalidCredentialException()
        {
            //arrange
            _userRepositoryMock.Setup(x => x.GetUserByUsername("nonexistentuser"))
                .ReturnsAsync((UserDTO)null);
            //act and assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _userService.UpdateRefreshToken("nonexistentuser", "newtoken"));
            Assert.Equal("Usuário não encontrado.", message.Message);
        }

        [Fact]
        public async Task UpdateRefreshToken_WhenValidUser_ShouldCallRepo()
        {
            //arrange
            _userRepositoryMock.Setup(x => x.GetUserByUsername("testuser"))
                .ReturnsAsync(new UserDTO { Username = "testuser" });
            //act
            await _userService.UpdateRefreshToken("testuser", "newtoken");
            //assert
            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.Is<UserDTO>(u => u.Username == "testuser" && u.RefreshToken == "newtoken")), Times.Once);

        }

        [Fact]
        public async Task GetUserByRefreshToken_WhenUserNotFound_ThrowsInvalidCredentialException()
        {
            //arrange
            _userRepositoryMock.Setup(x => x.GetUserByRefreshToken("invalidtoken"))
                .ReturnsAsync((UserDTO)null);
            //act and assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _userService.GetUserByRefreshToken("invalidtoken"));
            Assert.Equal("Refresh token inválido.", message.Message);
        }

        [Fact]
        public async Task GetUserByRefreshToken_WhenValidRefreshToken_ShouldReturnUserDTO()
        {
            //arrange
            var user = new UserDTO { Username = "testuser", RefreshToken = "validToken" };
            _userRepositoryMock.Setup(x => x.GetUserByRefreshToken("validToken"))
                .ReturnsAsync(user);

            //act
            var result = await _userService.GetUserByRefreshToken("validToken");

            Assert.Equal(user, result);
        }

        [Fact]
        public async Task ChangePassord_WhenUserNotInClaims_ShouldThrowInvalidCredentialException()
        {
            //arrange
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(x => x.User).Returns(claimsPrincipal);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);
            var changePasswordRequest = new ChangePasswordRequest
            {
                OldPassword = "oldpassword",
                NewPassword = "newpassword"
            };
            //act and assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _userService.ChangePassword(changePasswordRequest));
            Assert.Equal("Usuário não encontrado.", message.Message);
        }

        [Fact]
        public async Task ChangePassord_WhenUserNotInDb_ShouldThrowInvalidCredentialException()
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
                NewPassword = "newpassword"
            };
            _userRepositoryMock.Setup(x => x.GetUserByCredentials("testuser"))
                .ReturnsAsync((LoginUserDTO)null);
            //act and assert
            var message = await Assert.ThrowsAsync<InvalidCredentialException>(() => _userService.ChangePassword(changePasswordRequest));
            Assert.Equal("Usuário não encontrado.", message.Message);
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
            _userRepositoryMock.Setup(x => x.GetUserByCredentials("testuser"))
                .ReturnsAsync(new LoginUserDTO { Username = "testuser", Password = BCrypt.Net.BCrypt.HashPassword("oldpassword") });

            //act and assert
            var message = await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.ChangePassword(changePasswordRequest));
            Assert.Equal("A nova senha não pode ser igual à senha atual.", message.Message);
        }

        [Fact]
        public async Task ChangePassword_WhenOldPasswordIncorrect_ShouldThrowInvalidOperationException()
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
                OldPassword = "wrongpassword",
                NewPassword = "newpassword"
            };
            _userRepositoryMock.Setup(x => x.GetUserByCredentials("testuser"))
                .ReturnsAsync(new LoginUserDTO { Username = "testuser", Password = BCrypt.Net.BCrypt.HashPassword("oldpassword") });
            //act and assert
            var message = await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.ChangePassword(changePasswordRequest));
            Assert.Equal("A senha atual não coincide com a informada.", message.Message);
        }

        [Fact]
        public async Task ChangePassword_WhenValidRequest_ShouldChangePassword()
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
                NewPassword = "newpassword"
            };
            _userRepositoryMock.Setup(x => x.GetUserByCredentials("testuser"))
                .ReturnsAsync(new LoginUserDTO { Username = "testuser", Password = BCrypt.Net.BCrypt.HashPassword("oldpassword") });
            //act
            await _userService.ChangePassword(changePasswordRequest);
            //assert
            _userRepositoryMock.Verify(repo => repo.ChangePassord(It.Is<LoginUserDTO>(u => u.Username == "testuser" && u.Password != "oldpassword")), Times.Once);
        }
    }
}
