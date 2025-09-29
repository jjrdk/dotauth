namespace DotAuth.Shared;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;

internal sealed class JsonWebKeySetConverter : JsonConverter<JsonWebKeySet>
{
    public override JsonWebKeySet Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var obj = JsonNode.Parse(ref reader);
        var json = obj?.ToJsonString();
        return new JsonWebKeySet(json);
    }

    public override void Write(Utf8JsonWriter writer, JsonWebKeySet value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("keys");
        writer.WriteRawValue(JsonSerializer.Serialize(value.Keys, options));
        writer.WriteEndObject();
    }
}

internal sealed class JwtPayloadConverter : JsonConverter<JwtPayload>
{
    public override JwtPayload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonNode.Parse(ref reader) is not JsonObject obj)
        {
            throw new Exception("Failed to read json");
        }

        var nbf = obj["nbf"];
        var exp = obj["exp"];
        var iat = obj["iat"];
        var aud = obj["aud"];
        string[] audience = [];
        if (aud is JsonArray arr)
        {
            audience = arr.Where(a => a != null).Select(a => a!.GetValue<string>()).ToArray();
        }

        var claims = obj["claims"]?.Deserialize<Claim[]>(options);
        return new JwtPayload(
            obj["iss"]?.GetValue<string>() ?? "",
            null,
            audience,
            claims,
            new Dictionary<string, object?>(),
            nbf == null ? null : DateTimeOffset.FromUnixTimeSeconds(nbf.GetValue<long>()).DateTime,
            exp == null ? null : DateTimeOffset.FromUnixTimeSeconds(exp.GetValue<long>()).DateTime,
            iat == null ? null : DateTimeOffset.FromUnixTimeSeconds(iat.GetValue<long>()).DateTime);
    }

    public override void Write(Utf8JsonWriter writer, JwtPayload value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("acr", value.Acr);
        writer.WriteString("azp", value.Azp);
        writer.WriteStartArray("aud");
        foreach (var aud in value.Aud)
        {
            writer.WriteStringValue(aud);
        }

        writer.WriteEndArray();
        if (value.Sub != null)
        {
            writer.WriteString("sub", value.Sub);
        }

        writer.WriteString("iss", value.Iss);
        if (value.NotBefore.HasValue)
        {
            writer.WriteNumber("nbf", value.NotBefore.Value);
        }

        if (value.Expiration.HasValue)
        {
            writer.WriteNumber("exp", value.Expiration.Value);
        }

        writer.WriteNumber("iat",
            value.IssuedAt == default ? 0 : new DateTimeOffset(value.IssuedAt).ToUnixTimeSeconds());
        writer.WriteStartArray("claims");
        foreach (var claim in value.Claims)
        {
            writer.WriteStartObject();
            writer.WriteString("type", claim.Type);
            writer.WriteString("value", claim.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}

internal sealed class ClaimConverter : JsonConverter<Claim>
{
    public override Claim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var obj = JsonNode.Parse(ref reader);
        if (obj == null)
        {
            throw new Exception("Failed to read json");
        }

        return new Claim(
            obj["type"]?.GetValue<string>() ?? "",
            obj["value"]?.GetValue<string>() ?? "",
            obj["valueType"]?.GetValue<string>(),
            obj["issuer"]?.GetValue<string>());
    }

    public override void Write(Utf8JsonWriter writer, Claim value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        writer.WriteString("value", value.Value);
        writer.WriteEndObject();
    }
}

internal sealed class RegexConverter : JsonConverter<Regex>
{
    public override Regex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var regex = reader.GetString();
        return new Regex(regex ?? "", RegexOptions.Compiled);
    }

    public override void Write(Utf8JsonWriter writer, Regex value, JsonSerializerOptions options)
    {
        writer.WriteRawValue($"\"{value}\"");
    }
}

/// <inheritdoc cref="System.Text.Json.Serialization.JsonSerializerContext" />
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString |
        JsonNumberHandling.AllowNamedFloatingPointLiterals,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
    UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
    Converters =
    [
        typeof(ClaimConverter),
        typeof(JwtPayloadConverter),
        typeof(JsonWebKeySetConverter),
        typeof(RegexConverter)
    ])]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(AuthorizationCode))]
