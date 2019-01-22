namespace SimpleAuth.Twilio
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using System;

    public static class RoutingBuilderExtensions
    {
        public static IRouteBuilder UseSmsAuthentication(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            routeBuilder.MapRoute("BasicAuthentication",
                "Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = SmsConstants.AMR },
                constraints: new { area = SmsConstants.AMR });
            return routeBuilder;
        }
    }
}
