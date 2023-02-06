namespace dotauth.tool;

using System.Diagnostics;
using System.Net;
using DotAuth.Client;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
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

        var pkce = config.CodeChallengeMethod.BuildPkce();
        using var httpClient = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false });
        var client = new TokenClient(
            TokenCredentials.FromClientCredentials(config.ClientId, config.ClientSecret),
            // ReSharper disable once AccessToDisposedClosure
            () => httpClient,
            new Uri(config.Authority));
        var state = Guid.NewGuid().ToString("N");
        var uri = await client.GetAuthorization(
                new AuthorizationRequest(
                    args.Scopes,
                    new[] { ResponseTypeNames.Code },
                    config.ClientId,
                    new Uri(config.RedirectUrl),
                    pkce.CodeChallenge,
                    config.CodeChallengeMethod,
                    state) { nonce = Guid.NewGuid().ToString("N"), response_mode = args.ResponseMode })
            .ConfigureAwait(false);
        if (uri is Option<Uri>.Result result)
        {
            using var process = Process.Start(
                new ProcessStartInfo { FileName = result.Item.AbsoluteUri, UseShellExecute = true });
            var listener = new HttpListener();
            var authorityPart = new Uri(config.RedirectUrl).GetLeftPart(UriPartial.Authority);
            listener.Prefixes.Add($"{authorityPart}/");
            listener.Start();
            while (listener.IsListening)
            {
                var context = await listener.GetContextAsync().ConfigureAwait(false);
                var code = context.Request.QueryString.Get("code");
                if (code != null)
                {
                    var tokenOption = await client.GetToken(
                            TokenRequest.FromAuthorizationCode(code, config.RedirectUrl, pkce.CodeVerifier))
                        .ConfigureAwait(false);
                    if (tokenOption is Option<GrantedTokenResponse>.Result token)
                    {
                        var json = JsonConvert.SerializeObject(token.Item, Formatting.Indented);
                        await Console.Out.WriteLineAsync(json).ConfigureAwait(false);
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.OutputStream.WriteAsync(
                        "<html><head><title>Token flow completed</title><head><body><script>window.close();</script></body></html>"u8
                            .ToArray())
                    .ConfigureAwait(false);
                context.Response.Close();
                listener.Stop();
            }

            process?.Close();
        }
    }
}