namespace dotauth.tool;

using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Newtonsoft.Json;

internal partial class Program
{
    private static async Task GetToken(TokenArgs args)
    {
        var config = await GetConfiguration().ConfigureAwait(false);
        if (config == null)
        {
            await Console.Out.WriteLineAsync("Missing configuration. Did you run the `configure` action?")
                .ConfigureAwait(false);
            return;
        }

        var authenticateClient = new DefaultAuthenticateClient(
            new AuthenticateClientOptions
            {
                Authority = new Uri(config.Authority),
                Callback = new Uri(config.RedirectUrl),
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                ResponseMode = args.ResponseMode,
                Scopes = args.Scopes.ToArray()
            });

        var code = await authenticateClient.LogIn(CancellationToken.None).ConfigureAwait(false);
        if (code is Option<GrantedTokenResponse>.Result tokenResponse)
        {
            var json = JsonConvert.SerializeObject(tokenResponse.Item);
            await Console.Out.WriteLineAsync(json).ConfigureAwait(false);
        }
    }
}