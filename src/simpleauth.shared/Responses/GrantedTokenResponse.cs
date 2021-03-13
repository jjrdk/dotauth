namespace SimpleAuth.Shared.Responses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the granted token response.
    /// </summary>
    [DataContract]
    public record GrantedTokenResponse
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        [DataMember(Name = "access_token")]
        public string AccessToken { get; init; } = null!;

        /// <summary>
        /// Gets or sets the identifier token.
        /// </summary>
        /// <value>
        /// The identifier token.
        /// </value>
        [DataMember(Name = "id_token")]
        public string? IdToken { get; init; }

        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        /// <value>
        /// The type of the token.
        /// </value>
        [DataMember(Name = "token_type")]
        public string TokenType { get; init; } = null!;

        /// <summary>
        /// Gets or sets the expires in.
        /// </summary>
        /// <value>
        /// The expires in.
        /// </value>
        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; init; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        [DataMember(Name = "refresh_token")]
        public string? RefreshToken { get; init; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        [DataMember(Name = "scope")]
        public string Scope { get; init; } = null!;
    }
}
