namespace SimpleAuth.Shared.DTOs
{
    using System;
    using System.Runtime.Serialization;

    public class ResourceMetadata
    {
        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "location")]
        public string Location { get;set; }
    }
}