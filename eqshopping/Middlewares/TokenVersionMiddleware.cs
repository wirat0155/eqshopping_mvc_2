using eqshopping.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace eqshopping.Middlewares
{
    public class TokenVersionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TokenVersionMiddleware(RequestDelegate next, IConfiguration config, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _config = config;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Cookies["eqshopping_jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var versionClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "version")?.Value;

                var currentVersion = _config["Application:Version"];
                if (versionClaim != currentVersion)
                {
                    // Create a new scope to resolve the scoped service
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

                        // Token version is outdated, refresh the token using the service
                        var newToken = jwtTokenService.RefreshToken(token);

                        // Set the new token in the cookies
                        context.Response.Cookies.Append("eqshopping_jwt", newToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddHours(1)
                        });
                    }
                }
            }

            await _next(context);
        }
    }
}
