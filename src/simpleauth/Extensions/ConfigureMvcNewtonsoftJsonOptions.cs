namespace SimpleAuth.Extensions
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    internal class ConfigureMvcNewtonsoftJsonOptions : IConfigureOptions<MvcNewtonsoftJsonOptions>
    {
        public void Configure(MvcNewtonsoftJsonOptions options)
        {
            var settings = options.SerializerSettings;
            settings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Include;
            settings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
        }
    }
}