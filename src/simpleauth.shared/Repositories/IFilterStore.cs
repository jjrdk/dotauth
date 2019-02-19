namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using AccountFiltering;

    /// <summary>
    /// Defines the filter store interface.
    /// </summary>
    public interface IFilterStore
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Filter[]> GetAll(CancellationToken cancellationToken);
    }
}
