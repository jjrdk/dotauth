namespace SimpleAuth.Shared.Repositories;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the cleanable interface.
/// </summary>
public interface ICleanable
{
    /// <summary>
    /// Cleans the instance of any stale data.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The clean operation as a <see cref="Task"/>.</returns>
    Task Clean(CancellationToken cancellationToken);
}