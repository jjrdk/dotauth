namespace SimpleAuth.Shared.Responses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the introspection response.
    /// </summary>
    [DataContract]
    public class IntrospectionResponse
    {
        /// <summary>
        /// Gets or sets a boolean indicator of whether or not the presented token is currently active
        /// </summary>
        [DataMember(Name = "active")]
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the client id
        /// </summary>
        [DataMember(Name = "client_id")]
        public string ClientId { get; set; } = null!;

        /// <summary>
        /// Gets or sets identifier for the resource owner who authorized this token
        /// </summary>
        [DataMember(Name = "username")]
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the token type
        /// </summary>
        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }= null!;

        /// <summary>
        /// Gets or sets the expiration in seconds
        /// </summary>
        [DataMember(Name = "exp")]
        public int Expiration { get; set; }

        /// <summary>
        /// Gets or sets the issue date
        /// </summary>
        [DataMember(Name = "iat")]
        public double IssuedAt { get; set; }

        /// <summary>
        /// Gets or sets the NBF
        /// </summary>
        [DataMember(Name = "nbf")]
        public int Nbf { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        [DataMember(Name = "sub")]
        public string Subject { get; set; } = null!;

        /// <summary>
        /// Gets or sets the audience
        /// </summary>
        [DataMember(Name = "aud")]
        public string Audience { get; set; } = null!;

        /// <summary>
        /// Gets or sets the issuer of this token
        /// </summary>
        [DataMember(Name = "iss")]
        public string Issuer { get; set; } = null!;

        /// <summary>
        /// Gets or sets the string representing the issuer of the token
        /// </summary>
        [DataMember(Name = "jti")]
        public string? Jti { get; set; }
    }
}