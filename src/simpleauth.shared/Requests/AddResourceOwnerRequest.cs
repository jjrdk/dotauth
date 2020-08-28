namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the add resource owner request.
    /// </summary>
    [DataContract]
    public class AddResourceOwnerRequest
    {
        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        [DataMember(Name = "sub")]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [DataMember(Name = "password")]
        public string? Password { get; set; }
    }
}
