//// Copyright 2015 Habart Thierry
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////     http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json.Linq;
//using SimpleIdentityServer.Scim.Host.Extensions;
//using System;
//using System.Threading.Tasks;

//namespace SimpleIdentityServer.Scim.Host.Controllers
//{
//    using Common.Dtos.Events.Scim;
//    using Core.Errors;
//    using Microsoft.AspNetCore.Http;
//    using Newtonsoft.Json;
//    using SimpleIdentityServer.Core.Common;
//    using SimpleIdentityServer.Core.Common.DTOs;
//    using SimpleIdentityServer.Core.Common.Models;
//    using System.Collections;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Linq.Expressions;
//    using System.Net;
//    using System.Reflection;
//    using System.Threading;
//    using SearchParameter = SimpleIdentityServer.Core.Common.SearchParameter;
//    using SortOrders = SimpleIdentityServer.Core.Common.SortOrders;

//    [Route(Core.JwtConstants.RoutePaths.GroupsController)]
//    public class GroupsController : Controller
//    {
//        private readonly IProvide<GroupResource> _groupRepository;
//        private readonly GroupsAction _groupsAction;

//        public GroupsController(
//            IEventPublisher eventPublisher,
//            IStore<GroupResource> groupRepository)
//        {
//            _groupRepository = groupRepository;
//            _groupsAction = new GroupsAction(eventPublisher, groupRepository);
//        }

//        [Authorize(ScimConstants.ScimPolicies.ScimManage)]
//        [HttpPost]
//        public async Task<ActionResult> AddGroup([FromBody] GroupResource group)
//        {
//            if (@group == null)
//            {
//                return new BadRequestResult();
//            }

//            var result = await _groupsAction.AddGroup(group, GetLocationPattern()).ConfigureAwait(false);

//            return Created(result.Metadata.Location, group);
//        }

//        [Authorize(ScimConstants.ScimPolicies.ScimRead)]
//        [HttpGet("{id}")]
//        public async Task<ActionResult> GetGroup(string id)
//        {
//            if (string.IsNullOrWhiteSpace(id))
//            {
//                throw new ArgumentNullException(nameof(id));
//            }

//            //var searchParameter = _searchParameterParser.ParseQuery(query);
//            var result = await _groupRepository.Get(id, CancellationToken.None).ConfigureAwait(false);

//            return new ObjectResult(result); //this.GetActionResult(result);
//        }

//        [Authorize(ScimConstants.ScimPolicies.ScimRead)]
//        [HttpGet]
//        public async Task<ActionResult> SearchGroups()
//        {
//            var result = await _groupsAction.SearchGroups(Request.Query, GetLocationPattern()).ConfigureAwait(false);

//            return Ok(result);
//        }

//        [Authorize(ScimConstants.ScimPolicies.ScimRead)]
//        [HttpPost(".search")]
//        public async Task<ActionResult> SearchGroups([FromBody] JObject jObj)
//        {
//            var result = await _groupsAction.SearchGroups(jObj, GetLocationPattern()).ConfigureAwait(false);

//            return Ok(result);
//        }

//        [Authorize(ScimConstants.ScimPolicies.ScimManage)]
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteGroup(string id, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrWhiteSpace(id))
//            {
//                throw new ArgumentNullException(nameof(id));
//            }

//            var result = await _groupsAction.RemoveGroup(id, cancellationToken).ConfigureAwait(false);

//            return result
//                ? (IActionResult)NoContent()
//                : BadRequest();
//        }

//        [Authorize(ScimConstants.ScimPolicies.ScimManage)]
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateGroup(string id, [FromBody] GroupResource @group)
//        {
//            if (@group == null)
//            {
//                throw new ArgumentNullException(nameof(@group));
//            }

//            var result = await _groupsAction.UpdateGroup(id, @group).ConfigureAwait(false);
//            //if (result.IsSucceed())
//            //{
//            //    await _representationManager.AddOrUpdateRepresentationAsync(this, string.Format(GroupsName, result.Id), result.Version, true);
//            //}
//            return result != null
//                ? (IActionResult)Ok(result)
//                : StatusCode((int)HttpStatusCode.BadRequest);
//        }

//        [Authorize(ScimConstants.ScimPolicies.ScimManage)]
//        [HttpPatch("{id}")]
//        public async Task<IActionResult> PatchGroup(string id, [FromBody] PatchRequest patchRequest, CancellationToken cancellationToken)
//        {
//            if (string.IsNullOrWhiteSpace(id))
//            {
//                throw new ArgumentNullException(nameof(id));
//            }

//            if (patchRequest == null)
//            {
//                throw new ArgumentNullException(nameof(patchRequest));
//            }

