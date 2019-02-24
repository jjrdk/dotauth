namespace SimpleAuth.ViewModels
{
    /// <summary>
    /// Defines the OpenID local authentication view model.
    /// </summary>
    /// <seealso cref="SimpleAuth.ViewModels.AuthorizeOpenIdViewModel" />
    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        /// <summary>
        /// Gets or sets the login.
        /// </summary>
        /// <value>
        /// The login.
        /// </value>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
    }
}
