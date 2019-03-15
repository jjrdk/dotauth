namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the default marten registry for stored SimpleAuth types.
    /// </summary>
    /// <seealso cref="MartenRegistry" />
    public class SimpleAuthRegistry : MartenRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAuthRegistry"/> class.
        /// </summary>
        public SimpleAuthRegistry()
        {
            For<Scope>().Identity(x => x.Name).GinIndexJsonData();
            For<Filter>().Identity(x => x.Name).GinIndexJsonData();
            For<ResourceOwner>().Identity(x => x.Subject).GinIndexJsonData();
            For<Consent>().GinIndexJsonData();
            For<Policy>().GinIndexJsonData();
            For<Client>().Identity(x => x.ClientId).GinIndexJsonData();
            For<ResourceSet>().GinIndexJsonData();
            For<Ticket>().GinIndexJsonData();
            For<AuthorizationCode>().Identity(x => x.Code).GinIndexJsonData();
            For<ConfirmationCode>().Identity(x => x.Value).GinIndexJsonData();
            For<GrantedToken>().GinIndexJsonData();
        }
    }
}