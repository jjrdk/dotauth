namespace SimpleAuth.Client.Builders
{
    public class PKCE
    {
        public string CodeVerifier { get; set; }
        public string CodeChallenge { get; set; }
    }
}