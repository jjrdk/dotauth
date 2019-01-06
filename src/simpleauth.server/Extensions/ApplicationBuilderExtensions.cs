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

namespace SimpleAuth.Server.Extensions
{
    using System;
    using Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using MiddleWare;

    public static class ApplicationBuilderExtensions
    {
        public static void UseSimpleAuth(this IApplicationBuilder app, Action<SimpleAuthOptions> optionsCallback, ILoggerFactory loggerFactory)
        {
            if (optionsCallback == null)
            {
                throw new ArgumentNullException(nameof(optionsCallback));
            }

            var hostingOptions = new SimpleAuthOptions();
            optionsCallback(hostingOptions);
            app.UseSimpleAuth(hostingOptions,
                loggerFactory);
        }

        public static void UseSimpleAuth(this IApplicationBuilder app, SimpleAuthOptions options, ILoggerFactory loggerFactory)
        {
            UseSimpleAuth(app, options);
        }

        public static void UseSimpleAuth(this IApplicationBuilder app, SimpleAuthOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            app.UseSimpleAuthExceptionHandler(new ExceptionHandlerMiddlewareOptions
            {
                OpenIdEventSource = app.ApplicationServices.GetService<IOpenIdEventSource>()
            });
            var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            UriHelperExtensions.Configure(httpContextAccessor);
        }
    }
}