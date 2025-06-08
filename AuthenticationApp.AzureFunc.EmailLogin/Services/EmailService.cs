using AuthenticationApp.AzureFunc.EmailLogin.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationApp.AzureFunc.EmailLogin.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;

            var mailHost = _config["MailTrap:host"];
            var mailUser = _config["MailTrap:user"];
            var mailPassword = _config["MailTrap:password"];

            int? port = int.TryParse(
               _config["MailTrap:port"], 
                out int parsedPort) ? parsedPort : null;

            if (port is null)
            {
                throw new InvalidOperationException("MailTrap port is not configured correctly.");
            }

            _smtpClient = new SmtpClient(
                mailHost, 
                port.Value
            )
            {
                Credentials = new NetworkCredential(
                    mailUser,
                    mailPassword
                ),
                EnableSsl = true
            };
        }

        public void SendEmailAsync(string to, string subject, string body)
        {
            _smtpClient.Send(
                "donotrespond@authapp.com",
                to,
                subject,
                body);
        }
    }
}
