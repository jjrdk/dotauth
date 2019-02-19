namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the posted claim.
    /// </summary>
    [DataContract]
    public class PostClaim
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [DataMember(Name = "type")]
        public string Type { get; set; }

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