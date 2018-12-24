namespace SimpleIdentityServer.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class PatchResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.PatchResponseNames.Supported)]
        public bool Supported { get; set; }
    }
}