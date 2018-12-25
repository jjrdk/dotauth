namespace SimpleAuth.Api.Profile.Actions
{
    using System.Threading.Tasks;

    public interface IUnlinkProfileAction
    {
        Task<bool> Execute(string localSubject, string externalSubject);
    }
}