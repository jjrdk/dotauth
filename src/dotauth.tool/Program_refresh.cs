namespace dotauth.tool;

using System.Text.Json;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Responses;

internal partial class Program
{
    private static async Task Refresh(RefreshArgs refreshArgs)
    {
        var config = await GetConfiguration().ConfigureAwait(false);
        if (config == null)
        {
            await Console.Out.WriteLineAsync("Missing configuration. Did you run the `configure` action?")
                .ConfigureAwait(false);
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
            var options = DefaultJsonSerializerOptions.Instance;
            options.WriteIndented = true;
            var json = JsonSerializer.Serialize(token.Item, options);
            await Console.Out.WriteLineAsync(json).ConfigureAwait(false);
        }
    }
}
