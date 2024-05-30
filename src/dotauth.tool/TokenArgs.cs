namespace dotauth.tool;

using CommandLine;

[Verb("token", true, ["t"], HelpText = "Authenticate and get tokens", Hidden = false)]

public class TokenArgs
{
    [Option(
        'r',
        "response-mode",
        Default = "query",
        Required = false,
        HelpText = "Sets the client id for the login request.")]
    public string ResponseMode { get; set; } = null!;

    [Option(
        's',
        "scopes",
        Default = new[] { "openid" },
        Separator = ',',
        HelpText = "Sets the scopes to request token for.")]
    public IEnumerable<string> Scopes { get; set; } = null!;
}
