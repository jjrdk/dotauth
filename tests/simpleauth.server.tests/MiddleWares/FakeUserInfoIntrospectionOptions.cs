namespace SimpleAuth.Server.Tests.MiddleWares
{
    using Microsoft.AspNetCore.Authentication;

    public class FakeUserInfoIntrospectionOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "UserInfoIntrospection";
    }
}
