namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using SimpleIdentityServer.Core.Common.DTOs;
    using SimpleIdentityServer.Core.Common.Models;

    public interface IPatchRequestParser
    {
        /// <summary>
        /// Parse the object and returns the PatchOperation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the parameter is null</exception>
        /// <param name="jObj">Json that will be parsed.</param>
        /// <param name="errorResponse">Error response.</param>
        /// <returns>Patch operation or null</returns>
        IEnumerable<PatchOperation> Parse(JObject jObj, out ScimErrorResponse errorResponse);
    }
}