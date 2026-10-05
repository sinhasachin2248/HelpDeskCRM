using HelpDeskCRM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// SERVICES
// =====================================================

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    )
);
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";

        options.LogoutPath = "/Account/Logout";

        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;
    });


// =====================================================
// AUTHORIZATION
// =====================================================

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new Microsoft.AspNetCore.Authorization
            .AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});


var app = builder.Build();


// =====================================================
// HTTP REQUEST PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();

app.UseAuthorization();


// =====================================================
// API ROUTING
// =====================================================

// Required for attribute-routed API controllers
// Example:
// GET /api/tickets/pending

app.MapControllers();


// =====================================================
// MVC ROUTING
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();