//            var result = await _groupsAction.PatchGroup(id, patchRequest.Operations, cancellationToken).ConfigureAwait(false);

//            return result != null
//                ? (IActionResult)Ok(result)
//                : StatusCode((int)HttpStatusCode.BadRequest);
//        }

//        private string GetLocationPattern()
//        {
//            return new Uri(new Uri(Request.GetAbsoluteUriWithVirtualPath()), Core.JwtConstants.RoutePaths.GroupsController).AbsoluteUri + "/{id}";
//        }

//        private class GroupsAction
//        {
//            private static readonly PropertyInfo[] _properties = typeof(GroupResource).GetProperties();
//            private readonly SearchParameterParser _searchParameterParser;
//            private readonly IEventPublisher _eventPublisher;
//            private readonly IStore<GroupResource> _groupRepository;

//            public GroupsAction(
//                IEventPublisher eventPublisher,
//                IStore<GroupResource> groupRepository)
//            {
//                _searchParameterParser = new SearchParameterParser();
//                _eventPublisher = eventPublisher;
//                _groupRepository = groupRepository;
//            }

//            public async Task<GroupResource> AddGroup(GroupResource group, string locationBase, CancellationToken cancellationToken = default(CancellationToken))
//            {
//                var processId = Guid.NewGuid().ToString();
//                try
//                {
//                    if (string.IsNullOrWhiteSpace(group.Id))
//                    {
//                        group.Id = Guid.NewGuid().ToString("N");
//                    }
//                    group.Metadata = new ResourceMetadata
//                    {
//                        Location = $"{locationBase}/{group.Id}",
//                        Created = DateTime.UtcNow
//                    };
//                    _eventPublisher.Publish(new AddGroupReceived(Guid.NewGuid().ToString(), processId, null, 0));
//                    var result = await _groupRepository.Persist(group, cancellationToken).ConfigureAwait(false);
//                    _eventPublisher.Publish(new AddGroupFinished(Guid.NewGuid().ToString(), processId, JsonConvert.SerializeObject(result).ToString(), 1));
//                    return group;
//                }
//                catch (Exception ex)
//                {
//                    _eventPublisher.Publish(new ScimErrorReceived(Guid.NewGuid().ToString(), processId, ex.Message, 1));
//                    throw;
//                }
//            }

//            public async Task<bool> RemoveGroup(string id, CancellationToken cancellationToken)
//            {
//                var processId = Guid.NewGuid().ToString();
//                try
//                {
//                    var jObj = new JObject { { "id", id } };
//                    _eventPublisher.Publish(new RemoveGroupReceived(Guid.NewGuid().ToString(), processId, jObj.ToString(), 0));
//                    var result = await _groupRepository.Delete(id, cancellationToken).ConfigureAwait(false);
//                    _eventPublisher.Publish(new RemoveGroupFinished(Guid.NewGuid().ToString(), processId, JsonConvert.SerializeObject(result).ToString(), 1));
//                    return result;
//                }
//                catch (Exception ex)
//                {
//                    _eventPublisher.Publish(new ScimErrorReceived(Guid.NewGuid().ToString(), processId, ex.Message, 1));
//                    throw;
//                }
//            }

//            public async Task<GroupResource> UpdateGroup(string id, GroupResource @group, CancellationToken cancellationToken = default(CancellationToken))
//            {
//                var processId = Guid.NewGuid().ToString();
//                try
//                {
//                    _eventPublisher.Publish(new RemoveGroupReceived(Guid.NewGuid().ToString(), processId, null, 0));
//                    var deleted = await _groupRepository.Delete(id, cancellationToken).ConfigureAwait(false);
//                    if (!deleted)
//                    {
//                        return null;
//                    }
//                    await _groupRepository.Persist(@group, cancellationToken).ConfigureAwait(false);
//                    //var result = await _updateRepresentationAction.Execute(id, group, ScimConstants.SchemaUrns.Group, locationPattern, ScimConstants.ResourceTypes.Group).ConfigureAwait(false);
//                    //_eventPublisher.Publish(new RemoveGroupFinished(Guid.NewGuid().ToString(), processId, JsonConvert.SerializeObject(result).ToString(), 1));
//                    return group; //result;
//                }
//                catch (Exception ex)
//                {
//                    _eventPublisher.Publish(new ScimErrorReceived(Guid.NewGuid().ToString(), processId, ex.Message, 1));
//                    throw;
//                }
//            }

