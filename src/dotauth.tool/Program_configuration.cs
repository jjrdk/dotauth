﻿namespace dotauth.tool;

using System.Text;
using System.Text.Json;
using DotAuth.Shared;

internal partial class Program
{
    private static async Task Configure(ConfigureArgs args)
    {
        var config = await GetConfiguration().ConfigureAwait(false) ?? new ToolConfig();
        if (string.Equals("microsoft", args.Authority, StringComparison.OrdinalIgnoreCase))
        {
            args.Authority = "https://login.microsoftonline.com/common/v2.0";
        }
        else if (string.Equals("google", args.Authority, StringComparison.OrdinalIgnoreCase))
        {
            args.Authority = "https://accounts.google.com";
        }

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

        await File.WriteAllTextAsync(configFile,
                JsonSerializer.Serialize(config, DefaultJsonSerializerOptions.Instance), Encoding.UTF8)
            .ConfigureAwait(false);

        await Console.Out.WriteLineAsync("Tool configured").ConfigureAwait(false);
        if (args.OutputResulting)
        {
            var options = DefaultJsonSerializerOptions.Instance;
            options.WriteIndented = true;
            var json = JsonSerializer.Serialize(config, options);
            await Console.Out.WriteLineAsync(json).ConfigureAwait(false);
        }
    }
}
