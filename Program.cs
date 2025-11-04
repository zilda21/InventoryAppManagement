using InventoryApp.Data;
using InventoryApp.Hubs;
using InventoryApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------- DB ----------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------- Identity ----------------
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Don’t force email confirmation for this project
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// email sender (local stub)
builder.Services.AddTransient<IEmailSender, LocalEmailSender>();

// ---------------- UI + SignalR ----------------
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// ---------- Apply migrations on startup ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();   // <== line that shows up in logs
}

// ---------- Middleware ----------
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

// Default root: redirect "/" to Products (which is [Authorize])
app.MapGet("/", context =>
{
    context.Response.Redirect("/Products");
    return Task.CompletedTask;
});

app.MapRazorPages();
app.MapHub<ProductHub>("/hubs/products");

app.Run();
