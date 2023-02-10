namespace dotauth.tool;

using CommandLine;
using DotAuth.Shared.Models;

[Verb(
    "configure",
    false,
    null,
    HelpText = "Configures the tool with the provided values as defaults for later use.",
    Hidden = false)]
public class ConfigureArgs
{
    [Option(
        'i',
        "client-id",
        Required = true,
        Hidden = false,
        HelpText = "Sets the client id for the tool client (required).")]
    public string ClientId { get; set; } = null!;

    [Option(
        's',
        "client-secret",
        Required = true,
        Hidden = false,
        HelpText = "Sets the client secret for the tool client (required).")]
    public string ClientSecret { get; set; } = null!;

    [Option(
        'a',
        "authority",
        Required = true,
        Hidden = false,
        HelpText = "Sets the issuing authority for the tool (required).")]
    public string Authority { get; set; } = null!;

    [Option(
        'r',
        "redirect-url",
        Default = "http://localhost:65001/signin",
        Hidden = false,
        Required = false,
        HelpText =
            "Sets the redirect url for the authentication flow (optional). Default value is `http://localhost:65001/signin`")]
    public string? RedirectUrl { get; set; }

    [Option(
        'c',
        "code-challenge-method",
        Default = CodeChallengeMethods.S256,
        Hidden = false,
        Required = false,
        HelpText = "Sets the code challenge method (optional). Default value is S256")]
    public string? CodeChallengeMethod { get; set; }

    [Option(
        'o',
        "output-resulting",
        Default = true,
        Required = false,
        Hidden = false,
        HelpText = "Toggles whether to output the resulting configuration to the console. Default is true.")]
    public bool OutputResulting { get; set; }
}
