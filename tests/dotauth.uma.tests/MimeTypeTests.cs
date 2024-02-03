namespace DotAuth.Uma.Tests;

using System.Threading.Tasks;

public class MimeTypeTests
{
    [Theory]
    [InlineData("text/plain", "plain text")]
    [InlineData("image/png", "Picture")]
    public async Task CanFindResourceTypeFromMimeType(string mimetype, string resourceType)
    {
        var lookup = new FixedResourceTypeLookup();
        var found = await lookup.GetResourceType(mimetype);

        Assert.Equal(resourceType, found);
    }
}
