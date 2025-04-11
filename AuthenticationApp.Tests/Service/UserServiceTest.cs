using AuthenticationApp.Business.Services;
using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Models;
using AuthenticationApp.Interfaces.DataAccess;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApp.Tests.Service
{
    public class UserServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTest() 
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
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
    }
}
