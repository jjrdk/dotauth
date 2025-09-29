namespace DotAuth.Uma.Tests;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotAuth.Client;
using Shared;
using Shared.Models;
using Shared.Requests;

public class AuthenticationTests
{
    [Fact]
    public async Task GetUrl()
    {
        var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, ServerCertificateCustomValidationCallback =
            (msg,cert,chain,policyErrors) => { return true;}
        });
        var pkce = CodeChallengeMethods.Rs256.BuildPkce();
        var client = new TokenClient(
            TokenCredentials.FromClientCredentials("dataapp", "nvnwvnervsrakri"),
            () => http,
            new Uri("https://identity.reimers.dk"));
        var option = await client.GetAuthorization(
            new AuthorizationRequest(
                ["openid", "email", "profile", "role", "offline", UmaConstants.UmaProtectionScope],
                ResponseTypeNames.All,
                "dataapp",
                new Uri("dataapp://callback"),
                pkce.CodeChallenge,
                CodeChallengeMethods.Rs256,
                "454325453ggd"){nonce = "fgsgsdd"});
        switch (option)
        {
            case Option<Uri>.Result result:
                Assert.NotEmpty(result.Item.AbsoluteUri);
                break;
            case Option<Uri>.Error error:
                Assert.Fail(error.Details.Title);
                break;
        }
    }

}
