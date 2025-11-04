using InventoryApp.Data;
using InventoryApp.Hubs;
using InventoryApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
});

// Make sure unauthorized users go to the right login page
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Local email sender
builder.Services.AddTransient<IEmailSender, LocalEmailSender>();

// Razor Pages + SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// When someone hits the root ("/"), send them to /Products.
// Because /Products has [Authorize], they'll be redirected to the login page first.
app.MapGet("/", () => Results.Redirect("/Products"));

app.MapRazorPages();
app.MapHub<ProductHub>("/hubs/products");

app.Run();
