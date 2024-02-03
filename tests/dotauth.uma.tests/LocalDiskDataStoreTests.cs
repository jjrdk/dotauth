namespace DotAuth.Uma.Tests;

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class LocalDiskDataStoreTests
{
    private readonly LocalDiskDataStore _store = new(".");

    [Fact]
    public async Task CanStoreContentAndReadBack()
    {
        using var ms = new MemoryStream("Hello, World"u8.ToArray());
        var content = new DataContent
        {
            Content = ms,
            Encoding = Encoding.UTF8,
            FileName = "hello.txt",
            MimeType = "text/plain",
            Owner = "tester",
            Size = ms.Length
        };
        var location = await _store.Save(content, CancellationToken.None);
        var readBack = await _store.Get(location, CancellationToken.None);
        var buffer = new byte[12];
        _ = await readBack!.ReadAsync(buffer, 0, 12);

        ms.Position = 0;
        Assert.Equal(ms.ToArray(), buffer);
    }
}
