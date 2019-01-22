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

namespace SimpleAuth.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using MiddleWare;
    using System;
    using SimpleAuth.Shared;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSimpleAuthExceptionHandler(this IApplicationBuilder applicationBuilder)
        {
            var publisher = applicationBuilder.ApplicationServices.GetService(typeof(IEventPublisher)) ?? new NoOpPublisher();
            return applicationBuilder.UseMiddleware<ExceptionHandlerMiddleware>(publisher);
        }

        public static IApplicationBuilder UseSimpleAuth(this IApplicationBuilder app, Action<SimpleAuthOptions> optionsCallback)
        {
            if (optionsCallback == null)
            {
                throw new ArgumentNullException(nameof(optionsCallback));
            }

            var hostingOptions = new SimpleAuthOptions();
            optionsCallback(hostingOptions);

            return app.UseSimpleAuthExceptionHandler();
        }
    }
}