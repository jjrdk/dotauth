namespace DotAuth.AcceptanceTests.StepDefinitions;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Xunit;

public partial class FeatureTest
{
    private GrantedTokenResponse? _dcrToken;
    private HttpResponseMessage? _dcrResponse;
    private IDynamicRegistrationClient? _registrationClient;
    private DynamicClientRegistrationResponse? _clientRegistrationResponse;

    [Given(@"an out of band dynamic client registration token")]
    public async Task GivenAnOutOfBandDynamicClientRegistrationToken()
    {
        var option = await _tokenClient.GetToken(TokenRequest.FromScopes("dcr"));
        switch (option)
        {
            case Option<GrantedTokenResponse>.Result result:
                _dcrToken = result.Item;
                break;
            case Option<GrantedTokenResponse>.Error error:
                Assert.Fail(error.Details.Title);
                break;
        }
    }

    [When(@"posting a dynamic client registration request to the auth server")]
    public async Task WhenPostingADynamicClientRegistrationRequestToTheAuthServer()
    {
        var registration = new DynamicClientRegistrationRequest
        {
            ApplicationType = ApplicationTypes.Web,
            Contacts = ["A. Tester"],
            ClientName = "Test Client",
            RedirectUris = ["app://somewhere"],
            TokenEndpointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost
        };
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(new Uri(BaseUrl), "clients/register"),
            Content = new StringContent(
                JsonConvert.SerializeObject(registration),
                Encoding.UTF8,
                MediaTypeHeaderValue.Parse("application/json"))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _dcrToken!.AccessToken);
        var client = _fixture.Client();
        _dcrResponse = await client.SendAsync(request);
    }

    [Then(@"the response should be a 201")]
    public void ThenTheResponseShouldBeA()
    {
        Assert.Equal(HttpStatusCode.Created, _dcrResponse!.StatusCode);
    }

    [Then(@"the response should contain a client_id and client_secret")]
    public async Task ThenTheResponseShouldContainAClientIdAndClientSecret()
    {
        var json = await _dcrResponse!.Content.ReadAsStringAsync();
        var clientInfo = JsonConvert.DeserializeObject<DynamicClientRegistrationResponse>(json);

        Assert.False(string.IsNullOrWhiteSpace(clientInfo?.ClientId));
        Assert.False(string.IsNullOrWhiteSpace(clientInfo.ClientSecret));
    }

    [When(@"creating a new DynamicRegistrationClient")]
    public void WhenCreatingANewDynamicRegistrationClient()
    {
        _registrationClient = new DynamicRegistrationClient(_fixture.Client, new Uri(BaseUrl));
    }

    [Then(@"can use it to create a new client")]
    public async Task ThenCanUseItToCreateANewClient()
    {
        var clientResponse = await _registrationClient!.Register(
            _dcrToken!.AccessToken,
            new DynamicClientRegistrationRequest
            {
                ApplicationType = ApplicationTypes.Web,
                Contacts = ["A. Tester"],
                ClientName = "Test Client",
                RedirectUris = ["app://somewhere"],
                TokenEndpointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost
            },
            CancellationToken.None);

        var result = Assert.IsType<Option<DynamicClientRegistrationResponse>.Result>(clientResponse);
        _clientRegistrationResponse = result.Item;
    }

    [Then(@"can modify the registered app")]
    public async Task ThenCanModifyTheRegisteredApp()
    {
        var option = await _registrationClient!.Modify(
            _dcrToken!.AccessToken,
            _clientRegistrationResponse!.ClientId,
            new DynamicClientRegistrationRequest
            {
                ApplicationType = ApplicationTypes.Web,
                Contacts = ["Another Tester"],
                ClientName = "New Name",
                RedirectUris = ["app://somewhere"],
                TokenEndpointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost
            },
            CancellationToken.None);

        Assert.IsType<Option<DynamicClientRegistrationResponse>.Result>(option);
    }
}
