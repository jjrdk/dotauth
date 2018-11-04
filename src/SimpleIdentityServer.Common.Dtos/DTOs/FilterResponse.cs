namespace SimpleIdentityServer.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class FilterResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.FilterResponseNames.Supported)]
        public bool Supported { get; set; }

        /// <summary>
        /// Maximum number of resources returned in the response.
        /// </summary>
        [DataMember(Name = ScimConstants.FilterResponseNames.MaxResults)]
        public int MaxResults { get; set; }
    }
}