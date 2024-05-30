namespace dotauth.tool;

using CommandLine;

[Verb("refresh", false, ["r"], HelpText = "Refreshed the access token using a refresh token.", Hidden = false)]
public class RefreshArgs
{
    [Option('t', "token", Required = true, HelpText = "The refresh token to get the refreshed access token from.")]
    public string RefreshToken { get; set; } = null!;
}
