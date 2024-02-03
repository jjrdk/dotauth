namespace DotAuth.Uma.Tests;

public class UmaTicketInfoTests
{
    [Fact]
    public void CanParseResponseHeader()
    {
        var header = "WWW-Authenticate:UMA as_uri=\"https://localhost/\", ticket=\"ticket\"";
        var result = UmaTicketInfo.TryParse(header, out var info);

        Assert.True(result);
        Assert.Equal("https://localhost/", info!.UmaAuthority);
        Assert.Equal("ticket", info!.TicketId);
    }
}
