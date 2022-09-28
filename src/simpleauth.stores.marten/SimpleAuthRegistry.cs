﻿namespace SimpleAuth.Stores.Marten;

using global::Marten;
using global::Marten.Schema.Indexing.Unique;
using NpgsqlTypes;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Requests;

/// <summary>
/// Defines the default marten registry for stored SimpleAuth types.
/// </summary>
/// <seealso cref="MartenRegistry" />
public sealed class SimpleAuthRegistry : MartenRegistry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleAuthRegistry"/> class.
    /// </summary>
    public SimpleAuthRegistry()
    {
        For<Scope>().Identity(x => x.Name)
            .Index(s => s.Name,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = true;
                })
            .Duplicate(x => x.IsDisplayedInConsent, dbType: NpgsqlDbType.Boolean)
            .Duplicate(x => x.Type, "varchar(15)")
            .GinIndexJsonData();
        For<Filter>().Identity(x => x.Name)
            //.UniqueIndex(UniqueIndexType.Computed, s => s.Name)
            .Index(s => s.Name,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = true;
                })
            .GinIndexJsonData();
        For<ResourceOwner>()
            .Identity(x => x.Subject)
            //.UniqueIndex(UniqueIndexType.Computed, s => s.Subject)
            .Index(s => s.Subject,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                })
            .Duplicate(x => x.Password)
            .Index(x => x.Claims, idx => { idx.IsConcurrent = true; })
            .Index(x => x.ExternalLogins, idx => { idx.IsConcurrent = true; })
            .GinIndexJsonData();
        For<Consent>()
            .Identity(x => x.Id)
            //.UniqueIndex(UniqueIndexType.Computed, s => s.Id)
            .Index(s => s.Id,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = true;
                })
            .Duplicate(x => x.Subject, notNull: true)
            .GinIndexJsonData();
        For<Client>()
            .Identity(x => x.ClientId)
            //.UniqueIndex(UniqueIndexType.Computed, s => s.ClientId)
            .Index(
                s => s.ClientId,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = true;
                })
            //.Index(x => x.AllowedScopes)
            //.Index(x => x.GrantTypes)
            .Duplicate(x => x.IdTokenEncryptedResponseAlg, "varchar(10)")
            //.Index(x => x.ResponseTypes)
            //.Index(x => x.Claims)
            .GinIndexJsonData();
        For<OwnedResourceSet>()
            .Identity(x => x.Id)
            //.UniqueIndex(UniqueIndexType.Computed, s => s.Id)
            .Index(s => s.Id,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = true;
                })
            .Duplicate(x => x.Owner)
            .Duplicate(x => x.Name)
            .Duplicate(x => x.Type)
            .GinIndexJsonData();
        For<Ticket>()
            .Identity(x => x.Id)
            //.UniqueIndex(UniqueIndexType.Computed, s => s.Id)
            .Index(s => s.Id,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = true;
                })
            .Duplicate(x => x.Created, dbType: NpgsqlDbType.TimestampTz)
            .Duplicate(x => x.Expires, dbType: NpgsqlDbType.TimestampTz)
            .Duplicate(x => x.IsAuthorizedByRo, dbType: NpgsqlDbType.Boolean)
            .GinIndexJsonData();
        For<AuthorizationCode>()
            .Identity(x => x.Code)
            //.UniqueIndex(UniqueIndexType.Computed, s => s.Code)
            .Index(s => s.Code,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = true;
                })
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
            .Duplicate(x => x.IdToken, dbType: NpgsqlDbType.Varchar)
            .Duplicate(x => x.ParentTokenId, dbType: NpgsqlDbType.Varchar)
            .Duplicate(x => x.RefreshToken, dbType: NpgsqlDbType.Varchar)
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