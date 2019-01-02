namespace SimpleAuth.Shared
{
    public static class ResponseTypeNames
    {
        public const string Code = "code";
        public const string Token = "token";
        public const string IdToken = "id_token";
        public static readonly string[] All = new[] { Code, IdToken, Token };

        public static string Build(params string[] responseType)
        {
            return responseType == null
            ? string.Empty
            : string.Join(" ", responseType);
        }
    }
}