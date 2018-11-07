namespace SimpleIdentityServer.Core.Jwt
{
    using System.Threading.Tasks;

    public interface IConfirmationCodeStore
    {
        Task<ConfirmationCode> Get(string code);
        Task<bool> Add(ConfirmationCode confirmationCode);
        Task<bool> Remove(string code);
    }
}
