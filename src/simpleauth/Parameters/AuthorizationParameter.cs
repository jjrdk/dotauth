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

namespace DotAuth.Parameters;

using System;

internal sealed record AuthorizationParameter
{
    public string? ClientId { get; init; }
    public string? Scope { get; init; }
    public string[] AmrValues { get; init; } = Array.Empty<string>();
    public string? ResponseType { get; init; }
    public Uri? RedirectUrl { get; init; }
    public string? State { get; init; }
    public string ResponseMode { get; init; } = null!;
    public string? Nonce { get; init; }
    public string? Prompt { get; init; }
    public double MaxAge { get; init; }
    public string? UiLocales { get; init; }
    public string? IdTokenHint { get; init; }
    public string? LoginHint { get; init; }
    public string? AcrValues { get; init; }
    public ClaimsParameter? Claims { get; init; }
    public string? CodeChallenge { get; init; }
    public string? CodeChallengeMethod { get; init; }
    public string? ProcessId { get; init; }
    public string? OriginUrl { get; init; }
    public string? SessionId { get; init; }
}