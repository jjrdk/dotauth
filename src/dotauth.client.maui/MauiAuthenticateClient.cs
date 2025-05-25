namespace DotAuth.Client.Maui;

using System;
using System.Threading.Tasks;
using DotAuth.Client;
using Microsoft.Maui.Authentication;

/// <summary>
/// <para>Defines the MAUI authenticate client.</para>
/// <para>The authenticate client uses the default WebView to complete the authentication flow.</para>
/// </summary>
public class MauiAuthenticateClient : AuthenticateClientBase
{
    /// <inheritdoc />
    public MauiAuthenticateClient(AuthenticateClientOptions options)
        : base(options)
    {
    }

    /// <inheritdoc />
    [System.Runtime.Versioning.UnsupportedOSPlatform("windows")]
    protected override async Task<string> Authenticate(Uri uri, Uri callback)
    {
        var authResult = await WebAuthenticator.AuthenticateAsync(uri, callback);
        return authResult.Properties["code"];
    }
}