//            public async Task<GroupResource> PatchGroup(string id, IList<PatchOperation> patchOperations, CancellationToken cancellationToken)
//            {
//                var processId = Guid.NewGuid().ToString();
//                try
//                {
//                    _eventPublisher.Publish(new PatchGroupReceived(Guid.NewGuid().ToString(), processId, patchOperations.ToString(), 0));
//                    var group = await _groupRepository.Get(id, cancellationToken).ConfigureAwait(false);
//                    foreach (var patchOperation in patchOperations)
//                    {
//                        switch (patchOperation.Type)
//                        {
//                            case PatchOperations.add:
//                                {
//                                    var property = _properties.FirstOrDefault(x =>
//                                        string.Equals(x.Name, patchOperation.Path, StringComparison.OrdinalIgnoreCase));
//                                    if (property != null && typeof(ICollection).IsAssignableFrom(property.PropertyType))
//                                    {
//                                        var value = JsonConvert.DeserializeObject(patchOperation.Value.ToString(),
//                                            property.PropertyType);
//                                        property.SetValue(group, value);
//                                    }
//                                }
//                                break;
//                            case PatchOperations.remove:
//                                {
//                                    var property = _properties.FirstOrDefault(x =>
//                                        string.Equals(x.Name, patchOperation.Path, StringComparison.OrdinalIgnoreCase));
//                                    if (property != null)
//                                    {
//                                        property.SetValue(group, null);
//                                    }
//                                }
//                                break;
//                            case PatchOperations.replace:
//                                {
//                                    var property = _properties.FirstOrDefault(x =>
//                                        string.Equals(x.Name, patchOperation.Path, StringComparison.OrdinalIgnoreCase));
//                                    if (property != null)
//                                    {
//                                        var value = JsonConvert.DeserializeObject(patchOperation.Value.ToString(),
//                                            property.PropertyType);
//                                        property.SetValue(group, value);
//                                    }
//                                }
//                                break;
//                            default:
//                                throw new ArgumentOutOfRangeException();
//                        }
//                    }
//                    await _groupRepository.Persist(@group, cancellationToken).ConfigureAwait(false);
//                    //var result = await _patchRepresentationAction.Execute(id, group, ScimConstants.SchemaUrns.Group, locationPattern).ConfigureAwait(false);
//                    //_eventPublisher.Publish(new PatchGroupFinished(Guid.NewGuid().ToString(), processId, JsonConvert.SerializeObject(result).ToString(), 1));
//                    return group; //result;
//                }
//                catch (Exception ex)
//                {
//                    _eventPublisher.Publish(new ScimErrorReceived(Guid.NewGuid().ToString(), processId, ex.Message, 1));
//                    throw;
//                }
//            }

//            public Task<GroupResource[]> SearchGroups(IQueryCollection query, string locationPattern)
//            {
//                var searchParam = _searchParameterParser.ParseQuery(query);

//                return null;
//            }

//            public async Task<GroupResource[]> SearchGroups(JObject jObj, string locationPattern)
//            {
//                var searchParam = _searchParameterParser.ParseJson(jObj);

//                var groups = (await _groupRepository.Get(x => true).ConfigureAwait(false))
//                    .Skip(searchParam.StartIndex)
//                    .Take(searchParam.Count);
//                if (!string.IsNullOrWhiteSpace(searchParam.SortBy))
//                {
//                    var sortProp =
//                        Expression.Lambda<Func<GroupResource, object>>(Expression.Property(Expression.Parameter(typeof(GroupResource), "x"),
//                            searchParam.SortBy));
//                    groups = searchParam.SortOrder == SortOrders.Ascending
//                        ? groups.OrderBy(sortProp.Compile())
//                        : groups.OrderByDescending(sortProp.Compile());
//                }

//                return groups.ToArray();
//            }

//            private class SearchParameterParser
//            {
//                /// <summary>
//                /// Parse the query and return the search parameters.
//                /// </summary>
//                /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
//                /// <param name="query">Query parameters.</param>
//                /// <returns>Search parameters.</returns>
//                public SearchParameter ParseQuery(IQueryCollection query)
//                {
//                    var result = new SearchParameter();
//                    if (query == null)
//                    {
//                        return result;
//                    }

//                    foreach (var key in query.Keys)
//                    {
//                        //TrySetEnum((r) => result.Attributes = r.Select(a => GetFilter(a)), key, ScimConstants.SearchParameterNames.Attributes, query);
//                        //TrySetEnum((r) => result.ExcludedAttributes = r.Select(a => GetFilter(a)), key, ScimConstants.SearchParameterNames.ExcludedAttributes, query);
//                        //TrySetStr((r) => result.Filter = GetFilter(r), key, ScimConstants.SearchParameterNames.Filter, query);
//                        //TrySetStr((r) => result.SortBy = GetFilter(r), key, ScimConstants.SearchParameterNames.SortBy, query);
//                        //TrySetStr((r) => result.SortOrder = GetSortOrder(r), key, ScimConstants.SearchParameterNames.SortOrder, query);
//                        //TrySetInt((r) => result.StartIndex = r <= 0 ? result.StartIndex : r, key, ScimConstants.SearchParameterNames.StartIndex, query);
//                        //TrySetInt((r) => result.Count = r <= 0 ? result.Count : r, key, ScimConstants.SearchParameterNames.Count, query);
//                    }

