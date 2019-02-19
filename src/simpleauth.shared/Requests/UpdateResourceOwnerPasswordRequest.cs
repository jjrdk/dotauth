namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the request to update a resource owner password.
    /// </summary>
    [DataContract]
    public class UpdateResourceOwnerPasswordRequest
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
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}