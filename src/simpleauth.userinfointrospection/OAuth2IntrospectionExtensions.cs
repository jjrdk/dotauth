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

namespace SimpleAuth.UserInfoIntrospection
{
    using System;
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// Defines the extensions.
    /// </summary>
    public static class OAuth2IntrospectionExtensions
    {
        /// <summary>
        /// Adds the user information introspection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddUserInfoIntrospection(this AuthenticationBuilder builder) =>
            builder.AddUserInfoIntrospection(_ => { });

        /// <summary>
        /// Adds the user information introspection.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configureOptions">The configure options.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddUserInfoIntrospection(
            this AuthenticationBuilder builder,
            Action<AuthenticationSchemeOptions> configureOptions) =>
            builder.AddScheme<AuthenticationSchemeOptions, UserInfoIntrospectionHandler>(
                UserIntrospectionDefaults.AuthenticationScheme,
                configureOptions);
    }
}
