namespace SimpleAuth.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class AuthenticationSchemeResponse : MultiValueAttr
    {
        /// <summary>
        /// Authentication scheme.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.Type)]
        public AuthenticationTypes AuthenticationType { get; set; }

        /// <summary>
        /// Common authentication scheme name.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.Name)]
        public string Name { get; set; }

        /// <summary>
        /// A description of the authentication scheme.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.Description)]
        public string Description { get; set; }

        /// <summary>
        /// An HTTP-Addressable URL pointing to the authentication schem's specification.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.SpecUri)]
        public string SpecUri { get; set; }

        /// <summary>
        /// An HTTP-Addressable URL pointing to the authentication scheme's usage documentation.
        /// </summary>
        [DataMember(Name = ScimConstants.AuthenticationSchemeResponseNames.DocumentationUri)]
        public string DocumentationUri { get; set; }
    }
}