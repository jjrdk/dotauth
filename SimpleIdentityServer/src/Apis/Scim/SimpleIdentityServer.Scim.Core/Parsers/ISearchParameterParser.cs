namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;

    internal interface ISearchParameterParser
    {
        /// <summary>
        /// Parse the query and return the search parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
        /// <param name="query">Query parameters.</param>
        /// <returns>Search parameters.</returns>
        SearchParameter ParseQuery(IQueryCollection query);
        /// <summary>
        /// Parse the json and return the search parameters.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when something goes wrong in the operation.</exception>
        /// <param name="json">JSON that will be parsed.</param>
        /// <returns>Search parameters.</returns>
        SearchParameter ParseJson(JObject obj);
    }
}