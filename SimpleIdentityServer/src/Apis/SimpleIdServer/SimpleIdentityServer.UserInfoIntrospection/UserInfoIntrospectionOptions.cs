using Microsoft.AspNetCore.Authentication;

namespace SimpleIdentityServer.UserInfoIntrospection
{
    public class UserInfoIntrospectionOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "UserInfoIntrospection";

        public string WellKnownConfigurationUrl { get; set; }
    }
}
