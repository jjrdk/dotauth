namespace SimpleAuth.WebSite.User.Actions
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IUpdateUserClaimsOperation
    {
        Task<bool> Execute(string subject, IEnumerable<Claim> claims);
    }
}