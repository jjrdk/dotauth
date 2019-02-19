namespace SimpleAuth.Shared.Repositories
{
    using Models;
    using Results;
    using SimpleAuth.Shared.Requests;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the scope store interface.
    /// </summary>
    public interface IScopeStore
    {
        /// <summary>
        /// Searches the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<SearchScopeResult> Search(SearchScopesRequest parameter, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Scope> Get(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Searches the by names.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="names">The names.</param>
        /// <returns></returns>
        Task<Scope[]> SearchByNames(CancellationToken cancellationToken, params string[] names);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Scope[]> GetAll(CancellationToken cancellationToken);
    }
}