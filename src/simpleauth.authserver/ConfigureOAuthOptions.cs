namespace SimpleAuth.AuthServer
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.Extensions.Options;

    internal class ConfigureOAuthOptions : IPostConfigureOptions<OAuthOptions>
    {
        /// <inheritdoc />
        public void PostConfigure(string name, OAuthOptions options)
        {
            options.AuthorizationEndpoint = "http://localhost/authorization";
            options.TokenEndpoint = "http://localhost/token";
            options.UserInformationEndpoint = "http://localhost/userinfo";
            options.UsePkce = true;
            options.CallbackPath = "/callback";
            options.Events = new OAuthEvents
            {
                OnCreatingTicket = ctx =>
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(ctx.AccessToken);
                    ctx.Identity!.AddClaims(jwt.Claims.Where(c => !ctx.Identity.HasClaim(x => x.Type == c.Type)));
                    ctx.Success();
                    return Task.CompletedTask;
                },
                OnTicketReceived = ctx => Task.CompletedTask
            };
            options.SaveTokens = true;

            options.ClientId = "web";
            options.ClientSecret = "secret";
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("manager");
            options.Scope.Add("uma_protection");
        }
    }
}