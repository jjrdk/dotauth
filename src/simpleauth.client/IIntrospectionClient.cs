namespace SimpleAuth.Client
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the introspection client interface.
    /// </summary>
    public interface IIntrospectionClient
    {
        /// <summary>
        /// Executes the specified introspection request.
        /// </summary>
        /// <param name="introspectionRequest">The introspection request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<Option<UmaIntrospectionResponse>> Introspect(
            IntrospectionRequest introspectionRequest,
            CancellationToken cancellationToken = default);
    }
}