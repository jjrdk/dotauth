namespace dotauth.client.maui;

using System;
using System.Threading.Tasks;
using DotAuth.Client;

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
    protected override async Task<string> Authenticate(Uri uri, Uri callback)
    {
        var authResult = await WebAuthenticator.AuthenticateAsync(uri, callback);
        return authResult.Properties["code"];
    }
}