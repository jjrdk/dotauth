namespace SimpleAuth.Shared.Requests
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the revoke session request.
    /// </summary>
    [DataContract]
    public class RevokeSessionRequest
    {
        /// <summary>
        /// Gets or sets the identifier token hint.
        /// </summary>
        /// <value>
        /// The identifier token hint.
        /// </value>
        [DataMember(Name = "id_token_hint")]
        public string IdTokenHint { get; set; }

        /// <summary>
        /// Gets or sets the post logout redirect URI.
        /// </summary>
        /// <value>
        /// The post logout redirect URI.
        /// </value>
        [DataMember(Name = "post_logout_redirect_uri")]
        public Uri PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [DataMember(Name = "state")]
        public string State { get; set; }
    }
}
