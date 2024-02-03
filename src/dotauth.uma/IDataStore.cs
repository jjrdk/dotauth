namespace DotAuth.Uma;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the data store interface.
/// </summary>
public interface IDataStore
{
    /// <summary>
    /// Saves the content of the <see cref="DataContent"/>.
    /// </summary>
    /// <param name="file">The uploaded file info.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Uri> Save(DataContent file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the content at the location.
    /// </summary>
    /// <param name="location">The data location.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream?> Get(Uri location, CancellationToken cancellationToken = default);
}