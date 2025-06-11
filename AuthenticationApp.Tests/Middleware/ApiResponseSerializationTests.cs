using System.Text.Json;
using AuthenticationApp.Domain.Response;
using Xunit;

namespace AuthenticationApp.Tests.Middleware
{
    public class ApiResponseSerializationTests
    {
        [Fact]
        public void Serialize_SuccessResponse_ShouldContainSuccessTrue()
        {
            var response = ApiResponse<string>.CreateSuccess("data", "ok");
            var json = JsonSerializer.Serialize(response);
            Assert.Contains("\"Success\":true", json);
            Assert.Contains("\"Data\":\"data\"", json);
        }

        [Fact]
        public void Serialize_ErrorResponse_ShouldContainErrors()
        {
            var response = ApiResponse<object>.CreateFailure("fail", new[] { "err" });
            var json = JsonSerializer.Serialize(response);
            Assert.Contains("\"Success\":false", json);
            Assert.Contains("err", json);
        }
    }
}
