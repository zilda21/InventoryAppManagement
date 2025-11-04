using InventoryApp.Data;
using InventoryApp.Hubs;
using InventoryApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql; // for NpgsqlConnectionStringBuilder

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1. Choose connection string (local vs Render)
// ----------------------------------------------------
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");

// Render usually sets DATABASE_URL like:
// postgres://user:password@host:5432/dbname
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // We are on Render – convert DATABASE_URL into Npgsql connection string
    connectionString = BuildConnectionStringFromDatabaseUrl(databaseUrl);
}
else
{
    // Local dev – use appsettings.json / appsettings.Development.json
    connectionString = defaultConnection!;
}

// DB
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(connectionString));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
});

builder.Services.AddTransient<IEmailSender, LocalEmailSender>();

// Razor Pages + SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// ----------------------------------------------------
// 2. Apply EF migrations at startup (creates tables on Render)
// ----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ----------------------------------------------------
// 3. Normal middleware pipeline
// ----------------------------------------------------
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

// Root path → redirect to login
app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/Identity/Account/Login");
    return Task.CompletedTask;
});

app.Run();


// ----------------------------------------------------
// Helper: convert Render's DATABASE_URL into Npgsql string
// ----------------------------------------------------
static string BuildConnectionStringFromDatabaseUrl(string databaseUrl)
{
    var uri = new Uri(databaseUrl);

    // user:password
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port == -1 ? 5432 : uri.Port, // fallback if port missing
        Username = username,
        Password = password,
        Database = uri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require        // <- removed TrustServerCertificate
    };

    return builder.ConnectionString;
}
