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

namespace DotAuth.Shared;

using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Implementation of base64 encoding &amp; decoding according to the RFC
/// https://tools.ietf.org/html/draft-ietf-jose-json-web-signature-41#appendix-C
/// </summary>
public static class StringExtensions
{
    /// <param name="entry">The entry.</param>
    extension(string entry)
    {
        /// <summary>
        /// Converts to sha256hash.
        /// </summary>
        /// <param name="salt">The hash salt.</param>
        /// <returns>The salted hashed value as a hex string.</returns>
        public string ToSha256Hash(string salt)
        {
            var entryBytes = Encoding.UTF8.GetBytes(entry + salt);
            var hash = SHA256.HashData(entryBytes);
            return Convert.ToHexString(hash).Replace("-", string.Empty);
        }

        /// <summary>
        /// Converts to sha256 as simplified base64.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public string ToSha256SimplifiedBase64(Encoding? encoding = null)
        {
            var enc = encoding ?? Encoding.UTF8;
            using var sha256 = SHA256.Create();
            var entryBytes = enc.GetBytes(entry);
            var hash = sha256.ComputeHash(entryBytes);
            return hash.ToBase64Simplified();
        }

        /// <summary>
        /// Base64 encode the passed string.
        /// </summary>
        /// <returns></returns>
        public string Base64Encode()
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(entry);
            return ToBase64Simplified(plainTextBytes);
        }
    }

    /// <summary>
    /// Create simplified base64 encoding.
    /// </summary>
    /// <param name="bytes">The bytes to encode.</param>
    /// <returns></returns>
    public static string ToBase64Simplified(this byte[]? bytes)
    {
        return bytes == null
            ? string.Empty
            : Convert.ToBase64String(bytes).Split('=')[0].Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    /// Base64 decode.
    /// </summary>
    /// <param name="base64EncodedData">The base64 encoded data.</param>
    /// <returns></returns>
    public static string Base64Decode(this string base64EncodedData)
    {
        var decodeBytes = base64EncodedData.Base64DecodeBytes();
        return Encoding.UTF8.GetString(decodeBytes);
    }

    /// <summary>
    /// Base64 decode.
    /// </summary>
    /// <param name="base64EncodedData">The base64 encoded data.</param>
    /// <returns></returns>
    public static byte[] Base64DecodeBytes(this string base64EncodedData)
    {
        var s = base64EncodedData.Trim().Replace(" ", "+").Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 0:
                return Convert.FromBase64String(s);
            case 2:
                s += "==";
                goto case 0;
            case 3:
                s += "=";
                goto case 0;
            default:
                throw new InvalidOperationException("Illegal base64url string!");
        }
    }
}
