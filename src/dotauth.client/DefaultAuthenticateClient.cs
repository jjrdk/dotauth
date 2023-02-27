namespace DotAuth.Client;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

/// <summary>
/// <para>Defines the default authenticate client.</para>
/// <para>This client listens on the callback <see cref="Uri"/> to receive the authentication code.</para>
/// </summary>
public class DefaultAuthenticateClient : AuthenticateClientBase
{
    /// <inheritdoc />
    public DefaultAuthenticateClient(AuthenticateClientOptions options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override async Task<string> Authenticate(Uri uri, Uri callback)
    {
        using var process = Process.Start(new ProcessStartInfo { FileName = uri.AbsoluteUri, UseShellExecute = true });
        var listener = new HttpListener();
        var authorityPart = callback.GetLeftPart(UriPartial.Authority);
        listener.Prefixes.Add($"{authorityPart}/");
        listener.Start();
        while (listener.IsListening)
        {
            var context = await listener.GetContextAsync().ConfigureAwait(false);
            var code = context.Request.QueryString.Get("code");
            
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.OutputStream.WriteAsync(
                    "<html><head><title>Token flow completed</title><head><body><script>window.close();</script></body></html>"u8
                        .ToArray())
                .ConfigureAwait(false);
            context.Response.Close();
            listener.Stop();

            return code ?? "";
        }

        return "";
    }
}