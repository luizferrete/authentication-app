using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApp.AzureFunc.EmailLogin.DTOs
{
    public record LoginRequest
    {
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Ip { get; set; }
    }
}
