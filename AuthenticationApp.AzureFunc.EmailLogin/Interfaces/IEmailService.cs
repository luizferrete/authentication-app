using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApp.AzureFunc.EmailLogin.Interfaces
{
    public interface IEmailService
    {
       void SendEmailAsync(string to, string subject, string body);
    }
}
