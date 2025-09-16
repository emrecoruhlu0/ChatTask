using ChatTask.ChatService.Context;
using ChatTask.ChatService.Models;
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
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, LogLevel.Information));

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

// Dump EF model diagnostics for Conversation to trace shadow FKs
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    try
    {
        var entityType = ctx.Model.FindEntityType(typeof(Conversation));
        if (entityType != null)
        {
            Console.WriteLine("[EFModel] Conversation properties:");
            foreach (var p in entityType.GetProperties())
            {
                Console.WriteLine($" - {p.Name} (column: {p.GetColumnName()})");
            }
            Console.WriteLine("[EFModel] Conversation FKs:");
            foreach (var fk in entityType.GetForeignKeys())
            {
                var depProps = string.Join(",", fk.Properties.Select(x => x.Name));
                Console.WriteLine($" - FK to {fk.PrincipalEntityType.DisplayName()} on [{depProps}] delete: {fk.DeleteBehavior}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[EFModel] Dump error: {ex.Message}");
    }
    
    // Database'i oluştur (migration yerine)
    try
    {
        ctx.Database.EnsureCreated();
        Console.WriteLine("[Database] Tables created successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Database] EnsureCreated error: {ex.Message}");
    }
}


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