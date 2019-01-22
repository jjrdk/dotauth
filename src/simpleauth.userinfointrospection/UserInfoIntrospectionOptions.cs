namespace SimpleAuth.UserInfoIntrospection
{
    using Microsoft.AspNetCore.Authentication;

    public class UserInfoIntrospectionOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "UserInfoIntrospection";

        public string WellKnownConfigurationUrl { get; set; }
    }
}
