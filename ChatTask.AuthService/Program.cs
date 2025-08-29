var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("UserService", c =>
{
    // UserService base URL
    c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("UserService:BaseUrl") ?? "http://localhost:5001/");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
