using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Areas.Identity.Data;

public class InventoryAppIdentityDbContext : IdentityDbContext<IdentityUser>
{
    public InventoryAppIdentityDbContext(DbContextOptions<InventoryAppIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
    }
}
