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
    using System;

    public static class DateTimeExtensions
    {
        private static DateTime UnixStart;

        static DateTimeExtensions()
        {
            UnixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        }

        public static double ToUnix(this DateTime dateTime)
        {
            var epochTicks = new TimeSpan(UnixStart.Ticks);
            var unixTicks = new TimeSpan(dateTime.Ticks) - epochTicks;
            return unixTicks.TotalSeconds;
        }

        public static DateTime ToDateTime(this double unixTime)
        {
            var unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(UnixStart.Ticks + unixTimeStampInTicks, DateTimeKind.Utc);
        }

        public static DateTime ConvertFromUnixTimestamp(this double timestamp)
        {
            return UnixStart.AddSeconds(timestamp);
        }

        public static DateTime ConvertFromUnixTimestamp(this int timestamp)
        {
            return UnixStart.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(this DateTime date)
        {
            var diff = date.ToUniversalTime() - UnixStart;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}
