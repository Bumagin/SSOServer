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

            claimsPrincipal.SetScopes(request.GetScopes());

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
                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                identity.AddClaim(OpenIddictConstants.Claims.Subject,
                    request.ClientId ?? throw new InvalidOperationException());

                identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(request.GetScopes());
            }

            else if (request.IsAuthorizationCodeGrantType())
            {
                claimsPrincipal =
                    (await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                    .Principal;
            }

            else if (request.IsRefreshTokenGrantType())
            {
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