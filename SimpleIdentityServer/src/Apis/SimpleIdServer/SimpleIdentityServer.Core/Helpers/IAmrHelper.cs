namespace SimpleIdentityServer.Core.Helpers
{
    using System.Collections.Generic;

    public interface IAmrHelper
    {
        string GetAmr(IEnumerable<string> currentAmrs, IEnumerable<string> exceptedAmrs);
    }
}