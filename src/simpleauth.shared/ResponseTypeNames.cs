namespace SimpleAuth.Shared
{
    /// <summary>
    /// Defines the response type names.
    /// </summary>
    public static class ResponseTypeNames
    {
        /// <summary>
        /// Code
        /// </summary>
        public const string Code = "code";

        /// <summary>
        /// Token
        /// </summary>
        public const string Token = "token";

        /// <summary>
        /// ID Token
        /// </summary>
        public const string IdToken = "id_token";

        /// <summary>
        /// All
        /// </summary>
        public static readonly string[] All = { Code, IdToken, Token };
    }
}