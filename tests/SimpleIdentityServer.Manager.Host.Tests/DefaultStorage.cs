using SimpleIdentityServer.Core.Common.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Manager.Host.Tests
{
    public static class DefaultStorage
    {
        public static List<ResourceOwner> GetUsers()
        {
            return new List<ResourceOwner>
            {
                new ResourceOwner
                {
                    Id = "administrator",
                    Claims = new List<Claim>
                    {
                        new Claim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "administrator")
                    },
                    Password = "password",
                    IsLocalAccount = true
                },
                new ResourceOwner
                {
                    Id = "user",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim(SimpleIdentityServer.Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "user")
                    },
                    IsLocalAccount = true
                }
            };
        }
    }
}
