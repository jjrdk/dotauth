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

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Errors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using SimpleIdentityServer.Core.Common;

    internal class SearchParameterParser : ISearchParameterParser
    {
        private readonly IFilterParser _filterParser;

        public SearchParameterParser(IFilterParser filterParser)
        {
            _filterParser = filterParser;
        }

        /// <summary>
        /// Parse the query and return the search parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
        /// <param name="query">Query parameters.</param>
        /// <returns>Search parameters.</returns>
        public SearchParameter ParseQuery(IQueryCollection query)
        {
            var result = new SearchParameter();
            if (query == null)
            {
                return result;
            }

            foreach(var key in query.Keys)
            {
                TrySetEnum((r) => result.Attributes = r.Select(a => GetFilter(a)), key, ScimConstants.SearchParameterNames.Attributes, query);
                TrySetEnum((r) => result.ExcludedAttributes = r.Select(a => GetFilter(a)), key, ScimConstants.SearchParameterNames.ExcludedAttributes, query);
                TrySetStr((r) => result.Filter = GetFilter(r), key, ScimConstants.SearchParameterNames.Filter, query);
                TrySetStr((r) => result.SortBy = GetFilter(r), key, ScimConstants.SearchParameterNames.SortBy, query);
                TrySetStr((r) => result.SortOrder = GetSortOrder(r), key, ScimConstants.SearchParameterNames.SortOrder, query);
                TrySetInt((r) => result.StartIndex = r <= 0 ? result.StartIndex : r, key, ScimConstants.SearchParameterNames.StartIndex, query);
                TrySetInt((r) => result.Count = r <= 0 ? result.Count : r, key, ScimConstants.SearchParameterNames.Count, query);
            }

            return result;
        }

        /// <summary>
        /// Parse the json and return the search parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
        /// <param name="json">JSON that will be parsed.</param>
        /// <returns>Search parameters.</returns>
        public SearchParameter ParseJson(JObject json)
        {
            var result = new SearchParameter();
            if (json == null)
            {
                return result;
            }

            JArray jArr;
            JValue jVal;
            if (TryGetToken(json, ScimConstants.SearchParameterNames.Attributes, out jArr))
            {
                result.Attributes = (jArr.Values<string>()).Select(a => GetFilter(a));
            }

            if (TryGetToken(json, ScimConstants.SearchParameterNames.ExcludedAttributes, out jArr))
            {
                result.ExcludedAttributes = (jArr.Values<string>()).Select(a => GetFilter(a));
            }

            if (TryGetToken(json, ScimConstants.SearchParameterNames.Filter, out jVal))
            {
                result.Filter = GetFilter(jVal.Value<string>());
            }

            if (TryGetToken(json, ScimConstants.SearchParameterNames.SortBy, out jVal))
            {
                result.SortBy = GetFilter(jVal.Value<string>());
            }

            if (TryGetToken(json, ScimConstants.SearchParameterNames.SortOrder, out jVal))
            {
                result.SortOrder = GetSortOrder(jVal.Value<string>());
            }

            if (TryGetToken(json, ScimConstants.SearchParameterNames.StartIndex, out jVal))
            {
                var i = GetInt(jVal.Value<string>(), ScimConstants.SearchParameterNames.StartIndex);
                result.StartIndex = i <= 0 ? result.StartIndex : i;
            }

            if (TryGetToken(json, ScimConstants.SearchParameterNames.Count, out jVal))
            {
                var i = GetInt(jVal.Value<string>(), ScimConstants.SearchParameterNames.Count);
                result.Count = i <= 0 ? result.Count : i;
            }

            return result;
        }

        private Filter GetFilter(string value)
        {
            var filter = _filterParser.Parse(value);
            if (filter == null)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, ScimConstants.SearchParameterNames.Filter));
            }

            return filter;
        }

        private static void TrySetEnum(Action<IEnumerable<string>> setParameterCallback, string key, string value, IQueryCollection query)
        {
            if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                setParameterCallback(query[key].ToArray());
            }
        }

        private static void TrySetStr(Action<string> setParameterCallback, string key, string value, IQueryCollection query)
        {
            if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                setParameterCallback(query[key].ToString());
            }
        }

        private static void TrySetInt(Action<int> setParameterCallback, string key, string value, IQueryCollection query)
        {
            if (key.Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                int number = GetInt(query[key].ToString(), key);
                setParameterCallback(number);
            }
        }

        private static bool TryGetToken<T>(JObject jObj, string key, out T result) where T: class
        {
            var token = jObj.SelectToken(key);
            if (token == null)
            {
                result = null;
                return false;
            }

            result = token as T;
            return result != null;
        }

        private static SortOrders GetSortOrder(string value)
        {
            SortOrders sortOrder;
            if (value.Equals(ScimConstants.SortOrderNames.Ascending, StringComparison.CurrentCultureIgnoreCase))
            {
                sortOrder = SortOrders.Ascending;
            }
            else if (value.Equals(ScimConstants.SortOrderNames.Descending, StringComparison.CurrentCultureIgnoreCase))
            {
                sortOrder = SortOrders.Descending;
            }
            else
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, ScimConstants.SearchParameterNames.SortOrder));
            }

            return sortOrder;
        }

        private static int GetInt(string value, string name)
        {
            int number;
            if (!int.TryParse(value, out number))
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.TheParameterIsNotValid, name));
            }

            return number;
        }
    }
}
