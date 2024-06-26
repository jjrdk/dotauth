﻿namespace dotauth.tool
{
    using System.Text.Json;
    using CommandLine;
    using DotAuth.Shared;

    internal partial class Program
    {
        private static async Task Main(string[] args)
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
            var result = parser.ParseArguments<TokenArgs, ConfigureArgs, RefreshArgs>(args)
                .MapResult(
                    (TokenArgs tokenArgs) => GetToken(tokenArgs),
                    (ConfigureArgs configArgs) => Configure(configArgs),
                    (RefreshArgs refreshArgs) => Refresh(refreshArgs),
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

            return await JsonSerializer.DeserializeAsync<ToolConfig>(File.OpenRead(configFile),
                DefaultJsonSerializerOptions.Instance);
        }

        private static string GetConfigFilePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configFile = Path.Combine(appDataPath, "dotauth-tool", "config.json");
            return configFile;
        }
    }
}
