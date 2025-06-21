using AuthenticationApp.AzureFunc.EmailLogin.DTOs;
using AuthenticationApp.AzureFunc.EmailLogin.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace AuthenticationApp.AzureFunc.EmailLogin;

public class EmailFunctions
{
    private readonly ILogger _logger;
    private readonly IEmailService _emailService;

    public EmailFunctions(ILoggerFactory loggerFactory, IEmailService emailService)
    {
        _logger = loggerFactory.CreateLogger<EmailFunctions>();
        _emailService = emailService;
    }

    [Function("EmailOnLogin")]
    public void Run([RabbitMQTrigger("email_queue", ConnectionStringSetting = "rabbitConnection")] string myQueueItem)
    {
        _logger.LogInformation("C# Queue trigger function processed: {item}", myQueueItem);
        Console.WriteLine("Queue trigger function processed: " + myQueueItem);

        var loginObj = JsonSerializer.Deserialize<LoginRequest>(myQueueItem);

        if(loginObj is null)         {
            _logger.LogError("Deserialization failed for item: {item}", myQueueItem);
            return;
        }

        _emailService.SendEmailAsync(
            loginObj.Email,
            "New login on AuthApp",
            $"Hello, {loginObj.Username}. A new login on your account was detected from IP address {loginObj.Ip}. \n" +
            $"If this was not you, please change your password immediately.");

        _logger.LogInformation("E-mail sent to {email}", loginObj?.Email);
    }
}