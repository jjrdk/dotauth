namespace SimpleIdentityServer.Shared
{
    /// <summary>
    /// Encryptions algorithms for JWE : https://tools.ietf.org/html/rfc7518#page-22
    /// </summary>
    public enum JweEnc
    {
        A128CBC_HS256, //AES_128_CBC_HMAC_SHA_256 authenticated encryption using a 256 bit key. : documentation : https://tools.ietf.org/html/draft-ietf-jose-json-web-encryption-40#appendix-B
        A192CBC_HS384,
        A256CBC_HS512,
        A128GCM,
        A192GCM,
        A256GCM
    }
}