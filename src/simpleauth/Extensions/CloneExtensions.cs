namespace SimpleAuth.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Shared;
    using Shared.Models;

    internal static class CloneExtensions
    {
        public static ResourceOwnerProfile Copy(this ResourceOwnerProfile profile)
        {
            return new ResourceOwnerProfile
            {
                CreateDateTime = profile.CreateDateTime,
                Issuer = profile.Issuer,
                ResourceOwnerId = profile.ResourceOwnerId,
                Subject = profile.Subject,
                UpdateTime = profile.UpdateTime
            };
        }

        public static ClaimAggregate Copy(this ClaimAggregate claim)
        {
            return new ClaimAggregate
            {
                Code = claim.Code,
                CreateDateTime = claim.CreateDateTime,
                IsIdentifier = claim.IsIdentifier,
                UpdateDateTime = claim.UpdateDateTime,
                Value = claim.Value
            };
        }

        public static Consent Copy(this Consent consent)
        {
            return new Consent
            {
                Claims =  consent.Claims == null ? new List<string>() : consent.Claims.ToList(),
                Client = consent.Client,
                GrantedScopes = consent.GrantedScopes?.Select(s => s.Copy()).ToList(),
                Id =  consent.Id,
                ResourceOwner = consent.ResourceOwner?.Copy()
            };
        }

        public static Translation Copy(this Translation translation)
        {
            return new Translation
            {
                Code = translation.Code,
                LanguageTag = translation.LanguageTag,
                Value = translation.Value
            };
        }

        public static ResourceOwner Copy(this ResourceOwner user)
        {
            return new ResourceOwner
            {
                Claims = user.Claims == null ? new List<Claim>() : user.Claims.Select(c => c.Copy()).ToList(),
                CreateDateTime = user.CreateDateTime,
                Id = user.Id,
                IsLocalAccount = user.IsLocalAccount,
                Password = user.Password,
                TwoFactorAuthentication = user.TwoFactorAuthentication,
                UpdateDateTime = user.UpdateDateTime,
                UserProfile = user.UserProfile
            };
        }

        public static Claim Copy(this Claim claim)
        {
            return new Claim(claim.Type, claim.Value);
        }

        public static ClientSecret Copy(this ClientSecret clientSecret)
        {
            return new ClientSecret
            {
                Type = clientSecret.Type,
                Value = clientSecret.Value
            };
        }

        public static Scope Copy(this Scope scope)
        {
            return new Scope
            {
                CreateDateTime = scope.CreateDateTime,
                Description = scope.Description,
                IsDisplayedInConsent = scope.IsDisplayedInConsent,
                IsExposed = scope.IsExposed,
                IsOpenIdScope = scope.IsOpenIdScope,
                Name = scope.Name,
                Type = scope.Type,
                UpdateDateTime = scope.UpdateDateTime,
                Claims = scope.Claims == null ? new List<string>() : scope.Claims.ToList()
            };
        }

        public static JsonWebKey Copy(this JsonWebKey jsonWebKey)
        {
            return new JsonWebKey
            {
                Alg = jsonWebKey.Alg,
                KeyOps = jsonWebKey.KeyOps == null ? new KeyOperations[0] : jsonWebKey.KeyOps.ToList().ToArray(),
                Kid = jsonWebKey.Kid,
                Kty = jsonWebKey.Kty,
                SerializedKey = jsonWebKey.SerializedKey,
                Use = jsonWebKey.Use,
                X5t = jsonWebKey.X5t,
                X5tS256 = jsonWebKey.X5tS256,
                X5u = jsonWebKey.X5u
            };
        }
    }
}
