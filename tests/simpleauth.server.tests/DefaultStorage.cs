namespace SimpleAuth.Server.Tests
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Shared;
    using Shared.Models;

    public static class DefaultStorage
    {
        public static List<ResourceOwner> GetUsers()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator",
                    Claims = new []
                    {
                        new Claim(OpenIdClaimTypes.Subject, "administrator")
                    },
                    Password = "password",
                    IsLocalAccount = true
                },
                new ResourceOwner
                {
                    Id = "user",
                    Password = "password",
                    Claims = new []
                    {
                        new Claim(OpenIdClaimTypes.Subject, "user")
                    },
                    IsLocalAccount = true
                }
            };
        }
    }
}
