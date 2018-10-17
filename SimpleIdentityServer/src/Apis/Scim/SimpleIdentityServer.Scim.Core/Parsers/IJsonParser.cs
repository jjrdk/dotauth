namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System;
    using Newtonsoft.Json.Linq;
    using SimpleIdentityServer.Core.Common.DTOs;

    public interface IJsonParser
    {
        /// <summary>
        /// Parse json and returns the representation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one of the parameter is null.</exception>
        /// <param name="jObj">JSON</param>
        /// <param name="attribute">Schema attribute</param>
        /// <param name="checkStrategy">Strategy used to check the parameters.</param>
        /// <returns>Representation or null.</returns>
        ParseRepresentationAttrResult GetRepresentation(JToken jObj, SchemaAttributeResponse attribute, CheckStrategies checkStrategy);
    }
}