namespace SimpleAuth.Shared.DTOs
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class GroupResource
    {
        public GroupResource()
        {
            Schemas = new[] {ScimConstants.SchemaUrns.Group};
            Metadata = new ResourceMetadata();
            Members = Array.Empty<string>();
        }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "schemas")]
        public string[] Schemas { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "members")]
        public string[] Members { get; set; }

        [DataMember(Name = "meta")]
        public ResourceMetadata Metadata { get; set; }
    }
}