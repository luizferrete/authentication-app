using AuthenticationApp.Business.Services;
using AuthenticationApp.DataAccess.Repositories;
using AuthenticationApp.Domain.Settings;
using AuthenticationApp.Endpoints;
using AuthenticationApp.Interfaces.Business;
using AuthenticationApp.Interfaces.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;
using Scalar.AspNetCore;
using AuthenticationApp.DataAccess.UnitOfWork;
using AuthenticationApp.DataAccess.Context;
using AuthenticationApp.Domain.Validators.Request;
using FluentValidation;
using StackExchange.Redis;
using AuthenticationApp.Infra.Interfaces;
using AuthenticationApp.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ChangePasswordRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RefreshTokenRequestValidator>();

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection(nameof(MongoDB)));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection(nameof(RedisSettings)));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return client.GetDatabase(settings.Database);
});

builder.Services.AddSingleton<IMongoDbContext>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
        return new MongoDbContext(
            settings.ConnectionString,
            settings.Database
        );
    }
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisSettings = builder.Configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>();
    options.Configuration = redisSettings.ConnectionString;
    options.InstanceName = redisSettings.InstanceName;
}).AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisSettings = builder.Configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>();
    return ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
});

builder.Services.AddSingleton<IQueuePublisher>(sp =>
{
    var rabbitSettings = builder.Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
    return new QueuePublisher(rabbitSettings!.Host, rabbitSettings.Port, rabbitSettings.UserName, rabbitSettings.Password);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
        },
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWTToken:Issuer"],
        ValidAudience = builder.Configuration["JWTToken:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTToken:PrivateKey"]))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/swagger/{documentName}.json";
    });
    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/swagger/{documentName}.json");
    });
}

app.MapScalarApiReference(options =>
{
    options.WithEndpointPrefix("/api-reference/{documentName}");
});

app.UseHttpsRedirection();
app.MapWeatherForecastEndpoints();
app.MapUserEndpoints();
app.MapLoginEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

