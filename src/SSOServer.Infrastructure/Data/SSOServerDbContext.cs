using IdentityServer.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Persistence.Data;

public class SSOServerDbContext : IdentityDbContext<ApplicationUser>
{
    public SSOServerDbContext(DbContextOptions<SSOServerDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseOpenIddict();
        base.OnModelCreating(builder);
    }
}