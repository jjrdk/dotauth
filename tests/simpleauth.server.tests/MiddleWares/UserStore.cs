namespace SimpleAuth.Server.Tests.MiddleWares;

using System;

public sealed class UserStore
{
    private static UserStore _instance;
    private static readonly string DefaultSubject = "administrator";

    private UserStore()
    {
        Subject = DefaultSubject;
    }

    public static UserStore Instance()
    {
        return _instance ??= new UserStore();
    }

    public bool IsInactive { get; set; }
    public string Subject { get; set; }
    public DateTimeOffset? AuthenticationOffset { get; set; }
}