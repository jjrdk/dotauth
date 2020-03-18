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
            For<Consent>().Identity(x => x.Id).GinIndexJsonData();
            For<Client>().Identity(x => x.ClientId).GinIndexJsonData();
            For<ResourceSet>().Identity(x => x.Id).GinIndexJsonData();
            For<Ticket>().Identity(x => x.Id).GinIndexJsonData();
            For<AuthorizationCode>().Identity(x => x.Code).GinIndexJsonData();
            For<ConfirmationCode>().Identity(x => x.Value).GinIndexJsonData();
            For<GrantedToken>().Identity(x => x.Id).GinIndexJsonData();
        }
    }
}