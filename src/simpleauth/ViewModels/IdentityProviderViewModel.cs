namespace SimpleAuth.ViewModels
{
    /// <summary>
    /// Defines the identity provider view model.
    /// </summary>
    public class IdentityProviderViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityProviderViewModel"/> class.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        public IdentityProviderViewModel(string displayName)
        {
            DisplayName = displayName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityProviderViewModel"/> class.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="externalSubject">The external subject.</param>
        public IdentityProviderViewModel(string displayName, string externalSubject) : this(displayName)
        {
            ExternalSubject = externalSubject;
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the external subject.
        /// </summary>
        /// <value>
        /// The external subject.
        /// </value>
        public string ExternalSubject { get; set; }
    }
}