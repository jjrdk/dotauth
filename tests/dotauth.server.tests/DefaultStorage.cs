namespace DotAuth.Server.Tests;

using System.Collections.Generic;
using System.Security.Claims;
using DotAuth.Shared;
using DotAuth.Shared.Models;

public static class DefaultStorage
{
    public static List<ResourceOwner> GetUsers()
    {
        return
        [
            new ResourceOwner
            {
                Subject = "administrator",
                Claims =
                [
                    new Claim(OpenIdClaimTypes.Subject, "administrator")
                ],
                Password = "password",
                IsLocalAccount = true
            },

            new ResourceOwner
            {
                Subject = "user",
                Password = "password",
                Claims =
                [
                    new Claim(OpenIdClaimTypes.Subject, "user")
                ],
                IsLocalAccount = true
            }
        ];
    }
}