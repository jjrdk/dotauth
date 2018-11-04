namespace SimpleIdentityServer.Core.Validators
{
    using System.Threading.Tasks;
    using Shared.Models;

    public interface IGrantedTokenValidator
    {
        Task<GrantedTokenValidationResult> CheckAccessTokenAsync(string accessToken);
        Task<GrantedTokenValidationResult> CheckRefreshTokenAsync(string refreshToken);
        GrantedTokenValidationResult CheckGrantedToken(GrantedToken token);
    }
}