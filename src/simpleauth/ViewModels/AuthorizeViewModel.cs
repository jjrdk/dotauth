namespace SimpleAuth.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines the authorize view model.
    /// </summary>
    /// <seealso cref="SimpleAuth.ViewModels.IdProviderAuthorizeViewModel" />
    public class AuthorizeViewModel : IdProviderAuthorizeViewModel
    {
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        [Required(ErrorMessage = "the user name is required")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [Required(ErrorMessage = "the password is required")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is checked; otherwise, <c>false</c>.
        /// </value>
        public bool IsChecked { get; set; }
    }
}