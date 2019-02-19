namespace SimpleAuth.WebSite.Authenticate
{
    using System.Security.Claims;
    using SimpleAuth.Results;

    /// <summary>
    /// Defines the local OpenId authentication result.
    /// </summary>
    internal class LocalOpenIdAuthenticationResult
    {
        /// <summary>
        /// Gets or sets the endpoint result.
        /// </summary>
        /// <value>
        /// The endpoint result.
        /// </value>
        public EndpointResult EndpointResult { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public Claim[] Claims { get; set; }

        /// <summary>
        /// Gets or sets the two factor.
        /// </summary>
        /// <value>
        /// The two factor.
        /// </value>
        public string TwoFactor { get; set; }
    }
}