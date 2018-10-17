namespace SimpleIdentityServer.Core.Common.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class EtagResponse
    {
        /// <summary>
        /// A boolean value specifying whether or not the operation is supported.
        /// </summary>
        [DataMember(Name = ScimConstants.EtagResponseNames.Supported)]
        public bool Supported { get; set; }
    }
}