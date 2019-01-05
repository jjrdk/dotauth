//namespace SimpleAuth.JwtToken
//{
//    using System.IdentityModel.Tokens.Jwt;
//    using System.Threading.Tasks;

//    public interface IJwtParser
//    {
//        bool IsJweToken(string jwe);
//        bool IsJwsToken(string jws);
//        Task<JwtSecurityToken> DecryptAsync(string jwe);
//        Task<string> DecryptAsync(string jwe, string clientId);
//        Task<string> DecryptWithPasswordAsync(string jwe, string clientId, string password);
//        Task<JwtPayload> UnSignAsync(string jws);
//        Task<JwtPayload> UnSignAsync(string jws, string clientId);
//    }
//}