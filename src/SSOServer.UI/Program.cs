using AuthorizationServer;
using IdentityServer.Persistence.DependencyInjection;
using SSOServer.UI.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddIdentity(builder.Configuration);

builder.Services.AddDefaultServices();

builder.Services.AddOpenidConfiguration();

builder.Services.AddHostedService<TestData>();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

app.UseDefaultServices();

await app.MigrateNpgsqlAsync();

app.Run();