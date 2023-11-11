namespace DotAuth.Server.Tests.MiddleWares;

using System;

public sealed class UserStore
{
    private static readonly UserStore InnerInstance = new ();
    private static readonly string DefaultSubject = "administrator";

    private UserStore()
    {
        Subject = DefaultSubject;
    }

    public static UserStore Instance()
    {
        return InnerInstance;
    }

    public bool IsInactive { get; set; }
    public string Subject { get; set; }
    public DateTimeOffset? AuthenticationOffset { get; set; }
}
