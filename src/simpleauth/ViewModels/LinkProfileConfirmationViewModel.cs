namespace SimpleAuth.ViewModels
{
    /// <summary>
    /// Defines the link profile confirmation view model.
    /// </summary>
    public class LinkProfileConfirmationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkProfileConfirmationViewModel"/> class.
        /// </summary>
        /// <param name="issuer">The issuer.</param>
        public LinkProfileConfirmationViewModel(string issuer)
        {
            Issuer = issuer;
        }

        /// <summary>
        /// Gets the issuer.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; }
    }
}
