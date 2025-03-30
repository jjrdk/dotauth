namespace DotAuth.Shared;

using System;
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

internal static class DefaultJsonSerializerOptions
{
    static DefaultJsonSerializerOptions()
    {
        Instance = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString |
                JsonNumberHandling.AllowNamedFloatingPointLiterals,
            AllowTrailingCommas = true,
            Converters =
            {
                new ClaimConverter(),
                new JwtPayloadConverter(),
                new JsonWebKeySetConverter(),
                new RegexConverter()
            },
            ReadCommentHandling = JsonCommentHandling.Skip,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
            TypeInfoResolverChain = { SharedSerializerContext.Default }
        };
    }

    public static JsonSerializerOptions Instance { get; }

    private sealed class RegexConverter : JsonConverter<Regex>
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

    private sealed class ClaimConverter : JsonConverter<Claim>
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
            writer.WriteString(value.Type, value.Value);
            writer.WriteEndObject();
        }
    }

    private sealed class JwtPayloadConverter : JsonConverter<JwtPayload>
    {
        public override JwtPayload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var obj = JsonNode.Parse(ref reader) as JsonObject;
            if (obj == null)
            {
                throw new Exception("Failed to read json");
            }
            var nbf = obj["nbf"];
            var exp = obj["exp"];
            return new JwtPayload(
                obj["iss"]?.GetValue<string>(),
                obj["aud"]?.GetValue<string>(),
                obj["claims"]?.GetValue<Claim[]>(),
                nbf == null ? null : DateTimeOffset.FromUnixTimeSeconds(nbf.GetValue<long>()).DateTime,
                exp == null ? null : DateTimeOffset.FromUnixTimeSeconds(exp.GetValue<long>()).DateTime);
        }

        public override void Write(Utf8JsonWriter writer, JwtPayload value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("acr", value.Acr);
            writer.WriteString("azp", value.Azp);
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

    private sealed class JsonWebKeySetConverter : JsonConverter<JsonWebKeySet>
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
}

/// <inheritdoc cref="System.Text.Json.Serialization.JsonSerializerContext" />
[JsonSerializable(typeof(Client))]
[JsonSerializable(typeof(ClientSecret))]
[JsonSerializable(typeof(ClaimData))]
[JsonSerializable(typeof(ErrorDetails))]
[JsonSerializable(typeof(GrantedToken))]
[JsonSerializable(typeof(Permission))]
[JsonSerializable(typeof(Permission[]))]
[JsonSerializable(typeof(PolicyRule))]
[JsonSerializable(typeof(PagedResult<Client>))]
[JsonSerializable(typeof(PagedResult<ResourceSetDescription>))]
[JsonSerializable(typeof(PagedResult<ResourceOwner>))]
[JsonSerializable(typeof(ResourceOwner))]
[JsonSerializable(typeof(ResourceSet))]
[JsonSerializable(typeof(ResourceSetDescription))]
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
[JsonSerializable(typeof(TicketResponse))]
[JsonSerializable(typeof(UmaConfiguration))]
[JsonSerializable(typeof(UmaConfigurationResponse))]
[JsonSerializable(typeof(UmaIntrospectionResponse))]
[JsonSerializable(typeof(UpdateResourceSetResponse))]
[JsonSerializable(typeof(UpdateScopeResponse))]
public partial class SharedSerializerContext : JsonSerializerContext
{
}
