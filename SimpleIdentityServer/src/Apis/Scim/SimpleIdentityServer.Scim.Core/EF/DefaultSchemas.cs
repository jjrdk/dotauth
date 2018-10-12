﻿// Copyright 2016 Habart Thierry
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

using SimpleIdentityServer.Scim.Core.EF.Extensions;
using SimpleIdentityServer.Scim.Core.EF.Models;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Core.EF
{
    public static class DefaultSchemas
    {
        private static List<SchemaAttribute> UserMetaDataAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.ResourceType, "Name of the resource type of the resource", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, caseExact: true),
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.Created, "The 'DateTime' that the resource was added to the service provider", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, type: Common.ScimConstants.SchemaAttributeTypes.DateTime),
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.LastModified, "The most recent DateTime than the details of this resource were updated at the service provider", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, type: Common.ScimConstants.SchemaAttributeTypes.DateTime),
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.Location, "URI of the resource being returned", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly),
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.Version, "Version of the resource being returned", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, caseExact: true),
        };


        private static List<SchemaAttribute> GroupMetaDataAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.ResourceType, "Name of the resource type of the resource", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, caseExact: true),
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.Created, "The 'DateTime' that the resource was added to the service provider", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, type: Common.ScimConstants.SchemaAttributeTypes.DateTime),
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.LastModified, "The most recent DateTime than the details of this resource were updated at the service provider", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, type: Common.ScimConstants.SchemaAttributeTypes.DateTime),
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.Location, "URI of the resource being returned", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly),
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.MetaResponseNames.Version, "Version of the resource being returned", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, caseExact: true),
        };

        private static class SchemaAttributeFactory
        {
            public static SchemaAttribute CreateAttribute(
                string name,
                string description,
                string type = Common.ScimConstants.SchemaAttributeTypes.String,
                string mutability = Common.ScimConstants.SchemaAttributeMutability.ReadWrite,
                string returned = Common.ScimConstants.SchemaAttributeReturned.Default,
                string uniqueness = Common.ScimConstants.SchemaAttributeUniqueness.None,
                bool caseExact = false,
                bool required = false,
                bool multiValued = false,
                string[] referenceTypes = null,
                string[] canonicalValues = null,
                bool isCommon = false)
            {
                return new SchemaAttribute
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Type = type,
                    MultiValued = multiValued,
                    Description = description,
                    Required = required,
                    CaseExact = caseExact,
                    Mutability = mutability,
                    Returned = returned,
                    Uniqueness = uniqueness,
                    ReferenceTypes = MappingExtensions.ConcatList(referenceTypes),
                    CanonicalValues = MappingExtensions.ConcatList(canonicalValues),
                    IsCommon = isCommon
                };
            }

            public static SchemaAttribute CreateComplexAttribute(
                string name,
                string description,
                List<SchemaAttribute> subAttributes,
                string type = Common.ScimConstants.SchemaAttributeTypes.String,
                bool multiValued = false,
                bool required = false,
                bool caseExact = false,
                string mutability = Common.ScimConstants.SchemaAttributeMutability.ReadWrite,
                string returned = Common.ScimConstants.SchemaAttributeReturned.Default,
                string uniqueness = Common.ScimConstants.SchemaAttributeUniqueness.None,
                string[] referenceTypes = null,
                string[] canonicalValues = null,
                bool isCommon = false)
            {
                return new SchemaAttribute
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    MultiValued = multiValued,
                    Description = description,
                    Required = required,
                    CaseExact = caseExact,
                    Mutability = mutability,
                    Returned = returned,
                    Uniqueness = uniqueness,
                    ReferenceTypes = MappingExtensions.ConcatList(referenceTypes),
                    CanonicalValues = MappingExtensions.ConcatList(canonicalValues),
                    Children = subAttributes,
                    IsCommon = isCommon,
                    Type = Common.ScimConstants.SchemaAttributeTypes.Complex
                };
            }

            public static SchemaAttribute CreateValueAttribute(
                string description,
                string[] referenceTypes = null,
                string type = Common.ScimConstants.SchemaAttributeTypes.String,
                string mutability = Common.ScimConstants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                        Common.ScimConstants.MultiValueAttributeNames.Value,
                        description,
                        type: type,
                        referenceTypes: referenceTypes,
                        mutability: mutability);
            }

            public static SchemaAttribute CreateDisplayAttribute(
                string description,
                string mutability = Common.ScimConstants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                        Common.ScimConstants.MultiValueAttributeNames.Display,
                        description,
                        mutability: mutability);
            }

            public static SchemaAttribute CreateTypeAttribute(
                string description,
                string[] canonicalValues,
                string mutability = Common.ScimConstants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Common.ScimConstants.MultiValueAttributeNames.Type,
                    description,
                    canonicalValues: canonicalValues,
                    mutability: mutability);
            }

            public static SchemaAttribute CreatePrimaryAttribute(
                string description,
                string mutability = Common.ScimConstants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Common.ScimConstants.MultiValueAttributeNames.Primary,
                    description,
                    type: Common.ScimConstants.SchemaAttributeTypes.Boolean,
                    mutability: mutability);
            }

            public static SchemaAttribute CreateRefAttribute(
                string description,
                string[] referenceTypes,
                string mutability = Common.ScimConstants.SchemaAttributeMutability.ReadWrite)
            {
                return CreateAttribute(
                    Common.ScimConstants.MultiValueAttributeNames.Ref,
                    description,
                    type: Common.ScimConstants.SchemaAttributeTypes.Reference,
                    referenceTypes: referenceTypes,
                    mutability: mutability);
            }
        }

        private static List<SchemaAttribute> EmailAttributeSub = new List<SchemaAttribute>
        {
           SchemaAttributeFactory.CreateValueAttribute("Email addresses for the user.  The value SHOULD be canonicalized by the service provider, e.g., 'bjensen@example.com' instead of 'bjensen@EXAMPLE.COM'. Canonical type values of 'work', 'home', and 'other'."),
           SchemaAttributeFactory.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
           SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'work' or 'home'.", new string[] { "work", "home", "other" }),
           SchemaAttributeFactory.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute, e.g., the preferred mailing address or primary email address. The primary attribute value 'true' MUST appear no more than once.")
        };

        private static List<SchemaAttribute> UserPhoneNumberAttributes = new List<SchemaAttribute>
        {
           SchemaAttributeFactory.CreateValueAttribute("Phone number of the User."),
           SchemaAttributeFactory.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
           SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'work', 'home', 'mobile'.", new string[] { "work", "home", "mobile", "fax", "pager", "other" }),
           SchemaAttributeFactory.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute, e.g., the preferred phone number or primary phone number.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static List<SchemaAttribute> UserImsAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateValueAttribute("Instant messaging address for the User."),
            SchemaAttributeFactory.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.READ - ONLY."),
            SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'aim', 'gtalk', 'xmpp'.", new string[] { "aim", "gtalk", "icq", "xmpp", "msn", "skype", "qq", "yahoo" }),
            SchemaAttributeFactory.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute, e.g., the preferred messenger or primary messenger. The primary attribute value 'true' MUST appear no more than once."),
        };

        private static List<SchemaAttribute> UserPhotoAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateValueAttribute("URL of a photo of the User.", referenceTypes: new string [] { "external" }, type: Common.ScimConstants.SchemaAttributeTypes.Reference),
            SchemaAttributeFactory.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
            SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function, i.e., 'photo' or 'thumbnail'.", new string[] { "photo", "thumbnail" }),
            SchemaAttributeFactory.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute, e.g., the preferred photo or thumbnail.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static List<SchemaAttribute> UserGroupAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateValueAttribute("The identifier of the User's group.", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly),
            SchemaAttributeFactory.CreateRefAttribute("The URI of the corresponding 'Group' resource to which the user belongs.", new string[] { "User", "Group" }, mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly),
            SchemaAttributeFactory.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY.", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly),
            SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'direct' or 'indirect'.", new string[] { "direct", "indirect" }, mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly),
        };

        private static List<SchemaAttribute> UserEntitlementAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateValueAttribute("The value of an entitlement."),
            SchemaAttributeFactory.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
            SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function.", new string[] { }),
            SchemaAttributeFactory.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static List<SchemaAttribute> UserRoleAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateValueAttribute("The value of a role."),
            SchemaAttributeFactory.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
            SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function.", new string[] { }),
            SchemaAttributeFactory.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static List<SchemaAttribute> UserCertificateAttributes = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateValueAttribute("The value of an X.509 certificate.", type: Common.ScimConstants.SchemaAttributeTypes.Binary),
            SchemaAttributeFactory.CreateDisplayAttribute("A human-readable name, primarily used for display purposes.  READ-ONLY."),
            SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function.", new string[] { }),
            SchemaAttributeFactory.CreatePrimaryAttribute("A Boolean value indicating the 'primary' or preferred attribute value for this attribute.  The primary attribute value 'true' MUST appear no more than once.")
        };

        private static List<SchemaAttribute> UserNameAttributeSub = new List<SchemaAttribute>
        {
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.NameResponseNames.Formatted, "The full name, including all middle names, titles, and suffixes as appropriate, formatted for display (e.g., 'Ms. Barbara J Jensen, III')."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.NameResponseNames.FamilyName, "The family name of the User, or last name in most Western languages (e.g., 'Jensen' given the fullname 'Ms. Barbara J Jensen, III')."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.NameResponseNames.GivenName, "The given name of the User, or first name in most Western languages (e.g., 'Barbara' given the full name 'Ms. Barbara J Jensen, III')."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.NameResponseNames.MiddleName, "The middle name(s) of the User (e.g., 'Jane' given the full name 'Ms. Barbara J Jensen, III')."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.NameResponseNames.HonorificPrefix, "The honorific prefix(es) of the User, or title in most Western languages (e.g., 'Ms.' given the full name 'Ms. Barbara J Jensen, III')."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.NameResponseNames.HonorificSuffix, "The honorific suffix(es) of the User, or suffix in most Western languages (e.g., 'III' given the full name 'Ms. Barbara J Jensen, III').")
        };

        private static List<SchemaAttribute> UserAddressAttributes = new List<SchemaAttribute>
        {
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.AddressResponseNames.Formatted, "The full mailing address, formatted for display or use with a mailing label.  This attribute MAY contain newlines."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.AddressResponseNames.StreetAddress, "The full street address component, which may include house number, street name, P.O. box, and multi-line extended street address information.  This attribute MAY contain newlines."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.AddressResponseNames.Locality, "The city or locality component."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.AddressResponseNames.Region, "The state or region component."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.AddressResponseNames.PostalCode, "The zip code or postal code component."),
             SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.AddressResponseNames.Country, "The country name component."),
             SchemaAttributeFactory.CreateTypeAttribute("A label indicating the attribute's function, e.g., 'work' or 'home'.", new string[] { "work", "home", "other" })
        };

        public static Schema UserSchema = new Schema
        {
            Id = Common.ScimConstants.SchemaUrns.User,
            Name = Common.ScimConstants.ResourceTypes.User,
            Description = "User Account",
            Attributes = new List<SchemaAttribute>
            {
                // user name
                SchemaAttributeFactory.CreateAttribute(
                    Common.ScimConstants.UserResourceResponseNames.UserName,
                    "Unique identifier for the User, typically"+
                                    "used by the user to directly authenticate to the service provider."+
                                    "Each User MUST include a non-empty userName value.  This identifier"+
                                    "MUST be unique across the service provider's entire set of Users."+
                                    "REQUIRED.",
                    uniqueness: Common.ScimConstants.SchemaAttributeUniqueness.Server,
                    required : false),
                // name
                SchemaAttributeFactory.CreateComplexAttribute(
                    Common.ScimConstants.UserResourceResponseNames.Name,
                     "The components of the user's real name."+
                                    "Providers MAY return just the full name as a single string in the"+
                                    "formatted sub-attribute, or they MAY return just the individual"+
                                    "component attributes using the other sub-attributes, or they MAY"+
                                    "return both.If both variants are returned, they SHOULD be"+
                                    "describing the same name, with the formatted name indicating how the"+
                                    "component attributes should be combined.",
                     UserNameAttributeSub),
                // Display name
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.DisplayName,
                     "The name of the User, suitable for display"+
                                    "to end-users.  The name SHOULD be the full name of the User being"+
                                    "described, if known."),
                // Nick name
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.NickName,
                     "The casual way to address the user in real"+
                                    "life, e.g., 'Bob' or 'Bobby' instead of 'Robert'.  This attribute"+
                                    "SHOULD NOT be used to represent a User's username (e.g., 'bjensen' or"+
                                    "'mpepperidge')."),
                // Profile url
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.ProfileUrl,
                     "A fully qualified URL pointing to a page"+
                                    "representing the User's online profile.",
                     type: Common.ScimConstants.SchemaAttributeTypes.Reference,
                     referenceTypes: new string [] { "external" }),
                // Title
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.Title,
                     "The user's title, such as"+
                                    "\"Vice President.\""),
                // User type
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.UserType,
                     "Used to identify the relationship between"+
                                    "the organization and the user.  Typical values used might be"+
                                    "'Contractor', 'Employee', 'Intern', 'Temp', 'External', and"+
                                    "'Unknown', but any value may be used."),
                // preferred language
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.PreferredLanguage,
                     "Indicates the User's preferred written or"+
                                    "spoken language.  Generally used for selecting a localized user"+
                                    "interface; e.g., 'en_US' specifies the language English and country"+
                                    "US."),
                // locale
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.Locale,
                     "Used to indicate the User's default location"+
                                    "for purposes of localizing items such as currency, date time format, or"+
                                    "numerical representations."),
                // time zone
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.Timezone,
                     "The User's time zone in the 'Olson' time zone"+
                                "database format, e.g., 'America/Los_Angeles'."),
                // active
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.Active,
                     "A Boolean value indicating the User's"+
                                    "administrative status.",
                     uniqueness: string.Empty,
                     caseExact : false,
                     type: Common.ScimConstants.SchemaAttributeTypes.Boolean),
                // password
                SchemaAttributeFactory.CreateAttribute(
                     Common.ScimConstants.UserResourceResponseNames.Password,
                     "The User's cleartext password.  This"+
                                    "attribute is intended to be used as a means to specify an initial"+
                                    "password when creating a new User or to reset an existing User's"+
                                    "password.",
                     returned: Common.ScimConstants.SchemaAttributeReturned.Never,
                     mutability: Common.ScimConstants.SchemaAttributeMutability.writeOnly),
                // Emails
                SchemaAttributeFactory.CreateComplexAttribute(
                     Common.ScimConstants.UserResourceResponseNames.Emails,
                     "Email addresses for the user.  The value"+
                        "SHOULD be canonicalized by the service provider, e.g.,"+
                        "'bjensen@example.com' instead of 'bjensen@EXAMPLE.COM'."+
                        "Canonical type values of 'work', 'home', and 'other'.",
                     EmailAttributeSub,
                     multiValued: true),
                // Phone numbers
                SchemaAttributeFactory.CreateComplexAttribute(
                    Common.ScimConstants.UserResourceResponseNames.Phones,
                    "Phone numbers for the User.  The value"+
                        "SHOULD be canonicalized by the service provider according to the"+
                        "format specified in RFC 3966, e.g., 'tel:+1-201-555-0123'."+
                        "Canonical type values of 'work', 'home', 'mobile', 'fax', 'pager',"+
                        "and 'other'.",
                    UserPhoneNumberAttributes,
                    multiValued: true),
                // Ims
                SchemaAttributeFactory.CreateComplexAttribute(
                    Common.ScimConstants.UserResourceResponseNames.Ims,
                    "Instant messaging addresses for the User.",
                    UserImsAttributes,
                    multiValued: true),
                // Addresses
                SchemaAttributeFactory.CreateComplexAttribute(
                    Common.ScimConstants.UserResourceResponseNames.Addresses,
                    "A physical mailing address for this User. Canonical type values of 'work', 'home', and 'other'.  This attribute is a complex type with the following sub-attributes.",
                    UserAddressAttributes,
                    multiValued: true),
                // Groups
                SchemaAttributeFactory.CreateComplexAttribute(
                    Common.ScimConstants.UserResourceResponseNames.Groups,
                    "A list of groups to which the user belongs, either through direct membership, through nested groups, or dynamically calculated.",
                    UserGroupAttributes,
                    multiValued: true,
                    mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly),
                // Entitlements
                SchemaAttributeFactory.CreateComplexAttribute(
                    Common.ScimConstants.UserResourceResponseNames.Entitlements,
                    "A list of entitlements for the User that represent a thing the User has.",
                    UserEntitlementAttributes,
                    multiValued: true),
                // Roles
                SchemaAttributeFactory.CreateComplexAttribute(
                    Common.ScimConstants.UserResourceResponseNames.Roles,
                    "A list of roles for the User that collectively represent who the User is, e.g., 'Student', 'Faculty'.",
                    UserRoleAttributes,
                    multiValued: true),
                // Certificate
                SchemaAttributeFactory.CreateComplexAttribute(
                    Common.ScimConstants.UserResourceResponseNames.X509Certificates,
                    "A list of certificates issued to the User.",
                    UserCertificateAttributes,
                    multiValued: true),
                SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.IdentifiedScimResourceNames.Id, "Unique identifier for a SCIM resource as defined by the service provider", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, caseExact: true, returned: Common.ScimConstants.SchemaAttributeReturned.Always, isCommon: true),
                SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.IdentifiedScimResourceNames.ExternalId, "Identifier as defined by the provisioning client", caseExact: true, mutability: Common.ScimConstants.SchemaAttributeMutability.ReadWrite, required: false, isCommon: true),
                SchemaAttributeFactory.CreateComplexAttribute(Common.ScimConstants.ScimResourceNames.Meta, "Complex attribute contaning resource metadata", UserMetaDataAttributes, mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, returned: Common.ScimConstants.SchemaAttributeReturned.Default, isCommon: true)
            },
            Meta = new MetaData
            {
                Id = Guid.NewGuid().ToString(),
                ResourceType = "Schema",
                Location = Common.ScimConstants.SchemaUrns.User
            }
        };

        private static List<SchemaAttribute> GroupMembersAttribute = new List<SchemaAttribute>
        {
            SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.GroupMembersResponseNames.Value, "Identifier of the member of this Group.", uniqueness: Common.ScimConstants.SchemaAttributeUniqueness.None, required : false, mutability: Common.ScimConstants.SchemaAttributeMutability.Immutable),
            SchemaAttributeFactory.CreateRefAttribute("The URI corresponding to a SCIM resource that is a member of this Group.", new string[] { "User", "Group" }, Common.ScimConstants.SchemaAttributeMutability.Immutable),
            SchemaAttributeFactory.CreateTypeAttribute("A label indicating the type of resource, e.g., 'User' or 'Group'.", new string[] { "User", "Group" }, Common.ScimConstants.SchemaAttributeMutability.Immutable)
        };

        public static Schema GroupSchema = new Schema
        {
            Id = Common.ScimConstants.SchemaUrns.Group,
            Name = Common.ScimConstants.ResourceTypes.Group,
            Description = "Group",
            Attributes = new List<SchemaAttribute>
            {
                SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.GroupResourceResponseNames.DisplayName, "A human-readable name for the Group. REQUIRED.", uniqueness: Common.ScimConstants.SchemaAttributeUniqueness.None, required : false),
                SchemaAttributeFactory.CreateComplexAttribute(Common.ScimConstants.GroupResourceResponseNames.Members, "A list of members of the Group.", GroupMembersAttribute, multiValued: true),
                SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.IdentifiedScimResourceNames.Id, "Unique identifier for a SCIM resource as defined by the service provider", mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, caseExact: true, returned: Common.ScimConstants.SchemaAttributeReturned.Always, isCommon: true),
                SchemaAttributeFactory.CreateAttribute(Common.ScimConstants.IdentifiedScimResourceNames.ExternalId, "Identifier as defined by the provisioning client", caseExact: true, mutability: Common.ScimConstants.SchemaAttributeMutability.ReadWrite, required: false, isCommon: true),
                SchemaAttributeFactory.CreateComplexAttribute(Common.ScimConstants.ScimResourceNames.Meta, "Complex attribute contaning resource metadata", GroupMetaDataAttributes, mutability: Common.ScimConstants.SchemaAttributeMutability.ReadOnly, returned: Common.ScimConstants.SchemaAttributeReturned.Default, isCommon: true)
            },
            Meta = new MetaData
            {
                Id = Guid.NewGuid().ToString(),
                ResourceType = "Schema",
                Location = Common.ScimConstants.SchemaUrns.Group
            }
        };
    }
}
