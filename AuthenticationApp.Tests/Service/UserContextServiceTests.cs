using AuthenticationApp.Business.Services;
using AuthenticationApp.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApp.Tests.Service
{
    public class UserContextServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IHttpContextAccessor> _null_httpContextAccessorMock;
        private readonly IUserContextService _userContextService;
        private readonly IUserContextService _null_userContextService;
        public UserContextServiceTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _null_httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Headers["Authorization"] = "Bearer testtoken";

            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("123.45.67.89");

            var identity = new ClaimsIdentity(
                new[]
                {
                new Claim(ClaimTypes.Name, "testuser")
                },
                "TestAuthType");

            httpContext.User = new ClaimsPrincipal(identity);

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns(httpContext);

            _userContextService = new UserContextService(_httpContextAccessorMock.Object);

            _null_httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext)null);

            _null_userContextService = new UserContextService(_null_httpContextAccessorMock.Object);
        }

        [Fact]
        public void UserIpAddress_ShouldReturnIpAddress_WhenValidHttpContext()
        {
            // Act
            var ip = _userContextService.UserIpAddress;

            // Assert
            Assert.Equal("123.45.67.89", ip);
        }

        [Fact]
        public void UserName_ShouldReturnIpAddress_WhenValidHttpContext()
        {
            // Act
            var name = _userContextService.UserName;

            // Assert
            Assert.Equal("testuser", name);
        }

        [Fact]
        public void UserIpAddress_ShouldReturnUnknown_WhenNullHttpContext()
        {
            // Act
            var ip = _null_userContextService.UserIpAddress;
            // Assert
            Assert.Equal("unknown", ip);
        }

        [Fact]
        public void UserName_ShouldReturnNull_WhenNullHttpContext()
        {
            // Act
            var name = _null_userContextService.UserName;
            // Assert
            Assert.Null(name);
        }
    }
}
