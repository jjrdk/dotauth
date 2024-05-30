namespace DotAuth.Tests.Authenticate;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Authenticate;
using DotAuth.Repositories;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using NSubstitute;
using Xunit;

public sealed class AuthenticateClientFixture
{
    private readonly IClientStore _clientRepositoryStub;
    private readonly AuthenticateClient _authenticateClient;

    public AuthenticateClientFixture()
    {
        _clientRepositoryStub = Substitute.For<IClientStore>();
        _authenticateClient = new AuthenticateClient(_clientRepositoryStub, new InMemoryJwksRepository());
    }

    [Fact]
    public async Task When_The_ClientId_Cannot_Be_Fetch_Then_Message_Error_Is_Returned_And_Result_Is_Null()
    {
        var authenticationInstruction = new AuthenticateInstruction();

        var result = await _authenticateClient.Authenticate(authenticationInstruction, "", CancellationToken.None);

        Assert.Null(result.Client);
        Assert.Equal(SharedStrings.TheClientDoesntExist, result.ErrorMessage);
    }

    [Fact]
    public async Task When_The_ClientId_Is_Not_Valid_Then_Message_Error_Is_Returned_And_Result_Is_Null()
    {
        var authenticationInstruction = new AuthenticateInstruction();
        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Client?)null);

        var result = await _authenticateClient.Authenticate(authenticationInstruction, "", CancellationToken.None);

        Assert.Null(result.Client);
        Assert.Equal(SharedStrings.TheClientDoesntExist, result.ErrorMessage);
    }

    [Fact]
    public async Task
        When_Trying_To_Authenticate_The_Client_Via_Secret_Basic_Then_Operation_Is_Called_Client_Is_Returned_And_Events_Are_Logged()
    {
        const string clientId = "clientId";
        const string secret = "secret";
        var authenticationInstruction = new AuthenticateInstruction
        {
            ClientIdFromAuthorizationHeader = clientId,
            ClientSecretFromAuthorizationHeader = secret
        };
        var client = new Client
        {
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = secret }],
            TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretBasic,
            ClientId = clientId
        };

        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);

        var result = await _authenticateClient.Authenticate(authenticationInstruction, "", CancellationToken.None);

        Assert.NotNull(result.Client);
    }

    [Fact]
    public async Task
        When_Trying_To_Authenticate_The_Client_Via_Secret_Basic_But_Operation_Failed_Then_Event_Is_Not_Logged_And_Null_Is_Returned()
    {
        const string clientId = "clientId";
        var authenticationInstruction = new AuthenticateInstruction { ClientIdFromAuthorizationHeader = clientId };
        var client = new Client
        {
            TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretBasic,
            ClientId = clientId
        };

        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);

        var result = await _authenticateClient.Authenticate(authenticationInstruction, "", CancellationToken.None);

        Assert.Null(result.Client);
    }
}
