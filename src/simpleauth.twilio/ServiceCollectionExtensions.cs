namespace SimpleAuth.Twilio
{
    using System;
    using Actions;
    using Controllers;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Services;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTwoFactorSmsAuthentication(this IServiceCollection services, TwoFactorTwilioOptions twilioOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (twilioOptions == null)
            {
                throw new ArgumentNullException(nameof(twilioOptions));
            }

            services.AddSingleton(twilioOptions);
            services.AddTransient<ITwoFactorAuthenticationService, DefaultTwilioSmsService>();
            return services;
        }

        public static IServiceCollection AddSmsAuthentication(this IServiceCollection services, IMvcBuilder mvcBuilder, SmsAuthenticationOptions smsAuthenticationOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (mvcBuilder == null)
            {
                throw new ArgumentNullException(nameof(mvcBuilder));
            }

            if (smsAuthenticationOptions == null)
            {
                throw new ArgumentNullException(nameof(smsAuthenticationOptions));
            }

            var assembly = typeof(AuthenticateController).Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly);
            services.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(embeddedFileProvider);
                //opts.CompilationCallback = (context) =>
                //{
                //    var assm = MetadataReference.CreateFromFile(Assembly.Load("").Location);
                //    context.Compilation = context.Compilation.AddReferences(assm);
                //};
            });
            services.AddSingleton(smsAuthenticationOptions);
            //services.AddSingleton<ISubjectBuilder, DefaultSubjectBuilder>();
            services.AddSingleton<ITwilioClient, TwilioClient>();
            services.AddTransient<ISmsAuthenticationOperation, SmsAuthenticationOperation>();
            services.AddTransient<IGenerateAndSendSmsCodeOperation, GenerateAndSendSmsCodeOperation>();
            services.AddTransient<IAuthenticateResourceOwnerService, SmsAuthenticateResourceOwnerService>();
            mvcBuilder.AddApplicationPart(assembly);
            return services;
        }
    }
}
