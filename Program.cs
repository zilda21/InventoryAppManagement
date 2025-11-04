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
    // No email confirmation required for this project
    options.SignIn.RequireConfirmedAccount = false;
});

// Make sure unauthorized users go to the Identity login page
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Local "fake" email sender (we already implemented this)
builder.Services.AddTransient<IEmailSender, LocalEmailSender>();

// Razor Pages + SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

//
//  Apply EF Core migrations on startup (important for Render DB!)
//
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();   // creates Identity + Products tables if they don't exist
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Root ("/") -> /Products (and because of [Authorize] you'll be sent to Login first)
app.MapGet("/", () => Results.Redirect("/Products"));

app.MapRazorPages();
app.MapHub<ProductHub>("/hubs/products");

app.Run();
