namespace dotauth.tool;

using DotAuth.Shared.Models;

internal class ToolConfig
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string Authority { get; set; } = "https://accounts.google.com";
    public string RedirectUrl { get; set; } = "http://localhost:65001/signin";
    public string CodeChallengeMethod { get; set; } = CodeChallengeMethods.S256;
}