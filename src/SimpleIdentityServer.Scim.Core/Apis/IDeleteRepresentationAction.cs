namespace SimpleIdentityServer.Scim.Core.Apis
{
    using System.Threading.Tasks;
    using Results;

    public interface IDeleteRepresentationAction
    {
        /// <summary>
        /// Remove the representation.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when the id is null or empty</exception>
        /// <param name="id">Representation's id</param>
        /// <returns>StatusCode with the content.</returns>
        Task<ApiActionResult> Execute(
            string id);
    }
}