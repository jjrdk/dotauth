namespace SimpleAuth.Shared.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the add claim request.
    /// </summary>
    [DataContract]
    public class AddClaimRequest
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [DataMember(Name = "key")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is identifier.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is identifier; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "is_identifier")]
        public bool IsIdentifier { get; set; }
    }
}
