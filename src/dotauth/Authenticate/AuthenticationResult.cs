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

using DotAuth.Shared.Models;

internal sealed class AuthenticationResult
{
    public AuthenticationResult(Client? client, string? errorMessage, bool isInvalidRequest = false)
    {
        Client = client;
        ErrorMessage = errorMessage;
        IsInvalidRequest = isInvalidRequest;
    }

    public Client? Client { get; }

    public string? ErrorMessage { get; }

    /// <summary>
    /// When <see langword="true"/> the failure should be surfaced as <c>invalid_request</c>
    /// rather than the default <c>invalid_client</c>.
    /// </summary>
    public bool IsInvalidRequest { get; }
}