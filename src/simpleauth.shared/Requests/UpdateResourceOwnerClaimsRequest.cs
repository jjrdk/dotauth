namespace SimpleAuth.Shared.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using SimpleAuth.Shared.DTOs;

    /// <summary>
    /// Defines the request to update resource owner claims.
    /// </summary>
    [DataContract]
    public class UpdateResourceOwnerClaimsRequest
    {
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The login.
        /// </value>
        [DataMember(Name = "sub")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [DataMember(Name = "claims")]
        public PostClaim[] Claims { get; set; }
    }
}