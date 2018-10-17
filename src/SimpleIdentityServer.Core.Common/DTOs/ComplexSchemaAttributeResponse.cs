namespace SimpleIdentityServer.Core.Common.DTOs
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ComplexSchemaAttributeResponse : SchemaAttributeResponse
    {
        public ComplexSchemaAttributeResponse()
        {
            Type = ScimConstants.SchemaAttributeTypes.Complex;
        }

        /// <summary>
        /// Defines a set of sub-attributes
        /// </summary>
        [DataMember(Name = ScimConstants.ComplexSchemaAttributeResponseNames.SubAttributes)]
        public IEnumerable<SchemaAttributeResponse> SubAttributes { get; set; }
    }
}