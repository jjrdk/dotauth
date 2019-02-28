namespace SimpleAuth.Sms
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;

    /// <summary>
    /// Defines the route builder extensions
    /// </summary>
    public static class RoutingBuilderExtensions
    {
        /// <summary>
        /// Uses the SMS authentication.
        /// </summary>
        /// <param name="routeBuilder">The route builder.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">routeBuilder</exception>
        public static IRouteBuilder UseSmsAuthentication(this IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            routeBuilder.MapRoute("SmsAuthentication",
                "Authenticate/{action}/{id?}",
                new { controller = "Authenticate", action = "Index", area = SmsConstants.Amr },
                constraints: new { area = SmsConstants.Amr });
            return routeBuilder;
        }
    }
}
