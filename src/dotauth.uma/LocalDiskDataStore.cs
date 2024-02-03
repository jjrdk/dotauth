namespace DotAuth.Uma;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the local file data store.
/// </summary>
public class LocalDiskDataStore : IDataStore
{
    private readonly string _root;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalDiskDataStore"/> class.
    /// </summary>
    /// <param name="root">The root file path of the store.</param>
    public LocalDiskDataStore(string root)
    {
        _root = root;
    }

    /// <inheritdoc />
    public async Task<Uri> Save(DataContent file, CancellationToken cancellationToken = default)
    {
        if (file.FileName.Contains($"..{Path.PathSeparator}"))
        {
            throw new ArgumentException("Invalid file name", nameof(file));
        }

        var filepath = Path.Combine(file.Owner, file.FileName.Replace(Path.PathSeparator, '_'));
        var storePath = Path.Combine(_root, filepath);
        var fullPath = Path.GetFullPath(storePath);
        if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        }
        var builder = new UriBuilder(Uri.UriSchemeFile, filepath);
        await using var writer = File.Create(fullPath);
        await file.Content.CopyToAsync(writer, cancellationToken).ConfigureAwait(false);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        return builder.Uri;
    }

    /// <inheritdoc />
    public Task<Stream?> Get(Uri location, CancellationToken cancellationToken = default)
    {
        if (location.Scheme != Uri.UriSchemeFile)
        {
            return Task.FromResult<Stream?>(null);
        }

        var filepath = Path.TrimEndingDirectorySeparator(location.ToString().Replace($"{Uri.UriSchemeFile}://", ""));
        var storagePath = Path.Combine(_root, filepath);
        var stream = File.OpenRead(storagePath);
        return Task.FromResult<Stream?>(stream);
    }
}
