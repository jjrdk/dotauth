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
            For<Consent>()
                .Index(x => x.ResourceOwner.Subject)
                .GinIndexJsonData();
            For<Policy>()
                .Duplicate(x => x.Id)
                .Duplicate(x => x.ResourceSetIds)
                .GinIndexJsonData();
            For<Client>()
                .Identity(x => x.ClientId)
                .Index(x => x.AllowedScopes)
                .Index(x => x.GrantTypes)
                .Index(x => x.IdTokenEncryptedResponseAlg)
                .Index(x => x.ResponseTypes)
                .Index(x => x.Claims)
                .GinIndexJsonData();
            For<ResourceSet>()
                .Duplicate(x => x.Name)
                .Duplicate(x => x.Type)
                .GinIndexJsonData();
            For<Ticket>().GinIndexJsonData();
            For<AuthorizationCode>()
                .Identity(x => x.Code)
                .Duplicate(x => x.ClientId)
                .GinIndexJsonData();
            For<ConfirmationCode>()
                .Identity(x => x.Value)
                .GinIndexJsonData();
            For<GrantedToken>()
                .Duplicate(x => x.Scope)
                .Duplicate(x => x.AccessToken)
                .Duplicate(x => x.RefreshToken)
                .GinIndexJsonData();
            For<JsonWebKey>()
                .Identity(x => x.Kid)
                .Duplicate(x => x.Alg)
                .Duplicate(x => x.Use)
                .Duplicate(x => x.HasPrivateKey)
                .Index(x => x.KeyOps)
                .GinIndexJsonData();
        }
    }
}
