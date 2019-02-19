namespace SimpleAuth.Shared.Responses
{
    using System.Runtime.Serialization;
    using Models;

    /// <summary>
    /// Defines the resource client secret.
    /// </summary>
    [DataContract]
    public class ResponseClientSecret
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [DataMember(Name = "type")]
        public ClientSecretTypes Type { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}