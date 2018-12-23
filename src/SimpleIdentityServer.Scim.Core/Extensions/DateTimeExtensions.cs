namespace SimpleIdentityServer.Scim.Core.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        public static double ToUnix(this DateTime dateTime)
        {
            var epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            var unixTicks = new TimeSpan(dateTime.Ticks) - epochTicks;
            return unixTicks.TotalSeconds;
        }

        public static DateTime ToDateTime(this double unixTime)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, DateTimeKind.Utc);
        }
    }
}
