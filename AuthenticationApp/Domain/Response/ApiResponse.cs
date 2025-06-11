using System.Collections.Generic;

namespace AuthenticationApp.Domain.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static ApiResponse<T> CreateSuccess(T data, string? message = null)
            => new ApiResponse<T> { Success = true, Data = data, Message = message };

        public static ApiResponse<T> CreateFailure(string message, IEnumerable<string>? errors = null)
            => new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }
}
