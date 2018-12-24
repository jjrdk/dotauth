namespace SimpleAuth.Shared
{
    /// <summary>
    /// Algorithms used to create JWS
    /// Links to documentation : https://tools.ietf.org/html/rfc7518#page-6
    /// </summary>
    public enum JwsAlg
    {
        HS256,
        HS384,
        HS512,
        RS256,
        RS384,
        RS512,
        ES256,
        ES384,
        ES512,
        PS256,
        PS384,
        PS512,
        none
    }
}