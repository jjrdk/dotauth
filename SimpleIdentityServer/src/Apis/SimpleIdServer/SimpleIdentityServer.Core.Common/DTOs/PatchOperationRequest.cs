namespace SimpleIdentityServer.Core.Common.DTOs
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Linq;

    [DataContract]
    public class PatchOperationRequest
    {
        [DataMember(Name = ScimConstants.PatchOperationRequestNames.Operation)]
        public string Operation { get; set; }

        [DataMember(Name = ScimConstants.PatchOperationRequestNames.Path)]
        public string Path { get; set; }

        [DataMember(Name = ScimConstants.PatchOperationRequestNames.Value)]
        public JToken Value { get; set; }
    }
}