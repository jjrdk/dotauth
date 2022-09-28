namespace SimpleAuth.Server.Tests.MiddleWares;

using Microsoft.AspNetCore.Authentication;

public sealed class FakeUserInfoIntrospectionOptions : AuthenticationSchemeOptions
{
    public const string AuthenticationScheme = "UserInfoIntrospection";
}