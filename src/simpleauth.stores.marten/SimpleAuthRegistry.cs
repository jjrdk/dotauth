﻿namespace SimpleAuth.Stores.Marten
{
    using global::Marten;
    using global::Marten.Schema;
    using NpgsqlTypes;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;

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
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Name)
                .Duplicate(x => x.IsDisplayedInConsent, dbType: NpgsqlDbType.Boolean)
                .Duplicate(x => x.Type, "varchar(15)")
                .GinIndexJsonData();
            For<Filter>().Identity(x => x.Name)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Name)
                .GinIndexJsonData();
            For<ResourceOwner>()
                .Identity(x => x.Subject)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Subject)
                .Index(x => x.Claims)
                .Index(x => x.ExternalLogins)
                .GinIndexJsonData();
            For<Consent>()
                .Identity(x => x.Id)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Id)
                .Duplicate(x => x.Subject)
                .GinIndexJsonData();
            For<Client>().Identity(x => x.ClientId).UniqueIndex(UniqueIndexType.Computed, s => s.ClientId)
                //    .Index(x => x.AllowedScopes)
                //    .Index(x => x.GrantTypes)
                    .Duplicate(x => x.IdTokenEncryptedResponseAlg ?? "", "varchar(10)")
                //    .Index(x => x.ResponseTypes)
                //    .Index(x => x.Claims)
                    .GinIndexJsonData();
            For<OwnedResourceSet>()
                .Identity(x => x.Id)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Id)
                .Duplicate(x => x.Owner)
                .Duplicate(x => x.Name)
                .Duplicate(x => x.Type)
                .GinIndexJsonData();
            For<Ticket>()
                .Identity(x => x.Id)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Id)
                .Duplicate(x => x.Created, dbType: NpgsqlDbType.TimestampTz)
                .Duplicate(x => x.Expires, dbType: NpgsqlDbType.TimestampTz)
                .Duplicate(x => x.IsAuthorizedByRo, dbType: NpgsqlDbType.Boolean)
                .GinIndexJsonData();
            For<AuthorizationCode>()
                .Identity(x => x.Code)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Code)
                .Duplicate(x => x.ClientId)
                .GinIndexJsonData();
            For<ConfirmationCode>()
                .Identity(x => x.Value)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Value)
                .GinIndexJsonData();
            For<GrantedToken>()
                .Identity(x => x.Id)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Id)
                .Duplicate(x => x.Scope)
                .Duplicate(x => x.AccessToken)
                .Duplicate(x => x.ClientId)
                .Duplicate(x => x.CreateDateTime, dbType: NpgsqlDbType.TimestampTz)
                .Duplicate(x => x.ExpiresIn, dbType: NpgsqlDbType.Integer)
                .Duplicate(x => x.IdToken ?? "", notNull: false, dbType: NpgsqlDbType.Varchar)
                .Duplicate(x => x.ParentTokenId ?? "", notNull: false, dbType: NpgsqlDbType.Varchar)
                .Duplicate(x => x.RefreshToken ?? "", notNull: false, dbType: NpgsqlDbType.Varchar)
                .Duplicate(x => x.TokenType, "character(10)")
                .GinIndexJsonData();
            For<JsonWebKeyContainer>()
                .Identity(x => x.Id)
                //.UniqueIndex(UniqueIndexType.Computed, s => s.Id)
                .Duplicate(x => x.Jwk.Alg, pgType: "character(20)")
                .Duplicate(x => x.Jwk.Use, "character(3)")
                .Duplicate(x => x.Jwk.HasPrivateKey, dbType: NpgsqlDbType.Boolean)
                .Index(x => x.Jwk.KeyOps)
                .GinIndexJsonData();
            For<DeviceAuthorizationData>()
                .Identity(x => x.DeviceCode)
                .Duplicate(x => x.Response.UserCode, "character(8)", notNull: true)
                .Duplicate(x => x.ClientId, notNull: true);
        }
    }
}
