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

namespace DotAuth.Extensions;

using System;
using System.Text;
using System.Text.Json;
using DotAuth.Shared;
using Microsoft.AspNetCore.DataProtection;

internal static class DataProtectorExtensions
{
    public static T Unprotect<T>(this IDataProtector dataProtector, string encoded)
    {
        var unprotected = Unprotect(dataProtector, encoded);
        return JsonSerializer.Deserialize<T>(unprotected, DefaultJsonSerializerOptions.Instance)!;
    }

    private static string Unprotect(this IDataProtector dataProtector, string encoded)
    {
        var bytes = encoded.Base64DecodeBytes();
        var unprotectedBytes = dataProtector.Unprotect(bytes);
        return Encoding.ASCII.GetString(unprotectedBytes);
    }

    public static string Protect<T>(this IDataProtector dataProtector, T toEncode)
    {
        var serialized = JsonSerializer.Serialize(toEncode, DefaultJsonSerializerOptions.Instance);

        var bytes = Encoding.ASCII.GetBytes(serialized);
        var protectedBytes = dataProtector.Protect(bytes);
        return Convert.ToBase64String(protectedBytes);
    }
}
