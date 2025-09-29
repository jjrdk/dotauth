// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace DotAuth.Authenticate;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Properties;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;

/// <summary>
/// Defines the authenticate client.
/// </summary>
internal sealed class AuthenticateClient
{
    private readonly ClientAssertionAuthentication _clientAssertionAuthentication;
    private readonly IClientStore _clientRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticateClient"/> class.
    /// </summary>
    /// <param name="clientRepository">The client repository.</param>
    /// <param name="jwksStore">The key store.</param>
    public AuthenticateClient(IClientStore clientRepository, IJwksStore jwksStore)
    {
        _clientAssertionAuthentication = new ClientAssertionAuthentication(clientRepository, jwksStore);
        _clientRepository = clientRepository;
    }

    /// <summary>
    /// Authenticates the specified instruction.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    /// <param name="issuerName">Name of the issuer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">instruction</exception>
    public async Task<AuthenticationResult> Authenticate(
        AuthenticateInstruction instruction,
        string issuerName,
        CancellationToken cancellationToken)
    {
        Client? client = null;
        // First we try to fetch the client_id
        // The different client authentication mechanisms are described here : http://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication
        var clientId = TryGettingClientId(instruction);
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            client = await _clientRepository.GetById(clientId, cancellationToken).ConfigureAwait(false);
        }

        if (client == null)
        {
            return new AuthenticationResult(null, SharedStrings.TheClientDoesntExist);
        }

        var errorMessage = string.Empty;
        switch (client.TokenEndPointAuthMethod)
        {
            case TokenEndPointAuthenticationMethods.ClientSecretBasic:
                client = instruction.AuthenticateClient(client);
                if (client == null)
                {
                    errorMessage = Strings.TheClientCannotBeAuthenticatedWithSecretBasic;
                }

                break;
            case TokenEndPointAuthenticationMethods.ClientSecretPost:
                client = ClientSecretPostAuthentication.AuthenticateClient(instruction, client);
                if (client == null)
                {
                    errorMessage = Strings.TheClientCannotBeAuthenticatedWithSecretPost;
                }

                break;
            case TokenEndPointAuthenticationMethods.ClientSecretJwt:
                if (client.Secrets.Any(s => s.Type == ClientSecretTypes.SharedSecret))
                {
                    return await _clientAssertionAuthentication
                        .AuthenticateClientWithClientSecretJwt(instruction, cancellationToken)
                        .ConfigureAwait(false);
                }

                errorMessage = string.Format(
                    Strings.TheClientDoesntContainASharedSecret,
                    client.ClientId);
                break;

            case TokenEndPointAuthenticationMethods.PrivateKeyJwt:
                return await _clientAssertionAuthentication
                    .AuthenticateClientWithPrivateKeyJwt(instruction, issuerName, cancellationToken)
                    .ConfigureAwait(false);
            case TokenEndPointAuthenticationMethods.TlsClientAuth:
                client = AuthenticateTlsClient(instruction, client);
                if (client == null)
                {
                    errorMessage = Strings.TheClientCannotBeAuthenticatedWithTls;
                }

                break;
        }

        return new AuthenticationResult(client, errorMessage);
    }

    private static Client? AuthenticateTlsClient(AuthenticateInstruction instruction, Client client)
    {
        var thumbPrint = client.Secrets.FirstOrDefault(s => s.Type == ClientSecretTypes.X509Thumbprint);
        var name = client.Secrets.FirstOrDefault(s => s.Type == ClientSecretTypes.X509Name);
        if (thumbPrint == null || name == null || instruction.Certificate == null)
        {
            return null;
        }

        var certificate = instruction.Certificate;
        var isSameThumbPrint = thumbPrint.Value == certificate.Thumbprint;
        var isSameName = name.Value == certificate.SubjectName.Name;
        return isSameName && isSameThumbPrint ? client : null;
    }

    /// <summary>
    /// Try to get the client id from the HTTP body or HTTP header.
    /// </summary>
    /// <param name="instruction">Authentication instruction</param>
    /// <returns>Client id</returns>
    private static string? TryGettingClientId(AuthenticateInstruction instruction)
    {
        var clientId = ClientAssertionAuthentication.GetClientId(instruction);
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            return clientId;
        }

        clientId = instruction.ClientIdFromAuthorizationHeader;
        return !string.IsNullOrWhiteSpace(clientId)
            ? clientId
            : instruction.ClientIdFromHttpRequestBody;
    }
}
