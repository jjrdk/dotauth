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
using SimpleIdentityServer.Scim.Core.Errors;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using SimpleIdentityServer.Core.Common;
    using SimpleIdentityServer.Core.Common.DTOs;
    using SimpleIdentityServer.Core.Common.Models;

    internal class RepresentationResponseParser : IRepresentationResponseParser
    {
        private readonly ISchemaStore _schemasStore;
        private readonly ICommonAttributesFactory _commonAttributesFactory;
        //private readonly IEnumerable<IAttributeMapper> _attributeMappers;

        public RepresentationResponseParser(ISchemaStore schemaStore, ICommonAttributesFactory commonAttributeFactory)
        {
            _schemasStore = schemaStore;
            _commonAttributesFactory = commonAttributeFactory;
           // _attributeMappers = attributeMappers;
        }

        /// <summary>
        /// Parse the representation into JSON and returns the result.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when parameters are null empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when error occured during the parsing.</exception>
        /// <param name="representation">Representation that will be parsed.</param>
        /// <param name="location">Location of the representation</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="operationType">Type of operation.</param>
        /// <returns>JSON representation</returns>
        public async Task<Response> Parse(Representation representation, string location, string schemaId, OperationTypes operationType)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            if (string.IsNullOrWhiteSpace(schemaId))
            {
                throw new ArgumentNullException(nameof(schemaId));
            }

            var schema = await _schemasStore.GetSchema(schemaId).ConfigureAwait(false);
            if (schema == null)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheSchemaDoesntExist, schemaId));
            }

            //if (_attributeMappers != null && _attributeMappers.Any())
            //{
            //    await _attributeMappers.First().Map(representation, schemaId).ConfigureAwait(false);
            //}

            JObject result = new JObject();
            if (representation.Attributes != null &&
                representation.Attributes.Any())
            {
                foreach (var attribute in schema.Attributes)
                {
                    // Ignore the attributes.
                    if ((attribute.Returned == ScimConstants.SchemaAttributeReturned.Never) || (operationType == OperationTypes.Query && attribute.Returned == ScimConstants.SchemaAttributeReturned.Request))
                    {
                        continue;
                    }
                    
                    var attr = representation.Attributes.FirstOrDefault(a => a.SchemaAttribute.Name == attribute.Name);
                    var token = GetToken(attr, attribute);
                    if (token != null)
                    {
                        result.Add(token);
                    }
                }
            }
            
            SetCommonAttributes(result, location, representation, schema.Id);
            return new Response
            {
                Location = location,
                Object = result
            };
        }

        /// <summary>
        /// Filter the representations and return the result.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when representations are null.</exception>
        /// <param name="representations">Representations to filter.</param>
        /// <param name="searchParameter">Search parameters.</param>
        /// <param name="totalNumbers">Total number of records</param>
        /// <returns>Filtered response</returns>
        public FilterResult Filter(IEnumerable<Representation> representations, SearchParameter searchParameter, int totalNumbers)
        {
            if (representations == null)
            {
                throw new ArgumentNullException(nameof(representations));
            }

            if (searchParameter == null)
            {
                throw new ArgumentNullException(nameof(searchParameter));
            }

            IEnumerable<string> commonAttrs = new[]
            {
                ScimConstants.MetaResponseNames.ResourceType,
                ScimConstants.MetaResponseNames.Created,
                ScimConstants.MetaResponseNames.LastModified,
                ScimConstants.MetaResponseNames.Version,
                ScimConstants.MetaResponseNames.Location
            };
                                   
            var result = new JArray();
            // 1. Sort the representations.
            if (searchParameter.SortBy != null)
            {
                var comparer = new RepresentationComparer(searchParameter.SortBy);
                if (searchParameter.SortOrder == SortOrders.Ascending)
                {
                    representations = representations.OrderBy(repr => repr, comparer);
                }
                else
                {
                    representations = representations.OrderByDescending(repr => repr, comparer);
                }
            }
            
            // 2. Filter the representations.
            foreach(var representation in representations)
            {
                // 2.1 Exclude & include certains attributes.
                IEnumerable<RepresentationAttribute> attributes = null;
                if (searchParameter.ExcludedAttributes != null && searchParameter.ExcludedAttributes.Any())
                {
                    foreach (var excludedAttrFilter in searchParameter.ExcludedAttributes)
                    {
                        var excludedAttrs = excludedAttrFilter.Evaluate(representation);
                        if (excludedAttrs == null)
                        {
                            continue;
                        }

                        foreach (var excludedAttr in excludedAttrs)
                        {
                            var excludedParent = excludedAttr.Parent as ComplexRepresentationAttribute;
                            if (excludedParent == null)
                            {
                                continue;
                            }

                            excludedParent.Values = excludedParent.Values.Where(v => !excludedAttrs.Contains(v));
                        }
                    }
                }

                if (searchParameter.Attributes != null && searchParameter.Attributes.Any())
                {
                    attributes = new List<RepresentationAttribute>();
                    foreach (var attrFilter in searchParameter.Attributes)
                    {
                        attributes = attributes.Concat(attrFilter.Evaluate(representation));
                    }
                }
                else
                {
                    attributes = representation.Attributes;
                }

                if (attributes == null || !attributes.Any())
                {
                    continue;
                }

                // 2.2 Add all attributes
                var obj = new JObject();
                foreach (JProperty token in attributes.Select(a => (JProperty)GetToken(a, a.SchemaAttribute)))
                {
                    var value = obj[token.Name];
                    if (value != null)
                    {
                        var arr = value as JArray;
                        if (arr != null)
                        {
                            arr.Add(token.Value);
                        }
                        else
                        {
                            obj[token.Name] = new JArray(value, token.Value);
                        }
                    }
                    else
                    {
                        obj.Add(token);
                    }
                }

                result.Add(obj);
            }

            var filterResult = new FilterResult();
            if (result.Count() > searchParameter.Count)
            {
                filterResult.StartIndex = searchParameter.StartIndex;
                filterResult.ItemsPerPage = searchParameter.Count;
                filterResult.Values = result;
            }
            else
            {
                filterResult.Values = result;
            }

            filterResult.TotalNumbers = totalNumbers;
            // 3. Paginate the representations.
            return filterResult;
        }

        private void SetCommonAttributes(JObject jObj, string location, Representation representation, string schema)
        {
            jObj.Add(_commonAttributesFactory.CreateIdJson(representation));
            jObj[ScimConstants.ScimResourceNames.Meta] = new JObject(_commonAttributesFactory.CreateMetaDataAttributeJson(representation, location));
            var arr = new JArray();
            arr.Add(schema);
            jObj[ScimConstants.ScimResourceNames.Schemas] = arr;
        }

        private static JToken GetToken(RepresentationAttribute attr, SchemaAttributeResponse attribute)
        {
            // 1. Check the attribute is required
            if (attr == null)
            {
                if (attribute.Required)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsRequired, attribute.Name));
                }

                return null;
            }

            // 2. Create complex attribute
            var complexAttribute = attribute as ComplexSchemaAttributeResponse;
            if (complexAttribute != null)
            {
                var complexRepresentation = attr as ComplexRepresentationAttribute;
                if (complexRepresentation == null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeIsNotComplex, attribute.Name));
                }

                // 2.1 Complex attribute[Complex attribute]
                if (attribute.MultiValued)
                {
                    var array = new JArray();
                    if (complexRepresentation.Values != null)
                    {
                        foreach (var subRepresentation in complexRepresentation.Values)
                        {
                            var subComplex = subRepresentation as ComplexRepresentationAttribute;
                            if (subComplex == null)
                            {
                                throw new InvalidOperationException(ErrorMessages.TheComplexAttributeArrayShouldContainsOnlyComplexAttribute);
                            }

                            var obj = new JObject();
                            foreach (var subAttr in subComplex.Values)
                            {
                                var att = complexAttribute.SubAttributes.FirstOrDefault(a => a.Name == subAttr.SchemaAttribute.Name);
                                if (att == null)
                                {
                                    continue;
                                }

                                obj.Add(GetToken(subAttr, att));
                            }

                            array.Add(obj);
                        }
                    }

                    return new JProperty(complexRepresentation.SchemaAttribute.Name, array);
                }

                var properties = new List<JToken>();
                // 2.2 Complex attribute
                if (complexRepresentation.Values != null)
                {
                    foreach (var subRepresentation in complexRepresentation.Values)
                    {
                        var subAttribute = complexAttribute.SubAttributes.FirstOrDefault(a => a.Name == subRepresentation.SchemaAttribute.Name);
                        if (subAttribute == null)
                        {
                            continue;
                        }

                        properties.Add(GetToken(subRepresentation, subAttribute));
                    }
                }

                var props = new JObject(properties);
                return new JProperty(complexRepresentation.SchemaAttribute.Name, props);
            }
            
            // 3. Create singular attribute
            switch(attribute.Type)
            {
                case ScimConstants.SchemaAttributeTypes.String:
                case ScimConstants.SchemaAttributeTypes.Reference:
                    return GetSingularToken<string>(attribute, attr, attribute.MultiValued);
                case ScimConstants.SchemaAttributeTypes.Boolean:
                    return GetSingularToken<bool>(attribute, attr, attribute.MultiValued);
                case ScimConstants.SchemaAttributeTypes.Decimal:
                    return GetSingularToken<decimal>(attribute, attr, attribute.MultiValued);
                case ScimConstants.SchemaAttributeTypes.DateTime:
                    return GetSingularToken<DateTime>(attribute, attr, attribute.MultiValued);
                case ScimConstants.SchemaAttributeTypes.Integer:
                    return GetSingularToken<int>(attribute, attr, attribute.MultiValued);
                default:
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotSupported, attribute.Type));
            }

        }

        private static JToken GetSingularToken<T>(SchemaAttributeResponse attribute, RepresentationAttribute attr, bool isArray)
        {
            if (isArray)
            {
                var enumSingularRepresentation = attr as SingularRepresentationAttribute<IEnumerable<T>>;
                if (enumSingularRepresentation == null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, attribute.Type));
                }

                return new JProperty(enumSingularRepresentation.SchemaAttribute.Name, enumSingularRepresentation.Value);
            }
            else
            {
                var singularRepresentation = attr as SingularRepresentationAttribute<T>;
                if (singularRepresentation == null)
                {
                    throw new InvalidOperationException(string.Format(ErrorMessages.TheAttributeTypeIsNotCorrect, attribute.Name, attribute.Type));
                }

                return new JProperty(singularRepresentation.SchemaAttribute.Name, singularRepresentation.Value);
            }
        }
    }
}
