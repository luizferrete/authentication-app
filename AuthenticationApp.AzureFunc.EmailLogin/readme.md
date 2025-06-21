# AuthenticationApp.AzureFunc.EmailLogin

This project is an Azure Functions application that listens to RabbitMQ messages and sends email notifications when a user logs in to the AuthenticationApp platform.

## Overview

- **Trigger:** Uses the `RabbitMQTrigger` to process messages from a RabbitMQ queue (`email_queue`).
- **Functionality:** When a login event is published to the queue, the function deserializes the message and sends an email notification to the user.
- **Email Service:** Email sending is handled via SMTP, with configuration (host, port, user, password) loaded from [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables.

## How it Works

1. **Message Consumption:**  
   The function is triggered by new messages in the `email_queue` RabbitMQ queue.
2. **Deserialization:**  
   The message is expected to be a JSON object containing user login information (email, username, IP).
3. **Email Notification:**  
   An email is sent to the user, notifying them of a new login event.

## Configuration

- **RabbitMQ:**  
  Set the `rabbitConnection` connection string in [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or as an environment variable.

- **MailTrap SMTP (for development/testing):**  
  Store your SMTP credentials in [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables:

  ```json
  {
	"MailTrap:host": "smtp.mailtrap.io",
	"MailTrap:port": "2525",
	"MailTrap:user": "your_user",
	"MailTrap:password": "your_password"
  }
  ```

  
## Main Libraries Used

- **Microsoft.Azure.Functions.Worker**: Azure Functions .NET isolated worker SDK.
- **Microsoft.Azure.Functions.Worker.Extensions.RabbitMQ**: RabbitMQ trigger binding for Azure Functions.
- **Microsoft.Extensions.Configuration.UserSecrets**: Securely manage development secrets.
- **Microsoft.ApplicationInsights.WorkerService**: Application Insights telemetry for monitoring.

## Example

A message published to the `email_queue` should look like:

```json
{ 
	"Email": "user@example.com", 
	"Username": "user123", 
	"Ip": "192.168.1.1"
}
```
The function will send an email to `user@example.com` about the login event.

## Running Locally

1. Set up your `secrets.json` with the RabbitMQ connection string.
2. Add your MailTrap (or SMTP) credentials to User Secrets or environment variables.
3. Run the function app using Visual Studio or the .NET CLI.
4. To use Docker, build the image with `docker build -t authenticationapp-emailfunc .` and run it or start everything with the main project's `docker-compose.yml`.

## License

This project is licensed under the MIT License.
