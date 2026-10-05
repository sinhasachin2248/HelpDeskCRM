using HelpDeskCRM.API.Data;
using HelpDeskCRM.API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add OpenAPI
builder.Services.AddOpenApi();

// Register HttpClientFactory
builder.Services.AddHttpClient();

// Connect Entity Framework Core to MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )!
    )
);

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCRM", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// OpenAPI only in Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowCRM");

// API logging middleware
app.UseMiddleware<ApiLoggingMiddleware>();

// Authorization
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();