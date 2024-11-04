using Carter;
using IdentityServer.Domain.Users;
using IdentityServer.Persistence.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSOServer.UI.Components;
using SSOServer.UI.Components.Account;

namespace SSOServer.UI.DependencyInjection;

public static class ConfigureServices
{
    public static void AddDefaultServices(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        
        services.AddCarter();
    }

    public static void UseDefaultServices(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapCarter();
    }

    public static async Task MigrateNpgsqlAsync(this WebApplication app)
    {
        var scope1= app.Services.CreateScope();
        await scope1.ServiceProvider.GetRequiredService<SSOServerDbContext>().Database.MigrateAsync();
    }
}