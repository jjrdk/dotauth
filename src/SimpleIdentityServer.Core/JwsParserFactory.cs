namespace SimpleAuth
{
    using Signature;

    public class JwsParserFactory
    {
        public IJwsParser BuildJwsParser()
        {
            var createJwsSignature = new CreateJwsSignature();
            return new JwsParser(createJwsSignature);
        }
    }
}
