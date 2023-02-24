namespace DotAuth.Stores.Marten;

using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using global::Marten;
using global::Marten.Schema.Indexing.Unique;
using NpgsqlTypes;
using Weasel.Postgresql.Tables;

/// <summary>
/// Defines the default marten registry for stored DotAuth types.
/// </summary>
/// <seealso cref="MartenRegistry" />
public sealed class DotAuthRegistry : MartenRegistry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DotAuthRegistry"/> class.
    /// </summary>
    public DotAuthRegistry()
    {
        For<Scope>()
            .Identity(x => x.Name)
            .Duplicate(x => x.IsDisplayedInConsent, dbType: NpgsqlDbType.Boolean)
            .Duplicate(x => x.Type, "varchar(15)");
        For<Filter>()
            .Identity(x => x.Name);
        For<ResourceOwner>()
            .Identity(x => x.Subject)
#pragma warning disable CS8603
            .Duplicate(x => x.Password, configure: idx =>
            {
                idx.IsConcurrent = false;
                idx.TenancyScope = TenancyScope.PerTenant;
            })
#pragma warning restore CS8603
            .Index(
                x => x.Claims,
                configure: idx =>
                {
                    idx.IsUnique = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                })
            .Index(
                x => x.ExternalLogins,
                configure: idx =>
                {
                    idx.IsUnique = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                })
            .GinIndexJsonData(
                idx =>
                {
                    idx.IsConcurrent = false;
                });
        For<Consent>()
            .Identity(x => x.Id)
            .Duplicate(x => x.Subject, notNull: true)
            .Index(
                x => x.ClientName,
                idx =>
                {
                    idx.IsConcurrent = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                });
        For<Client>()
            .Identity(x => x.ClientId)
#pragma warning disable CS8603
            .Duplicate(x => x.IdTokenEncryptedResponseAlg, "varchar(10)", configure: index => index.TenancyScope = TenancyScope.PerTenant)
#pragma warning restore CS8603
            .GinIndexJsonData(
                idx =>
                {
                    idx.IsConcurrent = false;
                });
        For<OwnedResourceSet>()
            .Identity(x => x.Id)
            .Duplicate(x => x.Owner, configure: index => index.TenancyScope = TenancyScope.PerTenant)
            .Duplicate(x => x.Name, configure: index => index.TenancyScope = TenancyScope.PerTenant)
            .Duplicate(x => x.Description, configure: index => index.TenancyScope = TenancyScope.PerTenant)
            .Duplicate(x => x.Type, configure: index => index.TenancyScope = TenancyScope.PerTenant)
            .GinIndexJsonData(
                idx =>
                {
                    idx.IsConcurrent = false;
                });
        For<Ticket>()
            .Identity(x => x.Id)
            .Duplicate(
                x => x.ResourceOwner,
                configure: idx =>
                {
                    idx.IsUnique = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                })
            .Index(x => x.Created, configure: idx =>
            {
                idx.IsUnique = false;
                idx.TenancyScope = TenancyScope.PerTenant;
                idx.IsConcurrent = false;
            })
            .Index(x => x.Expires, configure: idx =>
            {
                idx.IsUnique = false;
                idx.TenancyScope = TenancyScope.PerTenant;
                idx.IsConcurrent = false;
            })
            .Duplicate(x => x.IsAuthorizedByRo, dbType: NpgsqlDbType.Boolean);
        For<AuthorizationCode>()
            .Identity(x => x.Code)
            .Duplicate(x => x.ClientId);
        For<ConfirmationCode>()
            .Identity(x => x.Value)
            .Index(
                s => s.Value,
                idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                });
        For<GrantedToken>()
            .Identity(x => x.Id)
            .Duplicate(
                x => x.Scope,
                configure: idx =>
                {
                    idx.IsUnique = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                })
            .Duplicate(
                x => x.AccessToken,
                configure: idx =>
                {
                    idx.IsUnique = true;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                })
            .Duplicate(
                x => x.ClientId,
                configure: idx =>
                {
                    idx.IsUnique = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                })
            .Duplicate(x => x.CreateDateTime)
            .Duplicate(x => x.ExpiresIn)
#pragma warning disable CS8603
            .Duplicate(
                x => x.IdToken,
                dbType: NpgsqlDbType.Varchar,
                configure: idx =>
                {
                    idx.IsUnique = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                })
            .Duplicate(x => x.ParentTokenId, dbType: NpgsqlDbType.Varchar)
            .Duplicate(
                x => x.RefreshToken,
                dbType: NpgsqlDbType.Varchar,
                configure: idx =>
                {
                    idx.IsUnique = true;

                    idx.IsConcurrent = false;
                })
#pragma warning restore CS8603
            .Duplicate(
                x => x.TokenType,
                "character(10)",
                configure: idx =>
                {
                    idx.IsUnique = false;
                    idx.IsConcurrent = false;
                })
            .GinIndexJsonData(
                idx =>
                {
                    idx.IsConcurrent = false;
                });
        For<JsonWebKeyContainer>()
            .Identity(x => x.Id)
            .Duplicate(x => x.Jwk.Alg, pgType: "character(20)")
            .Duplicate(x => x.Jwk.Use, "character(3)")
            .Duplicate(x => x.Jwk.HasPrivateKey, dbType: NpgsqlDbType.Boolean)
            .Index(x => x.Jwk.KeyOps);
        For<DeviceAuthorizationData>()
            .Identity(x => x.DeviceCode)
            .Duplicate(
                x => x.Response.UserCode,
                "character(8)",
                notNull: true,
                configure: idx =>
                {
                    idx.IsUnique = true;
                    idx.IsConcurrent = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                })
            .Duplicate(
                x => x.ClientId,
                notNull: true,
                configure: idx =>
                {
                    idx.IsUnique = false;
                    idx.TenancyScope = TenancyScope.PerTenant;
                    idx.IsConcurrent = false;
                });
    }
}