[JsonSerializable(typeof(Client))]
[JsonSerializable(typeof(ClientSecret))]
[JsonSerializable(typeof(ClaimData))]
[JsonSerializable(typeof(ConfirmationCode))]
[JsonSerializable(typeof(ErrorDetails))]
[JsonSerializable(typeof(GrantedToken))]
[JsonSerializable(typeof(GrantedToken[]))]
[JsonSerializable(typeof(Permission))]
[JsonSerializable(typeof(Permission[]))]
[JsonSerializable(typeof(PolicyRule))]
[JsonSerializable(typeof(PagedResult<Client>))]
[JsonSerializable(typeof(PagedResult<ResourceSetDescription>))]
[JsonSerializable(typeof(PagedResult<ResourceOwner>))]
[JsonSerializable(typeof(ResourceOwner))]
[JsonSerializable(typeof(ResourceSet))]
[JsonSerializable(typeof(ResourceSetDescription))]
[JsonSerializable(typeof(Consent))]
[JsonSerializable(typeof(Consent[]))]
[JsonSerializable(typeof(Scope))]
[JsonSerializable(typeof(Ticket))]
[JsonSerializable(typeof(TicketLine))]
[JsonSerializable(typeof(AddResourceOwnerRequest))]
[JsonSerializable(typeof(AuthorizationRequest))]
[JsonSerializable(typeof(ConfirmationCodeRequest))]
[JsonSerializable(typeof(DeviceAuthorizationData))]
[JsonSerializable(typeof(DeviceAuthorizationRequest))]
[JsonSerializable(typeof(DynamicClientRegistrationRequest))]
[JsonSerializable(typeof(IntrospectionRequest))]
[JsonSerializable(typeof(LinkProfileRequest))]
[JsonSerializable(typeof(PermissionRequest))]
[JsonSerializable(typeof(PermissionRequest[]))]
[JsonSerializable(typeof(RevokeSessionRequest))]
[JsonSerializable(typeof(SearchAuthPolicies))]
[JsonSerializable(typeof(SearchClientsRequest))]
[JsonSerializable(typeof(SearchResourceOwnersRequest))]
[JsonSerializable(typeof(SearchResourceSet))]
[JsonSerializable(typeof(SearchScopesRequest))]
[JsonSerializable(typeof(UpdateResourceOwnerClaimsRequest))]
[JsonSerializable(typeof(UpdateResourceOwnerPasswordRequest))]
[JsonSerializable(typeof(AddPolicyResponse))]
[JsonSerializable(typeof(AddResourceSetResponse))]
[JsonSerializable(typeof(AddResourceOwnerResponse))]
[JsonSerializable(typeof(AddScopeResponse))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(DeviceAuthorizationResponse))]
[JsonSerializable(typeof(DiscoveryInformation))]
[JsonSerializable(typeof(DynamicClientRegistrationResponse))]
[JsonSerializable(typeof(EditPolicyResponse))]
[JsonSerializable(typeof(GrantedTokenResponse))]
[JsonSerializable(typeof(IntrospectionResponse))]
[JsonSerializable(typeof(OauthIntrospectionResponse))]
[JsonSerializable(typeof(PolicyResponse))]
[JsonSerializable(typeof(PolicyRuleResponse))]
[JsonSerializable(typeof(ProfileResponse))]
[JsonSerializable(typeof(ResourceSetResponse))]
[JsonSerializable(typeof(SearchAuthPoliciesResponse))]
[JsonSerializable(typeof(SubjectResponse))]
[JsonSerializable(typeof(TicketResponse))]
[JsonSerializable(typeof(UmaConfiguration))]
[JsonSerializable(typeof(UmaConfigurationResponse))]
[JsonSerializable(typeof(UmaIntrospectionResponse))]
[JsonSerializable(typeof(UpdateResourceSetResponse))]
[JsonSerializable(typeof(UpdateScopeResponse))]
public partial class SharedSerializerContext : JsonSerializerContext
{
}
