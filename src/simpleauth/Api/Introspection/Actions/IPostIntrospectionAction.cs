namespace SimpleAuth.Api.Introspection.Actions
{
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IPostIntrospectionAction
    {
        Task<IntrospectionResult> Execute(IntrospectionParameter introspectionParameter, AuthenticationHeaderValue authenticationHeaderValue, string issuerName);
    }
}