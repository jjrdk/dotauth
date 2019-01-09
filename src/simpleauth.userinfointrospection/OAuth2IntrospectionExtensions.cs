namespace SimpleAuth.UserInfoIntrospection
{
    using System;
    using Microsoft.AspNetCore.Authentication;

    public static class OAuth2IntrospectionExtensions
    {
        public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder)
           // => builder.AddUserInfoIntrospection(UserInfoIntrospectionOptions.AuthenticationScheme, _ => { });
            => builder.AddUserInfoIntrospection(_ => { });

        public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder, Action<UserInfoIntrospectionOptions> configureOptions)
        //=> builder.AddUserInfoIntrospection(UserInfoIntrospectionOptions.AuthenticationScheme, configureOptions);
            => builder.AddScheme<UserInfoIntrospectionOptions, UserInfoIntrospectionHandler>(UserInfoIntrospectionOptions.AuthenticationScheme, configureOptions);

        //public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder, string authenticationScheme, Action<UserInfoIntrospectionOptions> configureOptions)
        //    => builder.AddScheme<UserInfoIntrospectionOptions, UserInfoIntrospectionHandler>(authenticationScheme, configureOptions);
    }
}
