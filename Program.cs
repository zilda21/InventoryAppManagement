using InventoryApp.Data;
using InventoryApp.Hubs;
using InventoryApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql; // for NpgsqlConnectionStringBuilder

var builder = WebApplication.CreateBuilder(args);

// 1. Choose the connection string (Render vs local)
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    // On Render: convert DATABASE_URL -> Npgsql connection string
    connectionString = BuildConnectionStringFromDatabaseUrl(databaseUrl);
}
else
{
    // Local dev: use DefaultConnection from appsettings
    if (string.IsNullOrWhiteSpace(defaultConnection))
        throw new InvalidOperationException("No database connection string configured.");

    connectionString = defaultConnection;
}

// 2. DB + Identity + services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
});

builder.Services.AddTransient<IEmailSender, LocalEmailSender>();

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// 3. Apply EF migrations at startup (creates tables on Render)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// 4. Normal middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ProductHub>("/hubs/products");

// Root path → login page
app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/Identity/Account/Login");
    return Task.CompletedTask;
});

app.Run();

// Helper: convert Render's DATABASE_URL into an Npgsql connection string
static string BuildConnectionStringFromDatabaseUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);

    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port == -1 ? 5432 : uri.Port,  // default port
        Username = username,
        Password = password,
        Database = uri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}
