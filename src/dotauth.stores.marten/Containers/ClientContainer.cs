namespace DotAuth.Stores.Marten.Containers;

using System;
using System.Runtime.Serialization;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the storage container type for <see cref="Client"/>.
/// </summary>
[DataContract]
public sealed class ClientContainer : Client
{
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    [DataMember(Name = "id")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Converts the contents of this container to a <see cref="Client"/>.
    /// </summary>
    /// <returns>A <see cref="Client"/> instance.</returns>
    public Client ToClient()
    {
        return new Client
        {
            AllowedScopes = AllowedScopes,
            ApplicationType = ApplicationType,
            Claims = Claims,
            ClientId = ClientId,
            ClientName = ClientName,
            ClientUri = ClientUri,
            Contacts = Contacts,
            DefaultAcrValues = DefaultAcrValues,
            DefaultMaxAge = DefaultMaxAge, Secrets = Secrets, GrantTypes = GrantTypes,
            LogoUri = LogoUri, PolicyUri = PolicyUri, ResponseTypes = ResponseTypes,
            TosUri = TosUri, RedirectionUrls = RedirectionUrls,
            InitiateLoginUri = InitiateLoginUri, RequirePkce = RequirePkce,
            RequireAuthTime = RequireAuthTime, SectorIdentifierUri = SectorIdentifierUri,
            PostLogoutRedirectUris = PostLogoutRedirectUris, TokenLifetime = TokenLifetime,
            JsonWebKeys = JsonWebKeys, RequestObjectEncryptionAlg = RequestObjectEncryptionAlg,
            RequestObjectEncryptionEnc = RequestObjectEncryptionEnc,
            RequestObjectSigningAlg = RequestObjectSigningAlg,
            IdTokenEncryptedResponseAlg = IdTokenEncryptedResponseAlg,
            IdTokenEncryptedResponseEnc = IdTokenEncryptedResponseEnc,
            IdTokenSignedResponseAlg = IdTokenSignedResponseAlg,
            UserInfoEncryptedResponseAlg = UserInfoEncryptedResponseAlg,
            UserInfoEncryptedResponseEnc = UserInfoEncryptedResponseEnc,
            UserInfoSignedResponseAlg = UserInfoSignedResponseAlg,
            TokenEndPointAuthSigningAlg = TokenEndPointAuthSigningAlg,
            TokenEndPointAuthMethod = TokenEndPointAuthMethod,
            UserClaimsToIncludeInAuthToken = UserClaimsToIncludeInAuthToken
        };
    }

    /// <summary>
    /// Creates a new <see cref="ClientContainer"/> from the specified <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The <paramref name="client"/> to copy content from.</param>
    /// <returns>A <see cref="ClientContainer"/> instance.</returns>
    public static ClientContainer Create(Client client)
    {
        return new ClientContainer
        {
            Id = Guid.NewGuid().ToString("N"),
            AllowedScopes = client.AllowedScopes,
            ApplicationType = client.ApplicationType,
            Claims = client.Claims,
            ClientId = client.ClientId,
            ClientName = client.ClientName,
            ClientUri = client.ClientUri,
            Contacts = client.Contacts,
            DefaultAcrValues = client.DefaultAcrValues,
            DefaultMaxAge = client.DefaultMaxAge, Secrets = client.Secrets, GrantTypes = client.GrantTypes,
            LogoUri = client.LogoUri, PolicyUri = client.PolicyUri, ResponseTypes = client.ResponseTypes,
            TosUri = client.TosUri, RedirectionUrls = client.RedirectionUrls,
            InitiateLoginUri = client.InitiateLoginUri, RequirePkce = client.RequirePkce,
            RequireAuthTime = client.RequireAuthTime, SectorIdentifierUri = client.SectorIdentifierUri,
            PostLogoutRedirectUris = client.PostLogoutRedirectUris, TokenLifetime = client.TokenLifetime,
            JsonWebKeys = client.JsonWebKeys, RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
            RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
            RequestObjectSigningAlg = client.RequestObjectSigningAlg,
            IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
            IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
            IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
            UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg,
            UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc,
            UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg,
            TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg,
            TokenEndPointAuthMethod = client.TokenEndPointAuthMethod,
            UserClaimsToIncludeInAuthToken = client.UserClaimsToIncludeInAuthToken
        };
    }

    /// <summary>
    /// Updates the container with the specified <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The <paramref name="client"/> to copy content from.</param>
    public void Update(Client client)
    {
        AllowedScopes = client.AllowedScopes;
        ApplicationType = client.ApplicationType;
        Claims = client.Claims;
        ClientId = client.ClientId;
        ClientName = client.ClientName;
        ClientUri = client.ClientUri;
        Contacts = client.Contacts;
        DefaultAcrValues = client.DefaultAcrValues;
        DefaultMaxAge = client.DefaultMaxAge;
        Secrets = client.Secrets;
        GrantTypes = client.GrantTypes;
        LogoUri = client.LogoUri;
        PolicyUri = client.PolicyUri;
        ResponseTypes = client.ResponseTypes;
        TosUri = client.TosUri;
        RedirectionUrls = client.RedirectionUrls;
        InitiateLoginUri = client.InitiateLoginUri;
        RequirePkce = client.RequirePkce;
        RequireAuthTime = client.RequireAuthTime;
        SectorIdentifierUri = client.SectorIdentifierUri;
        PostLogoutRedirectUris = client.PostLogoutRedirectUris;
        TokenLifetime = client.TokenLifetime;
        JsonWebKeys = client.JsonWebKeys;
        RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg;
        RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc;
        RequestObjectSigningAlg = client.RequestObjectSigningAlg;
        IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg;
        IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc;
        IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg;
        UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg;
        UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc;
        UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg;
        TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg;
        TokenEndPointAuthMethod = client.TokenEndPointAuthMethod;
        UserClaimsToIncludeInAuthToken = client.UserClaimsToIncludeInAuthToken;
    }
}
