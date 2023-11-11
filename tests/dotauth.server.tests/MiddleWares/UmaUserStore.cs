namespace DotAuth.Server.Tests.MiddleWares;

public sealed class UmaUserStore
{
    private static readonly UmaUserStore InnerInstance = new();
    private const string DefaultClient = "client";

    private UmaUserStore()
    {
        ClientId = DefaultClient;
    }

    public static UmaUserStore Instance()
    {
        return InnerInstance;
    }

    public bool IsInactive { get; set; }
    public string ClientId { get; set; }
}
