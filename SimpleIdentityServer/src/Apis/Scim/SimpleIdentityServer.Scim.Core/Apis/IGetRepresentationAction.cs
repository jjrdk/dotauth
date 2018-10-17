namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System;
    using System.Threading.Tasks;
    using Results;

    public interface IGetRepresentationAction
    {
        /// <summary>
        /// Get the representation.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when a parameter is null or empty</exception>
        /// <param name="identifier">Identifier of the representation.</param>
        /// <param name="locationPattern">Location pattern of the representation.</param>
        /// <param name="schemaId">Identifier of the schema.</param>
        /// <returns>Representation or null if it doesn't exist.</returns>
        Task<ApiActionResult> Execute(string identifier, string locationPattern, string schemaId);
    }
}