// Copyright © 2017 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Client;

using System;
using System.Text;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the PKCE builder.
/// </summary>
public static class PkceBuilder
{
    private static readonly Random Random = new();

    /// <summary>
    /// Builds a PKCE challenge.
    /// </summary>
    /// <param name="method">The challenge method.</param>
    /// <returns>A <see cref="Pkce"/> instance.</returns>
    public static Pkce BuildPkce(this string method)
    {
        var codeVerifier = GetCodeVerifier();
        var codeChallenge = GetCodeChallenge(codeVerifier, method);
        return new Pkce(codeVerifier, codeChallenge);
    }

    private static string GetCodeVerifier()
    {
        const string possibleChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
        var nb = Random.Next(43, 128);
        var buffer = new char[nb];
        for (var i = 0; i < nb; i++)
        {
            buffer[i] = possibleChars[Random.Next(possibleChars.Length)];
        }

        return new string(buffer);
    }

    private static string GetCodeChallenge(string codeVerifier, string method)
    {
        return method == CodeChallengeMethods.Plain ? codeVerifier : codeVerifier.ToSha256SimplifiedBase64(Encoding.ASCII);
    }
}