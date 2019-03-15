namespace SimpleAuth.Stores.Marten.AcceptanceTests
{
    using global::Marten;
    using SimpleAuth.Shared.Models;

    public class SimpleAuthRegistry : MartenRegistry
    {
        public SimpleAuthRegistry()
        {
            For<Scope>().Identity(x => x.Name).GinIndexJsonData();
            For<Filter>().Identity(x => x.Name).GinIndexJsonData();
            For<ResourceOwner>().Identity(x => x.Subject).GinIndexJsonData();
            For<Consent>().GinIndexJsonData();
            For<Policy>().GinIndexJsonData();
            For<Client>().Identity(x => x.ClientId).GinIndexJsonData();
            For<GrantedToken>().GinIndexJsonData();
        }
    }
}