using System.Security.Claims;
using Carter;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace SSOServer.API.Endpoints.Account;

public class OpenIdConnectEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/connect/authorize", async (HttpContext context) =>
        {
            var request = context.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            var result = await context.AuthenticateAsync("Identity.Application");

            return TypedResults.Challenge(
                authenticationSchemes: new List<string> { "Identity.Application" },
                properties: new AuthenticationProperties
                {
                    RedirectUri = context.Request.PathBase + context.Request.Path + QueryString.Create(
                        context.Request.HasFormContentType
                            ? context.Request.Form.ToList()
                            : context.Request.Query.ToList())
                });
        });


        app.MapGet("/connect/authorize", async (HttpContext context) =>
        {
            var request = context.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            var result = await context.AuthenticateAsync("Identity.Application");

            if (!result.Succeeded)
            {
                return TypedResults.Challenge(
                    authenticationSchemes: new List<string> { "Identity.Application" },
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = context.Request.PathBase + context.Request.Path + QueryString.Create(
                            context.Request.HasFormContentType
                                ? context.Request.Form.ToList()
                                : context.Request.Query.ToList())
                    });
            }

            // Создаем нового пользователя с заявками
            var claims = new List<Claim>
            {
                new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Identity.Name),
                new Claim("some claim", "some value").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim(OpenIddictConstants.Claims.Email,
                        result.Principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty)
                    .SetDestinations(OpenIddictConstants.Destinations.IdentityToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Устанавливаем запрашиваемые области
            claimsPrincipal.SetScopes(request.GetScopes());

            // Используем Results.SignIn для возвращения результата
            return Results.SignIn(claimsPrincipal,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        });

        app.MapPost("/connect/token", async (HttpContext context) =>
        {
            var request = context.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            ClaimsPrincipal claimsPrincipal;

            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Subject (sub) is a required field, we use the client id as the subject identifier here.
                identity.AddClaim(OpenIddictConstants.Claims.Subject,
                    request.ClientId ?? throw new InvalidOperationException());

                // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
                identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(request.GetScopes());
            }

            else if (request.IsAuthorizationCodeGrantType())
            {
                // Retrieve the claims principal stored in the authorization code
                claimsPrincipal =
                    (await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                    .Principal;
            }

            else if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                claimsPrincipal =
                    (await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                    .Principal;
            }

            else
            {
                throw new InvalidOperationException("The specified grant type is not supported.");
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return Results.SignIn(claimsPrincipal,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        });
    }
}