//                    return result;
//                }

//                /// <summary>
//                /// Parse the json and return the search parameters.
//                /// </summary>
//                /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
//                /// <param name="json">JSON that will be parsed.</param>
//                /// <returns>Search parameters.</returns>
//                public SearchParameter ParseJson(JObject json)
//                {
//                    var result = new SearchParameter();
//                    if (json == null)
//                    {
//                        return result;
//                    }
//                    if (TryGetToken(json, ScimConstants.SearchParameterNames.Attributes, out JArray jArr))
//                    {
//                        result.Attributes = jArr.Select(x => x.ToString()).ToArray(); //(jArr.Values<string>()).Select(a => GetFilter(a));
//                    }

//                    if (TryGetToken(json, ScimConstants.SearchParameterNames.ExcludedAttributes, out jArr))
//                    {
//                        result.ExcludedAttributes = Array.Empty<string>(); //(jArr.Values<string>()).Select(a => GetFilter(a));
//                    }

//                    if (TryGetToken(json, ScimConstants.SearchParameterNames.Filter, out JValue jVal))
//                    {
//                        result.Filter = jVal.Value<string>();  //GetFilter(jVal.Value<string>());
//                    }

//                    if (TryGetToken(json, ScimConstants.SearchParameterNames.SortBy, out jVal))
//                    {
//                        result.SortBy = jVal.Value<string>(); //GetFilter(jVal.Value<string>());
//                    }

//                    if (TryGetToken(json, ScimConstants.SearchParameterNames.SortOrder, out jVal))
//                    {
//                        result.SortOrder = GetSortOrder(jVal.Value<string>());
//                    }

//                    if (TryGetToken(json, ScimConstants.SearchParameterNames.StartIndex, out jVal))
//                    {
//                        var i = GetInt(jVal.Value<string>(), ScimConstants.SearchParameterNames.StartIndex);
//                        result.StartIndex = i <= 0 ? result.StartIndex : i;
//                    }

//                    if (TryGetToken(json, ScimConstants.SearchParameterNames.Count, out jVal))
//                    {
//                        var i = GetInt(jVal.Value<string>(), ScimConstants.SearchParameterNames.Count);
//                        result.Count = i <= 0 ? result.Count : i;
//                    }

//                    return result;
//                }

//                //private Filter GetFilter(string value)
//                //{
//                //    var filter = _filterParser.Parse(value);
//                //    if (filter == null)
//                //    {
//                //        throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, ScimConstants.SearchParameterNames.Filter));
//                //    }

//                //    return filter;
//                //}

//                //private static void TrySetEnum(Action<IEnumerable<string>> setParameterCallback, string key, string value, IQueryCollection query)
//                //{
//                //    if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
//                //    {
//                //        setParameterCallback(query[key].ToArray());
//                //    }
//                //}

//                //private static void TrySetStr(Action<string> setParameterCallback, string key, string value, IQueryCollection query)
//                //{
//                //    if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
//                //    {
//                //        setParameterCallback(query[key].ToString());
//                //    }
//                //}

//                //private static void TrySetInt(Action<int> setParameterCallback, string key, string value, IQueryCollection query)
//                //{
//                //    if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
//                //    {
//                //        int number = GetInt(query[key].ToString(), key);
//                //        setParameterCallback(number);
//                //    }
//                //}

//                private static bool TryGetToken<T>(JObject jObj, string key, out T result) where T : class
//                {
//                    var token = jObj.SelectToken(key);
//                    if (token == null)
//                    {
//                        result = null;
//                        return false;
//                    }

//                    result = token as T;
//                    return result != null;
//                }

//                private static SortOrders GetSortOrder(string value)
//                {
//                    SortOrders sortOrder;
//                    if (value.Equals(ScimConstants.SortOrderNames.Ascending, StringComparison.CurrentCultureIgnoreCase))
//                    {
//                        sortOrder = SortOrders.Ascending;
//                    }
//                    else if (value.Equals(ScimConstants.SortOrderNames.Descending, StringComparison.CurrentCultureIgnoreCase))
//                    {
//                        sortOrder = SortOrders.Descending;
//                    }
//                    else
//                    {
//                        throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, ScimConstants.SearchParameterNames.SortOrder));
//                    }

//                    return sortOrder;
//                }

//                private static int GetInt(string value, string name)
//                {
//                    if (!int.TryParse(value, out int number))
//                    {
//                        throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, name));
//                    }

//                    return number;
//                }
//            }
//        }
//    }
//}
