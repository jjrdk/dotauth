namespace dotauth.tool;

using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;
using Newtonsoft.Json;

internal partial class Program
{
    private static async Task Refresh(RefreshArgs refreshArgs)
    {
        var config = await GetConfiguration().ConfigureAwait(false);
        if (config == null)
        {
            await Console.Out.WriteLineAsync("Missing configuration. Did you run the `configure` action?").ConfigureAwait(false);
            return;
        }

        using var httpClient = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false });
        var client = new TokenClient(
            TokenCredentials.FromClientCredentials(config.ClientId, config.ClientSecret),
            // ReSharper disable once AccessToDisposedClosure
            () => httpClient,
            new Uri(config.Authority));
        var tokenOption = await client.GetToken(TokenRequest.FromRefreshToken(refreshArgs.RefreshToken))
            .ConfigureAwait(false);
        if (tokenOption is Option<GrantedTokenResponse>.Result token)
        {
            var json = JsonConvert.SerializeObject(token.Item, Formatting.Indented);
            await Console.Out.WriteLineAsync(json).ConfigureAwait(false);
        }
    }

}