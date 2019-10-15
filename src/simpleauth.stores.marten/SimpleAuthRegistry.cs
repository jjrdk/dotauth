namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using Microsoft.IdentityModel.Tokens;
    using NpgsqlTypes;
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
                .Duplicate(x => x.IsDisplayedInConsent, configure: idx => { idx.IsConcurrent = true; }, dbType: NpgsqlDbType.Boolean)
                .Duplicate(x => x.Type, "varchar(15)", configure: idx => { idx.IsConcurrent = true; })
                .GinIndexJsonData();
            For<Filter>().Identity(x => x.Name).GinIndexJsonData();
            For<ResourceOwner>()
                .Identity(x => x.Subject)
                .Index(x => x.Claims, configure: idx => { idx.IsConcurrent = true; })
                .Index(x => x.ExternalLogins, configure: idx => { idx.IsConcurrent = true; })
                .GinIndexJsonData();
            For<Consent>()
                .Duplicate(x => x.ResourceOwner.Subject)
                .GinIndexJsonData();
            For<Policy>()
                .Duplicate(x => x.Id)
                .Duplicate(x => x.ResourceSetIds)
                .GinIndexJsonData();
            For<Client>()
                .Identity(x => x.ClientId)
                .Index(x => x.AllowedScopes, configure: idx => { idx.IsConcurrent = true; })
                .Index(x => x.GrantTypes, configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.IdTokenEncryptedResponseAlg, "varchar(10)", configure: idx => { idx.IsConcurrent = true; })
                .Index(x => x.ResponseTypes, configure: idx => { idx.IsConcurrent = true; })
                .Index(x => x.Claims, configure: idx => { idx.IsConcurrent = true; })
                .GinIndexJsonData();
            For<ResourceSetModel>()
                .Duplicate(x => x.Name)
                .Duplicate(x => x.Type)
                .GinIndexJsonData();
            For<Ticket>().GinIndexJsonData();
            For<AuthorizationCode>()
                .Identity(x => x.Code)
                .Duplicate(x => x.ClientId, configure: idx => { idx.IsConcurrent = true; })
                .GinIndexJsonData();
            For<ConfirmationCode>().Identity(x => x.Value).GinIndexJsonData();
            For<GrantedToken>()
                .Duplicate(x => x.Scope, configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.AccessToken, configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.ClientId, configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.CreateDateTime, configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.ExpiresIn, configure: idx => { idx.IsConcurrent = true; }, dbType: NpgsqlDbType.Integer)
                .Duplicate(x => x.IdToken, configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.ParentTokenId, configure: idx => { idx.IsConcurrent = true; }, dbType: NpgsqlDbType.Uuid)
                .Duplicate(x => x.RefreshToken, configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.TokenType, "char(10)", configure: idx => { idx.IsConcurrent = true; })
                .GinIndexJsonData();
            For<JsonWebKey>()
                .Identity(x => x.Kid)
                .Duplicate(x => x.Alg, pgType: "char(20)", configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.Use, "char(3)", configure: idx => { idx.IsConcurrent = true; })
                .Duplicate(x => x.HasPrivateKey, configure: idx => { idx.IsConcurrent = true; }, dbType: NpgsqlDbType.Boolean)
                .Index(x => x.KeyOps, configure: idx => { idx.IsConcurrent = true; })
                .GinIndexJsonData();
        }
    }
}
