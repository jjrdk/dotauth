﻿// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Server.Tests.MiddleWares;

using System;
using Microsoft.AspNetCore.Authentication;

public static class FakeCustomAuthExtensions
{
    public static AuthenticationBuilder AddFakeCustomAuth(this AuthenticationBuilder builder, Action<TestAuthenticationOptions> configureOptions)
    {
        return builder.AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(FakeStartup.DefaultSchema, FakeStartup.DefaultSchema, configureOptions);
    }

    public static AuthenticationBuilder AddUmaCustomAuth(this AuthenticationBuilder builder, Action<AuthenticationSchemeOptions> configureOptions)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, UmaAuthenticationHandler>(FakeUmaStartup.DefaultSchema, FakeUmaStartup.DefaultSchema, configureOptions);
    }
}