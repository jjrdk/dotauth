namespace SimpleAuth.Api.Introspection
{
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Parameters;
    using Results;

    public interface IIntrospectionActions
    {
        Task<IntrospectionResult> PostIntrospection(
            IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue, string issuerName);
    }
}