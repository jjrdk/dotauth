namespace SimpleAuth.Shared.Responses
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the error response with state.
    /// </summary>
    /// <seealso cref="ErrorResponse" />
    [DataContract]
    public class ErrorResponseWithState : ErrorResponse
    {
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [DataMember(Name = "state")]
        public string State { get; set; }
    }
}
