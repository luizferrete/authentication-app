# AuthenticationApp

AuthenticationApp is a .NET 8-based application designed to provide user authentication and authorization services. It leverages modern technologies such as MongoDB, JWT, and ASP.NET Core Minimal APIs to deliver a secure and scalable solution.

## Features
- User registration and authentication.
- JWT-based authentication and authorization.
- Refresh token support for session management.
- Secure password storage using BCrypt.
- OpenAPI/Scalar integration for API documentation.
- Modular and testable architecture.
- Change password functionality.
- Request validation with **FluentValidation**.
- Centralized API responses and global exception handling.
- Redis-based refresh token cache with mass logout capability.
- RabbitMQ integration for asynchronous events.
- Email notifications via Azure Functions.
- Dockerized environment with `docker-compose`.

## Libraries Used
The solution uses the following libraries:

### Main Application (`AuthenticationApp.csproj`)
- **[BCrypt.Net-Next](https://www.nuget.org/packages/BCrypt.Net-Next)** (v4.0.3): For secure password hashing.
- **[Microsoft.AspNetCore.Authentication.JwtBearer](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer)** (v8.0.14): For JWT-based authentication.
- **[Microsoft.AspNetCore.OpenApi](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi)** (v8.0.13): For OpenAPI/Swagger integration.
- **[MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver)** (v3.3.0): For MongoDB database access.
- **[Scalar.AspNetCore](https://www.nuget.org/packages/Scalar.AspNetCore)** (v2.1.13): For API reference generation.
- **[Swashbuckle.AspNetCore](https://www.nuget.org/packages/Swashbuckle.AspNetCore)** (v6.6.2): For Swagger UI and API documentation.
- **[FluentValidation.AspNetCore](https://www.nuget.org/packages/FluentValidation.AspNetCore)** (v11.3.0): For request validation.
- **[RabbitMQ.Client](https://www.nuget.org/packages/RabbitMQ.Client)** (v6.8.1): For message queue integration.
- **[StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis)** (v2.8.31) and **[Microsoft.Extensions.Caching.StackExchangeRedis](https://www.nuget.org/packages/Microsoft.Extensions.Caching.StackExchangeRedis)** (v10.0.0-preview.3.25172.1): For Redis caching.

### Test Project (`AuthenticationApp.Tests.csproj`)
- **[coverlet.collector](https://www.nuget.org/packages/coverlet.collector)** (v6.0.0): For code coverage collection.
- **[Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)** (v17.8.0): For running tests in Visual Studio.
- **[Moq](https://www.nuget.org/packages/Moq)** (v4.20.72): For mocking dependencies in unit tests.
- **[xUnit](https://www.nuget.org/packages/xunit)** (v2.5.3): For unit testing.
- **[xUnit.runner.visualstudio](https://www.nuget.org/packages/xunit.runner.visualstudio)** (v2.5.3): For running xUnit tests in Visual Studio.

## Main Points of the Solution

### Architecture
- **Repositories**: Handles data access logic (e.g., `UserRepository` for user-related operations).
- **Services**: Encapsulates business logic (e.g., `UserService` for user management).
- **Endpoints**: Defines API routes (e.g., `UserEndpoints` and `AuthEndpoints`).
- **Unit of Work**: Manages database transactions (`UnitOfWork`).
- **Caching**: `RedisCacheService` stores refresh tokens and other cached data.
- **Messaging**: `QueuePublisher` uses RabbitMQ to dispatch login events.

### Authentication and Authorization
- **JWT Authentication**: Configured using `Microsoft.AspNetCore.Authentication.JwtBearer` to validate tokens.
- **Refresh Tokens**: Implemented to allow users to renew their sessions securely.
- **Password Hashing**: Uses BCrypt for secure password storage.
- **Change Password**: Endpoint available to update user passwords.
- **Request Validation**: Input models validated with FluentValidation.

### Database
- **MongoDB**: The application uses MongoDB as its database, with `MongoDB.Driver` for data access. The `MongoDbContext` class manages the database connection.

### API Documentation
- **Scalar/OpenAPI**: Integrated using `Swashbuckle.AspNetCore` and `Microsoft.AspNetCore.OpenApi` for interactive API documentation.
- **ApiResponse Wrapper**: Standardizes responses and error handling.

### Testing
- Unit tests are written using xUnit and Moq to ensure the reliability of services and repositories.
- Code coverage is collected using Coverlet.


## Getting Started
1. Clone the repository.
2. Configure the `appsettings.json` file with your MongoDB connection string and JWT settings. Use `dotnet user-secrets` (the `secrets.json` file) to store the connection string and JWT private key locally.
3. Build and run the solution using Visual Studio 2022 or the .NET CLI.
4. Alternatively, create an `.env` file and execute `docker compose up` inside the `AuthenticationApp` folder to start the API, Redis, RabbitMQ and the Azure Function.
5. Access the Scalar UI at `https://localhost:<port>/scalar`.

## License
This project is licensed under the MIT License.
