namespace SimpleIdentityServer.Host
{
    using System;
    using System.Collections.Generic;

    public class BasicAuthenticateOptions
    {
        public BasicAuthenticateOptions()
        {
            ClaimsIncludedInUserCreation = new List<string>();
        }

        /// <summary>
        /// Gets a list of claims include when the resource owner is created.
        /// If the list is empty then all the claims are included.
        /// </summary>
        public List<string> ClaimsIncludedInUserCreation { get; }
        /// <summary>
        /// Base url of the SCIM server.
        /// </summary>
        public Uri ScimBaseUrl { get; set; }
        /// <summary>
        /// Credentials used to get an access token.
        /// </summary>
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationWellKnownConfiguration { get; set; }
    }
}
