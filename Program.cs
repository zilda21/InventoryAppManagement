using InventoryApp.Data;
using InventoryApp.Hubs;
using InventoryApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Config;



// var builder for creats the host nad read the config //appsetting jsn that have the connection string 

var builder = WebApplication.CreateBuilder(args);
 
// This will read:
// - appsettings.json -> ConnectionStrings:DefaultConnection
// - overridden by env var ConnectionStrings__DefaultConnection (Render)

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//  EF  npgsql   Postgresql
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(connectionString));

// so basicaly it addds to teh asp.net core the idnetitiy of user  and their role by default and it stores in teh applicationdb context 

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.Configure<SalesforceOptions>(
    builder.Configuration.GetSection("Salesforce"));

builder.Services.AddHttpClient<ISalesforceService, SalesforceService>();


// in this project its not required for email confirmation to sing in/to register 
// builder.Services.Configure<IdentityOptions>(o =>
// {
//     o.SignIn.RequireConfirmedAccount = false;
// });

// when the identitty need to send emails /register reset  it uses my local email sneder implementaiton exmample logging to console or file instiad of real SMTP 
// builder.Services.AddTransient<IEmailSender, LocalEmailSender>();

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// AIT CREATE A di SCOPE, gets the applicaitondbcontext, run migrate()

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
// in the productioni t shpows frindly error (/ERROR) and it redirect to HSTS
// to avoid the fact that the app is not running ion http , it redirect from http to https 
// it serve static file like (css, js,etc)
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

// maps all the razor pages pages/.cshtml
//  maps you signalR hub to hubs/products
app.MapRazorPages();
app.MapHub<ProductHub>("/hubs/products");


app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/Identity/Account/Login");
    return Task.CompletedTask;
});

app.Run();
