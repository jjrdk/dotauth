namespace SimpleAuth.Shared.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the token response.
    /// </summary>
    [DataContract]
    public class TokenResponse
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the identifier token.
        /// </summary>
        /// <value>
        /// The identifier token.
        /// </value>
        [DataMember(Name = "id_token")]
        public string IdToken { get; set; }

        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        /// <value>
        /// The type of the token.
        /// </value>
        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expires in.
        /// </summary>
        /// <value>
        /// The expires in.
        /// </value>
        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        [DataMember(Name = "scope")]
        public List<string> Scope { get; set; }
    }
}
