namespace SimpleAuth
{
    public static class JwtSecurityTokenExtensions
    {
        public static bool IsJweToken(this string token)
        {
            return token.Split('.').Length == 5;
        }

        public static bool IsJwsToken(this string token)
        {
            return token.Split('.').Length == 3;
        }
    }
}