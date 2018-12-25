namespace SimpleAuth.WebSite.User.Actions
{
    using System.Threading.Tasks;

    public interface IUpdateUserCredentialsOperation
    {
        Task<bool> Execute(string subject, string newPassword);
    }
}