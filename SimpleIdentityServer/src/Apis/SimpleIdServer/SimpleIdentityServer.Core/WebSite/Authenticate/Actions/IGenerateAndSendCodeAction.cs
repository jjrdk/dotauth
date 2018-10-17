namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    using System.Threading.Tasks;

    public interface IGenerateAndSendCodeAction
    {
        Task<string> ExecuteAsync(string subject);
    }
}