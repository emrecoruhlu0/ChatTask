using ChatTask.ChatService.Context;
using ChatTask.ChatService.Hubs;
using ChatTask.ChatService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Client for User Service - DÜZELTME: Base URL'i appsettings'ten al
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    var userServiceUrl = builder.Configuration.GetValue<string>("Services:UserService:BaseUrl") ?? "http://localhost:5001";
    client.BaseAddress = new Uri(userServiceUrl);
});

// Services
builder.Services.AddScoped<ChatMappingService>();

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Auth: JWT Bearer
var issuer = builder.Configuration["Jwt:Issuer"] ?? "chat-task";
var audience = builder.Configuration["Jwt:Audience"] ?? "chat-task-audience";
var key = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-change-me-please-1234567890";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
        
        // SignalR için token query'den al
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat-hub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chat-hub");

app.Run();  