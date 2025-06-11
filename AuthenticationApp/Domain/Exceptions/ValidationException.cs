using System.Collections.Generic;

namespace AuthenticationApp.Domain.Exceptions
{
    public class ValidationException : Exception
    {
        public IEnumerable<string> ValidationErrors { get; }

        public ValidationException(IEnumerable<string> errors, string message = "Validation failed") : base(message)
        {
            ValidationErrors = errors;
        }
    }
}
