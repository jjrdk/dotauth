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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Common.Models;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.Factories
{
    public interface ICommonAttributesFactory
    {
        JProperty CreateIdJson(Representation representation);
        JProperty CreateIdJson(string id);
        Task<RepresentationAttribute> CreateId(Representation representation);
        IEnumerable<JProperty> CreateMetaDataAttributeJson(Representation representation, string location);
        Task<RepresentationAttribute> CreateMetaDataAttribute(Representation representation, string location);
        string GetFullPath(string key);
    }

    internal class CommonAttributesFactory : ICommonAttributesFactory
    {
        private readonly Dictionary<string, string> _mappingCommonAttrsKeysWithFullPath = new Dictionary<string, string>
        {
            {
                Common.ScimConstants.MetaResponseNames.ResourceType, Common.ScimConstants.ScimResourceNames.Meta + "." +Common.ScimConstants.MetaResponseNames.ResourceType
            },
            {
                Common.ScimConstants.MetaResponseNames.Version, Common.ScimConstants.ScimResourceNames.Meta + "." +Common.ScimConstants.MetaResponseNames.Version
            },
            {
                Common.ScimConstants.MetaResponseNames.Created, Common.ScimConstants.ScimResourceNames.Meta + "." +Common.ScimConstants.MetaResponseNames.Created
            },
            {
                Common.ScimConstants.MetaResponseNames.LastModified, Common.ScimConstants.ScimResourceNames.Meta + "." +Common.ScimConstants.MetaResponseNames.LastModified
            },
            {
                Common.ScimConstants.MetaResponseNames.Location, Common.ScimConstants.ScimResourceNames.Meta + "." +Common.ScimConstants.MetaResponseNames.Location
            },
            {
                Common.ScimConstants.ScimResourceNames.Meta, Common.ScimConstants.ScimResourceNames.Meta
            },
            {
                Common.ScimConstants.ScimResourceNames.Schemas, Common.ScimConstants.ScimResourceNames.Schemas
            },
            {
                Common.ScimConstants.IdentifiedScimResourceNames.ExternalId, Common.ScimConstants.IdentifiedScimResourceNames.ExternalId
            },
            {
                Common.ScimConstants.IdentifiedScimResourceNames.Id, Common.ScimConstants.IdentifiedScimResourceNames.Id
            }
        };
        private readonly ISchemaStore _schemaStore;

        public CommonAttributesFactory(ISchemaStore schemaStore)
        {
            _schemaStore = schemaStore;
        }

        public JProperty CreateIdJson(Representation representation)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            return CreateIdJson(representation.Id);
        }

        public JProperty CreateIdJson(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return new JProperty(Common.ScimConstants.IdentifiedScimResourceNames.Id, id);
        }

        public async Task<RepresentationAttribute> CreateId(Representation representation)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            var commonAttrs = await _schemaStore.GetCommonAttributes().ConfigureAwait(false);
            var idAttr = commonAttrs.First(n => n.Name == Common.ScimConstants.IdentifiedScimResourceNames.Id);
            return new SingularRepresentationAttribute<string>(idAttr, representation.Id);
        }

        public async Task<RepresentationAttribute> CreateMetaDataAttribute(Representation representation, string location)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentNullException(nameof(location));
            }

            var commonAttrs = await _schemaStore.GetCommonAttributes().ConfigureAwait(false);
            var metaAttr = commonAttrs.First(m => m.Name == Common.ScimConstants.ScimResourceNames.Meta) as ComplexSchemaAttributeResponse;
            return new ComplexRepresentationAttribute(metaAttr)
            {
                Values = new RepresentationAttribute[]
                {
                    new SingularRepresentationAttribute<string>(metaAttr.SubAttributes.First(a => a.Name == Common.ScimConstants.MetaResponseNames.ResourceType), representation.ResourceType),
                    new SingularRepresentationAttribute<DateTime>(metaAttr.SubAttributes.First(a => a.Name == Common.ScimConstants.MetaResponseNames.Created), representation.Created),
                    new SingularRepresentationAttribute<DateTime>(metaAttr.SubAttributes.First(a => a.Name == Common.ScimConstants.MetaResponseNames.LastModified), representation.LastModified),
                    new SingularRepresentationAttribute<string>(metaAttr.SubAttributes.First(a => a.Name == Common.ScimConstants.MetaResponseNames.Version), representation.Version),
                    new SingularRepresentationAttribute<string>(metaAttr.SubAttributes.First(a => a.Name == Common.ScimConstants.MetaResponseNames.Location), location)
                }
            };
        }

        public IEnumerable<JProperty> CreateMetaDataAttributeJson(Representation representation, string location)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentNullException(nameof(location));
            }

            return new JProperty[]
            {
                new JProperty(Common.ScimConstants.MetaResponseNames.ResourceType, representation.ResourceType),
                new JProperty(Common.ScimConstants.MetaResponseNames.Created, representation.Created),
                new JProperty(Common.ScimConstants.MetaResponseNames.LastModified, representation.LastModified),
                new JProperty(Common.ScimConstants.MetaResponseNames.Version, representation.Version),
                new JProperty(Common.ScimConstants.MetaResponseNames.Location, location)
            };
        }

        public string GetFullPath(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_mappingCommonAttrsKeysWithFullPath.ContainsKey(key))
            {
                return null;
            }

            return _mappingCommonAttrsKeysWithFullPath[key];
        }
    }
}
