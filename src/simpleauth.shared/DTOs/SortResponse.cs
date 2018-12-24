namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class SortResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.SortResponseNames.Supported)]
        public bool Supported { get; set; }
    }
}