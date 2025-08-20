using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace FunctionApp_Auth;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("Function1")]
    public static IActionResult Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
    ILogger log)
    {
        string authHeader = req.Headers["Authorization"];
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return new UnauthorizedResult();

        string token = authHeader.Substring("Bearer ".Length);
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://dev-5ad0jt24mzim62jm.us.auth0.com",
            ValidateAudience = false,
            ValidAudience = "https://my-azure-function-api",
            ValidateLifetime = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                // Fetch keys from Auth0 JWKS
                var client = new HttpClient();
                var keys = client.GetStringAsync("https://dev-5ad0jt24mzim62jm.us.auth0.com/.well-known/jwks.json").Result;
                var jwks = new JsonWebKeySet(keys);
                return jwks.Keys;
            }
        };

        try
        {
            handler.ValidateToken(token, validationParameters, out _);
            return new OkObjectResult("Valid access token!");
        }
        catch
        {
            return new OkObjectResult("in valid Valid access token!");
        }
    }
}