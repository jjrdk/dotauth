// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Client;

using SimpleAuth.Shared.Responses;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Defines the revoke token request.
/// </summary>
/// <seealso cref="IEnumerable{KeyValuePair}" />
public sealed record RevokeTokenRequest : IEnumerable<KeyValuePair<string?, string?>>
{
    private readonly Dictionary<string, string> _form;

    private RevokeTokenRequest(Dictionary<string, string> form)
    {
        _form = form;
    }

    /// <summary>
    /// Creates the request.
    /// </summary>
    /// <param name="tokenResponse">The token response.</param>
    /// <returns></returns>
    public static RevokeTokenRequest Create(GrantedTokenResponse tokenResponse)
    {
        return Create(tokenResponse.AccessToken, tokenResponse.TokenType);
    }

    /// <summary>
    /// Creates the request.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="tokenType">Type of the token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">token</exception>
    public static RevokeTokenRequest Create(string token, string tokenType)
    {
        var dict = new Dictionary<string, string>
        {
            {"token", token},
            {"token_type_hint", tokenType}
        };
        return new RevokeTokenRequest(dict);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string?, string?>> GetEnumerator()
    {
        return _form.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}