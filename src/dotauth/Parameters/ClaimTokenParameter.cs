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

/// <summary>
/// Defines the claim token parameter
/// </summary>
public sealed record ClaimTokenParameter
{
    /// <summary>
    /// Gets the token format.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the token.
    /// </summary>
    public string? Token { get; init; }
}