namespace SimpleIdentityServer.Shared
{
    /// <summary>
    /// Algorithms used to create JWE
    /// Link to documentation : https://tools.ietf.org/html/rfc7518#page-12
    /// </summary>
    public enum JweAlg
    {
        RSA1_5,
        RSA_OAEP,
        RSA_OAEP_256,
        A128KW,
        A192KW,
        A256KW,
        DIR,
        ECDH_ES,
        ECDH_ESA_128KW,
        ECDH_ESA_192KW,
        ECDH_ESA_256_KW,
        A128GCMKW,
        A192GCMKW,
        A256GCMKW,
        PBES2_HS256_A128KW,
        PBES2_HS384_A192KW,
        PBES2_HS512_A256KW
    }
}