using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO;
using NewsletterApp.Application.Interfaces;
using NewsletterApp.Application.Services;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;
using NewsletterApp.Infrastructure.Data;
using NewsletterApp.Infrastructure.Repositories;
using NewsletterApp.Infrastructure.Services;
using System.Text;
using NewsletterApp.API.Middleware;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.production");
if (!File.Exists(envPath))
{
    envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env.production"));
}
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/");
    options.Conventions.AllowAnonymousToAreaPage("Admin", "/Account/Login");
});

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/admin.min.css", "css/admin.css");
    pipeline.AddJavaScriptBundle("/js/admin.min.js", "js/admin.js");
});



var databaseType = builder.Configuration.GetValue<string>("DatabaseType") ?? "SqlServer";
var migrationsAssembly = "NewsletterApp.API";
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<NewsletterDbContext>(options =>
{
    switch (databaseType?.Trim().ToLowerInvariant())
    {
        case "sqlite":
            options.UseSqlite(defaultConnection, b => b.MigrationsAssembly(migrationsAssembly));
            break;
        case "sqlserver":
        default:
            options.UseSqlServer(defaultConnection, b => b.MigrationsAssembly(migrationsAssembly));
            break;
    }
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<NewsletterDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.Cookie.Name = "NewsletterAdminAuth";
});



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
              {
                  var uri = new Uri(origin);
                  return uri.Host == "localhost" || uri.Host == "127.0.0.1";
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



builder.Services.AddScoped<ISubscriberRepository, SubscriberRepository>();
builder.Services.AddScoped<ILookupRepository, LookupRepository>();
builder.Services.AddScoped<INewsletterRepository, NewsletterRepository>();

builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<ISubscriberService, SubscriberService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<IDeletedItemsService, DeletedItemsService>();
builder.Services.AddScoped<IUnsubscribeAnalyticsService, UnsubscribeAnalyticsService>();
builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Newsletter API", Version = "v1" });
});

builder.Services.AddHealthChecks().AddDbContextCheck<NewsletterDbContext>();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Newsletter API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication(); 
app.UseAuthorization();



app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHealthChecks("/health");



using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}


app.Run();
