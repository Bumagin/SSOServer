using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityServer.Persistence.Data;

public class DbContextFactory : IDesignTimeDbContextFactory<SSOServerDbContext>
{
    public SSOServerDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var optionsBuilder = new DbContextOptionsBuilder<SSOServerDbContext>()
            .UseNpgsql("User ID=postgres;Password=qwerty1234;Host=localhost;Port=5432;Database=SSOServer;Pooling=true;");

        return new SSOServerDbContext(optionsBuilder.Options);
    }
}