namespace SimpleAuth.Shared.DTOs
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class ScimUser : IdentifiedScimResource
    {
        public ScimUser()
        {
            Id = Guid.NewGuid().ToString("N");
            Schemas = new[] { ScimConstants.SchemaUrns.User };
        }

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

        [DataMember(Name = ScimConstants.UserResourceResponseNames.Emails)]
        public TypedString[] Emails { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.PhoneNumbers)]
        public TypedString[] PhoneNumbers { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Ims)]
        public TypedString[] Ims { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Photos)]
        public TypedString[] Photos { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Addresses)]
        public Address[] Addresses { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Groups)]
        public string[] Groups { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Entitlements)]
        public string Entitlements { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.Roles)]
        public string Roles { get; set; }
        [DataMember(Name = ScimConstants.UserResourceResponseNames.X509Certificates)]
        public string[] X509Certificates { get; set; }
    }
}