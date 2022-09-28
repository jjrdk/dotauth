namespace SimpleAuth.Shared.Models;

/// <summary>
/// Defines the code challenge methods.
/// </summary>
public static class CodeChallengeMethods
{
    /// <summary>
    /// Plain text code challenge.
    /// </summary>
    public const string Plain = "plain";

    /// <summary>
    /// RS256 code challenge.
    /// </summary>
    public const string Rs256 = "RS256";

    /// <summary>
    /// SHA256 code challenge.
    /// </summary>
    public const string S256 = "S256";
}