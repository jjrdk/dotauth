﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Shared.Responses;

using System;
using System.Runtime.Serialization;

/// <summary>
/// Defines the OAuth introspection response.
/// </summary>
public sealed record OauthIntrospectionResponse : IntrospectionResponse
{
    /// <summary>
    /// Gets or sets a list of scopes
    /// </summary>
    [DataMember(Name = "scope")]
    public string[] Scope { get; set; } = Array.Empty<string>();
}