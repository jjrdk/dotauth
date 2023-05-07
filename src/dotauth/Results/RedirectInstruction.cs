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

namespace DotAuth.Results;

using System;

internal sealed record RedirectInstruction
{
    public Parameter[] Parameters { get; init; } = Array.Empty<Parameter>();

    public DotAuthEndPoints Action { get; init; }

    public string? ResponseMode { get; init; }

    public RedirectInstruction AddParameter(string name, string? value)
    {
        var record = new Parameter(name, value ?? string.Empty);
        var newRecords = new Parameter[Parameters.Length + 1];
        Parameters.CopyTo(newRecords, 0);
        newRecords[^1] = record;
        return this with { Parameters = newRecords };
    }
}
