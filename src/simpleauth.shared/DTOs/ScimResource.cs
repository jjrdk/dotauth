namespace SimpleAuth.Shared.DTOs
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Parameters are listed here : https://tools.ietf.org/html/rfc7643#section-3
    /// </summary>
    [DataContract]
    public class ScimResource
    {
        [DataMember(Name = ScimConstants.ScimResourceNames.Schemas)]
        public IEnumerable<string> Schemas { get; set; }

        [DataMember(Name = ScimConstants.ScimResourceNames.Meta)]
        public Meta Meta { get; set; }
    }
}