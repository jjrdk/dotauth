namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleIdentityServer.Core.Common.Models;

    public interface IRepresentationResponseParser
    {
        /// <summary>
        /// Parse the representation into JSON and returns the result.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when parameters are null empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when error occured during the parsing.</exception>
        /// <param name="representation">Representation that will be parsed.</param>
        /// <param name="location">Location of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <param name="operationType">Type of operation.</param>
        /// <returns>JSON representation</returns>
        Task<Response> Parse(
            Representation representation, 
            string location,
            string schemaId, 
            OperationTypes operationType);

        /// <summary>
        /// Filter the representations and return the result.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when representations are null.</exception>
        /// <param name="representations">Representations to filter.</param>
        /// <param name="searchParameter">Search parameters.</param>
        /// <param name="totalNumbers">Total numbers</param>
        /// <returns>Filtered response</returns>
        FilterResult Filter(IEnumerable<Representation> representations, SearchParameter searchParameter, int totalNumbers);
    }
}