namespace dotauth.tool
{
    using CommandLine;
    using DotAuth.Client;
    using Newtonsoft.Json;
    using System.Diagnostics;
    using System.Net;
    using System.Text;
    using DotAuth.Shared;
    using DotAuth.Shared.Requests;
    using DotAuth.Shared.Responses;

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var parser = new Parser(
                settings =>
                {
                    settings.AutoHelp = true;
                    settings.AutoVersion = true;
                    settings.CaseInsensitiveEnumValues = true;
                    settings.CaseSensitive = false;
                    settings.EnableDashDash = true;
                    settings.GetoptMode = false;
                    settings.HelpWriter = Console.Out;
                    settings.IgnoreUnknownArguments = true;
                    settings.MaximumDisplayWidth = 80;
                    settings.PosixlyCorrect = true;
                });
            var result = parser.ParseArguments<TokenArgs, ConfigureArgs>(args)
                .MapResult(
                    (TokenArgs tokenArgs) => GetToken(tokenArgs),
                    (ConfigureArgs configArgs) => Configure(configArgs),
                    _ => Task.CompletedTask);
            await result.ConfigureAwait(false);
        }

        private static async Task<ToolConfig?> GetConfiguration()
        {
            var configFile = GetConfigFilePath();
            if (!File.Exists(configFile))
            {
                return null;
            }

            var configText = await File.ReadAllTextAsync(configFile).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<ToolConfig>(configText)!;
        }

        private static string GetConfigFilePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configFile = Path.Combine(appDataPath, "dotauth-tool", "config.json");
            return configFile;
        }

        private static async Task Configure(ConfigureArgs args)
        {
            var config = await GetConfiguration().ConfigureAwait(false) ?? new ToolConfig();

            if (Uri.TryCreate(args.Authority, UriKind.Absolute, out var auth))
            {
                config.Authority = auth.AbsoluteUri;
            }

            if (Uri.TryCreate(args.RedirectUrl, UriKind.Absolute, out var r))
            {
                config.RedirectUrl = r.AbsoluteUri;
            }

            if (!string.IsNullOrWhiteSpace(args.ClientId))
            {
                config.ClientId = args.ClientId;
            }

            if (!string.IsNullOrWhiteSpace(args.ClientSecret))
            {
                config.ClientSecret = args.ClientSecret;
            }

            if (!string.IsNullOrWhiteSpace(args.CodeChallengeMethod))
            {
                config.CodeChallengeMethod = args.CodeChallengeMethod;
            }

            var configFile = GetConfigFilePath();
            var directoryName = Path.GetDirectoryName(configFile);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName!);
            }

            await File.WriteAllTextAsync(configFile, JsonConvert.SerializeObject(config), Encoding.UTF8)
                .ConfigureAwait(false);

            await Console.Out.WriteLineAsync("Tool configured").ConfigureAwait(false);
        }

        private static async Task GetToken(TokenArgs args)
        {
            var config = await GetConfiguration().ConfigureAwait(false);
            if (config == null)
            {
                await Console.Out.WriteLineAsync("Missing configuration. Did you run the `configure` action?").ConfigureAwait(false);
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
                        state)
                    { nonce = Guid.NewGuid().ToString("N"), response_mode = args.ResponseMode }).ConfigureAwait(false);
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
                            TokenRequest.FromAuthorizationCode(code, config.RedirectUrl, pkce.CodeVerifier)).ConfigureAwait(false);
                        if (tokenOption is Option<GrantedTokenResponse>.Result token)
                        {
                            var json = JsonConvert.SerializeObject(token.Item, Formatting.Indented);
                            await Console.Out.WriteLineAsync(json).ConfigureAwait(false);
                        }
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.OutputStream.WriteAsync(
                        "<html><head><title>Token flow completed</title><head><body><script>window.close();</script></body></html>"u8
                            .ToArray()).ConfigureAwait(false);
                    context.Response.Close();
                    listener.Stop();
                }

                process?.Close();
            }
        }
    }
}
