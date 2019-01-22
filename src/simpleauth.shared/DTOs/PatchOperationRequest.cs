namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PatchOperationRequest
    {
        [DataMember(Name = ScimConstants.PatchOperationRequestNames.Operation)]
        public string Operation { get; set; }

        [DataMember(Name = ScimConstants.PatchOperationRequestNames.Path)]
        public string Path { get; set; }

        [DataMember(Name = ScimConstants.PatchOperationRequestNames.Value)]
        public object Value { get; set; }
    }
}