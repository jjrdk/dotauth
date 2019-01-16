namespace SimpleAuth.Shared.DTOs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    [DataContract]
    public class SchemaAttributeResponse
    {
        public string FullPath => GetFullPath();

        private string GetFullPath()
        {
            var parents = new List<SchemaAttributeResponse>();
            var names = new List<string>
            {
                Name
            };

            GetParents(this, parents);
            parents.Reverse();
            var parentNames = names.Concat(parents.Select(p => p.Name));
            return string.Join(".", parentNames);
        }

        private IEnumerable<SchemaAttributeResponse> GetParents(SchemaAttributeResponse representation, IEnumerable<SchemaAttributeResponse> parents)
        {
            if (representation.Parent == null)
            {
                return parents;
            }

            parents = parents.Concat(new[] { representation });
            return GetParents(representation.Parent, parents);
        }

        public SchemaAttributeResponse Parent { get; set; }

        /// <summary>
        /// Gets or sets the id
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.Id)]
        public string Id { get; set; }
        /// <summary>
        /// Attribute's name
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.Name)]
        public string Name { get; set; }
        /// <summary>
        /// Attribute's data type. Valid values are : "string", "boolean", "decimal", "integer" etc ...
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.Type)]
        public string Type { get; set; }
        /// <summary>
        /// Indicate attribute's plurality
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.MultiValued)]
        public bool MultiValued { get; set; }
        /// <summary>
        /// Human-readable description
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.Description)]
        public string Description { get; set; }
        /// <summary>
        /// Specifies whether or not the attribute is required
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.Required)]
        public bool Required { get; set; }
        /// <summary>
        /// Collection of suggested canonical values
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.CanonicalValues)]
        public IEnumerable<string> CanonicalValues { get; set; }
        /// <summary>
        /// Specifies whether or not a string attribute is case sensitive
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.CaseExact)]
        public bool CaseExact { get; set; }
        /// <summary>
        /// Circumstances under which the value of the attribute can be (re)defined
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.Mutability)]
        public string Mutability { get; set; }
        /// <summary>
        /// When an attribute and associated values are returned in response to a GET or in response to PUT etc ...
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.Returned)]
        public string Returned { get; set; }
        /// <summary>
        /// How the service provider enforces uniqueness of attribute values
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.Uniqueness)]
        public string Uniqueness { get; set; }
        /// <summary>
        /// Indicate the SCIM resource types that may be referenced.
        /// </summary>
        [DataMember(Name = ScimConstants.SchemaAttributeResponseNames.ReferenceTypes)]
        public IEnumerable<string> ReferenceTypes { get; set; }
    }
}