namespace SimpleAuth.WebSite.Authenticate.Actions
{
    using System.Threading.Tasks;

    public interface IRemoveConfirmationCodeAction
    {
        Task<bool> Execute(string code);
    }
}