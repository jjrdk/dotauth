namespace SimpleIdentityServer.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class BulkOperationRequest
    {
        /// <summary>
        /// HTTP method of the current operation.
        /// </summary>
        [DataMember(Name = ScimConstants.BulkOperationRequestNames.Method)]
        public string Method { get; set; }
        /// <summary>
        /// Transient identifier of a newly created resource.
        /// Unique and created by the client. 
        /// REQUIRED when method is "POST"
        /// </summary>
        [DataMember(Name = ScimConstants.BulkOperationRequestNames.BulkId)]
        public string BulkId { get; set; }
        /// <summary>
        /// Current resource version.
        /// </summary>
        [DataMember(Name = ScimConstants.BulkOperationRequestNames.Version)]
        public string Version { get; set; }
        /// <summary>
        /// Resource's relative path to the SCIM service provider's root.
        /// POST : "/Users" or "/Groups"
        /// OTHERS : "/Users/<id>" or "/Groups/<id>"
        /// </summary>
        [DataMember(Name = ScimConstants.BulkOperationRequestNames.Path)]
        public string Path { get; set; }
        /// <summary>
        /// Resource data as it would appear for a single SCIM POST, PUT etc ...
        /// </summary>
        [DataMember(Name = ScimConstants.BulkOperationRequestNames.Data)]
        public object Data { get; set; }
    }
}