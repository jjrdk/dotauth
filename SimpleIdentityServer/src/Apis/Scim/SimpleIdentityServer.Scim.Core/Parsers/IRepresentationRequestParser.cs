namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    public interface IRepresentationRequestParser
    {
        /// <summary>
        /// Parse JSON and returns its representation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when a parameter is null or empty.</exception>
        /// <param name="jObj">JSON</param>
        /// <param name="schemaId">Schema identifier</param>
        /// <param name="checkStrategy">Strategy used to check the parameters.</param>
        /// <returns>Representation or null.</returns>
        Task<ParseRepresentationResult> Parse(JToken jObj, string schemaId, CheckStrategies checkStrategy);
    }
}