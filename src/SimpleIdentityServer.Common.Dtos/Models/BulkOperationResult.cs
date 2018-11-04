namespace SimpleIdentityServer.Shared.Models
{
    using System.Net.Http;

    public class BulkOperationResult
    {
        /// <summary>
        /// HTTP method of the current operation.
        /// </summary>
        public HttpMethod Method { get; set; }
        /// <summary>
        /// Transient identifier of a newly created resource.
        /// Unique and created by the client. 
        /// REQUIRED when method is "POST"
        /// </summary>
        public string BulkId { get; set; }
        /// <summary>
        /// Current resource version.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Name of the schema.
        /// </summary>
        public string SchemaId { get; set; }
        /// <summary>
        /// Resource type.
        /// </summary>
        public string ResourceType { get; set; }
        /// <summary>
        /// Resource identifier.
        /// </summary>
        public string ResourceId { get; set; }
        /// <summary>
        /// Resource data as it would appear for a single SCIM POST, PUT etc ...
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// Location pattern.
        /// </summary>
        public string LocationPattern { get; set; }
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        public string Path { get; set; }
    }
}