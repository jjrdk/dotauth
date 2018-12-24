namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BulkResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.BulkResponseNames.Supported)]
        public bool Supported { get; set; }

        /// <summary>
        /// Maximum number of operations.
        /// </summary>
        [DataMember(Name = ScimConstants.BulkResponseNames.MaxOperations)]
        public int MaxOperations { get; set; }

        /// <summary>
        /// Maximum payload size in bytes.
        /// </summary>
        [DataMember(Name = ScimConstants.BulkResponseNames.MaxPayloadSize)]
        public int MaxPayloadSize { get; set; }
    }
}