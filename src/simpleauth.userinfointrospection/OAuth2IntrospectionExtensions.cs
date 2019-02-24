namespace SimpleAuth.UserInfoIntrospection
{
    using System;
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// Defines the extensions.
    /// </summary>
    public static class OAuth2IntrospectionExtensions
    {
        /// <summary>
        /// Adds the user information introspection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder)
           // => builder.AddUserInfoIntrospection(UserInfoIntrospectionOptions.AuthenticationScheme, _ => { });
            => builder.AddUserInfoIntrospection(_ => { });

        /// <summary>
        /// Adds the user information introspection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configureOptions">The configure options.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder, Action<UserInfoIntrospectionOptions> configureOptions)
        //=> builder.AddUserInfoIntrospection(UserInfoIntrospectionOptions.AuthenticationScheme, configureOptions);
            => builder.AddScheme<UserInfoIntrospectionOptions, UserInfoIntrospectionHandler>(UserInfoIntrospectionOptions.AuthenticationScheme, configureOptions);

        //public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder, string authenticationScheme, Action<UserInfoIntrospectionOptions> configureOptions)
        //    => builder.AddScheme<UserInfoIntrospectionOptions, UserInfoIntrospectionHandler>(authenticationScheme, configureOptions);
    }
}
