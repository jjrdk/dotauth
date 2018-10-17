namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    public interface IBulkRequestParser
    {
        /// <summary>
        /// Parse the JSON and return the bulk request.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when a REQUIRED parameter is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the 'baseUrlPattern' parameter  is not correctly formatted.</exception>
        /// <param name="jObj">JSON that will be parsed.</param>
        /// <param name="baseUrlPattern">Base url pattern.</param>
        /// <returns>Bulk request or null.</returns>
        Task<BulkRequestResponse> Parse(JObject jObj, string baseUrlPattern);
    }
}