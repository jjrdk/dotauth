namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using Microsoft.IdentityModel.Tokens;
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
            For<Scope>()
                .Identity(x => x.Name)
                .Index(x => x.Name)
                .Index(x => x.IsDisplayedInConsent)
                .Index(x => x.Type)
                .GinIndexJsonData();
            For<Filter>().Identity(x => x.Name).GinIndexJsonData();
            For<ResourceOwner>()
                .Identity(x => x.Subject)
                .Index(x => x.Claims)
                .Index(x => x.ExternalLogins)
                .GinIndexJsonData();
            For<Consent>().GinIndexJsonData();
            For<Policy>().GinIndexJsonData();
            For<Client>()
                .Identity(x => x.ClientId)
                .Index(x => x.AllowedScopes)
                .Index(x => x.GrantTypes)
                .Index(x => x.IdTokenEncryptedResponseAlg)
                .Index(x => x.ResponseTypes)
                .Index(x => x.Claims)
                .GinIndexJsonData();
            For<ResourceSet>().GinIndexJsonData();
            For<Ticket>().GinIndexJsonData();
            For<AuthorizationCode>().Identity(x => x.Code).Index(x => x.ClientId).GinIndexJsonData();
            For<ConfirmationCode>().Identity(x => x.Value).GinIndexJsonData();
            For<GrantedToken>().GinIndexJsonData();
            For<JsonWebKey>()
                .Identity(x => x.Kid)
                .Duplicate(x => x.Use)
                .Duplicate(x => x.HasPrivateKey)
                .Index(x => x.Use)
                .Index(x => x.HasPrivateKey)
                .Index(x => x.KeyOps)
                .GinIndexJsonData();
        }
    }
}
