namespace SimpleAuth.Shared.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the request to update resource owner claims.
    /// </summary>
    [DataContract]
    public class UpdateResourceOwnerClaimsRequest
    {
        /// <summary>
        /// Gets or sets the login.
        /// </summary>
        /// <value>
        /// The login.
        /// </value>
        [DataMember(Name = "login")]
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        [DataMember(Name = "claims")]
        public List<KeyValuePair<string, string>> Claims { get; set; }
    }
}