namespace SimpleIdentityServer.Core.Common.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class UserResourceResponse : IdentifiedScimResource
    {
        [DataMember(Name = ScimConstants.UserResourceResponseNames.UserName)]
        public string UserName { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Name)]
        public Name Name { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.DisplayName)]
        public string DisplayName { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.NickName)]
        public string NickName { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.ProfileUrl)]
        public string ProfileUrl { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Title)]
        public string Title { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.UserType)]
        public string UserType { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.PreferredLanguage)]
        public string PreferredLanguage { get; set; }
        /// <summary>
        /// Read the RFC : https://tools.ietf.org/html/rfc5646
        /// </summary>
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Locale)]
        public string Locale { get; set; }
        /// <summary>
        /// Read the RFC : https://tools.ietf.org/html/rfc6557
        /// </summary>
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Timezone)]
        public string Timezone { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Active)]
        public bool Active { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Password)]
        public string Password { get; set; }
        /*
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Emails)]
        public IEnumerable<MultiValueAttrResponse> Emails { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Phones)]
        public IEnumerable<MultiValueAttrResponse> Phones { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Ims)]
        public IEnumerable<MultiValueAttrResponse> Ims { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Photos)]
        public IEnumerable<MultiValueAttrResponse> Photos { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Addresses)]
        public IEnumerable<AddressResponse> Addresses { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Groups)]
        public IEnumerable<AddressResponse> Groups { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Entitlements)]
        public IEnumerable<MultiValueAttrResponse> Entitlements { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Roles)]
        public IEnumerable<MultiValueAttrResponse> Roles { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.X509Certificates)]
        public IEnumerable<MultiValueAttrResponse> X509Certificates { get; set; }
        */
    }
}