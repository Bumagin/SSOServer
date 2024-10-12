using IdentityServer.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Persistence.Data;

public class SSOServerDbContext : IdentityDbContext<User>
{
    public SSOServerDbContext(DbContextOptions<SSOServerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}