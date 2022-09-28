// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Extensions;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

internal static class UriExtensions
{
    /// <summary>
    /// Add the given parameter in the query string.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static Uri AddParametersInQuery(this Uri uri, RouteValueDictionary dic)
    {
        var uriBuilder = new UriBuilder(uri);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uriBuilder.Query);
        foreach (var (key, value) in dic)
        {
            query[key] = value!.ToString();
        }

        uriBuilder.Query = ConcatQueryStrings(query);
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Add the given parameters in the fragment.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    public static Uri AddParametersInFragment(this Uri uri, RouteValueDictionary dic)
    {
        var uriBuilder = new UriBuilder(uri);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uriBuilder.Query);
        foreach (var (key, value) in dic)
        {
            query[key] = value!.ToString();
        }

        uriBuilder.Fragment = ConcatQueryStrings(query);
        return uriBuilder.Uri;
    }

    private static string ConcatQueryStrings(IDictionary<string, StringValues> queryStrings)
    {
        var lst = queryStrings.Select(keyValuePair => $"{keyValuePair.Key}={keyValuePair.Value}");

        return string.Join("&", lst);
    }
}