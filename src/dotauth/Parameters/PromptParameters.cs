namespace DotAuth.Parameters;

internal static class PromptParameters
{
    public const string None = "none";
    public const string Login = "login";
    public const string Consent = "consent";
    public const string SelectAccount = "select_account";

    public static string[] All() => [None, Login, Consent, SelectAccount];
}
