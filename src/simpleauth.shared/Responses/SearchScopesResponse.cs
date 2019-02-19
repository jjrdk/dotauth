namespace SimpleAuth.Shared.Responses
{
    using System.Runtime.Serialization;
    using SimpleAuth.Shared.Models;

    [DataContract]
    public class SearchScopesResponse : GenericResult<Scope>
    {
    }
}
