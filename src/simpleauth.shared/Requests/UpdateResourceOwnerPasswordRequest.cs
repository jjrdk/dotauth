namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the request to update a resource owner password.
    /// </summary>
    [DataContract]
    public record UpdateResourceOwnerPasswordRequest
    {
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The login.
        /// </value>
        [DataMember(Name = "sub")]
        public string? Subject { get; init; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [DataMember(Name = "password")]
        public string? Password { get; init; }
    }
}