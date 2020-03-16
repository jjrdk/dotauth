namespace SimpleAuth.UserInfoIntrospection
{
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// Defines the user info introspection options.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions" />
    public class UserInfoIntrospectionOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// The authentication scheme
        /// </summary>
        public const string AuthenticationScheme = "UserInfoIntrospection";

        /// <summary>
        /// Gets or sets the well known configuration URL.
        /// </summary>
        /// <value>
        /// The well known configuration URL.
        /// </value>
        public string WellKnownConfigurationUrl { get; set; }
    }
}
