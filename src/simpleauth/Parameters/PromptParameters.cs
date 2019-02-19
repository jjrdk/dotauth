namespace SimpleAuth.Parameters
{
    public static class PromptParameters
    {
        public static readonly string None = "none";
        public static readonly string Login = "login";
        public static readonly string Consent = "consent";
        public static readonly string SelectAccount = "select_account";

        public static string[] All() => new[] {None, Login, Consent, SelectAccount};
    }
}