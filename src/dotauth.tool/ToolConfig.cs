namespace dotauth.tool;

using DotAuth.Shared.Models;

public class ToolConfig
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string Authority { get; set; } = "https://accounts.google.com";
    public string RedirectUrl { get; set; } = "http://localhost:65001/signin";
    public string CodeChallengeMethod { get; set; } = CodeChallengeMethods.S256;

      /// <summary>
      /// The token endpoint authentication method the tool uses to authenticate to the
      /// authorization server (e.g. <c>private_key_jwt</c>, <c>client_secret_jwt</c>).
      /// Defaults to <c>client_secret_basic</c>.
      /// </summary>
    public string TokenEndPointAuthMethod { get; set; } = TokenEndPointAuthenticationMethods.ClientSecretBasic;
}