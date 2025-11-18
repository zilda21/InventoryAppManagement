using InventoryApp.Data;
using InventoryApp.Hubs;
using InventoryApp.Services;
using InventoryApp.config;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;  // for AddDatabaseDeveloperPageExceptionFilter & UseMigrationsEndPoint

var builder = WebApplication.CreateBuilder(args);

// ----------------- DB + Identity -----------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// nice EF Core error pages in development
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ----------------- MVC / SignalR -----------------
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// ----------------- Salesforce -----------------
builder.Services.Configure<SalesforceOptions>(
    builder.Configuration.GetSection(SalesforceOptions.SectionName));

builder.Services.AddHttpClient<SalesforceService>();
builder.Services.AddScoped<ISalesforceService, SalesforceService>();

// ----------------- Build app -----------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint(); // migrations endpoint for dev only
}
else
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
app.MapHub<ProductHub>("/productHub");

app.Run();
