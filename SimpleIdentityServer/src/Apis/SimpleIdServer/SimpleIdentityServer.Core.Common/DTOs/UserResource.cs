// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Common.DTOs
{
    [DataContract]
    public class Name
    {
        [DataMember(Name = ScimConstants.NameResponseNames.Formatted)]
        public string Formatted { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.FamilyName)]
        public string FamilyName { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.GivenName)]
        public string GivenName { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.MiddleName)]
        public string MiddleName { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.HonorificPrefix)]
        public string HonorificPrefix { get; set; }
        [DataMember(Name = ScimConstants.NameResponseNames.HonorificSuffix)]
        public string HonorificSuffix { get; set; }
    }

    /*
    [DataContract]
    public class Address : MultiValueAttrResponse
    {
        [DataMember(Name = ScimConstants.AddressResponseNames.Formatted)]
        public string Formatted { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.StreetAddress)]
        public string StreetAddress { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.Locality)]
        public string Locality { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.Region)]
        public string Region { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.PostalCode)]
        public string PostalCode { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.Country)]
        public string Country { get; set; }
    }
    */

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