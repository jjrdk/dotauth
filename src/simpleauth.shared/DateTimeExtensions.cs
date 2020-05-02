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

namespace SimpleAuth.Shared
{
    using System;

    /// <summary>
    /// Defines the date time extensions.
    /// </summary>
    public static class DateTimeExtensions
    {
        private static readonly DateTimeOffset UnixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private const long TicksFactor = 10000000L;

        /// <summary>
        /// Convert the input <see cref="DateTimeOffset"/> to a unix timestamp.
        /// </summary>
        /// <param name="dateTime">The time to convert.</param>
        /// <returns></returns>
        public static long ConvertToUnixTimestamp(this DateTimeOffset dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks)
                   / TicksFactor;
        }

        /// <summary>
        /// Convert the input <see cref="DateTime"/> to a unix timestamp.
        /// </summary>
        /// <param name="date">The time to convert.</param>
        /// <returns></returns>
        public static long ConvertToUnixTimestamp(this DateTime date)
        {
            var diff = date.ToUniversalTime() - UnixStart;
            return (long) Math.Floor(diff.TotalSeconds);
        }

        /// <summary>
        /// Converts the unix timestamp to a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="value">The timestamp to convert.</param>
        /// <returns></returns>
        public static DateTimeOffset ConvertFromUnixTicks(this long? value)
        {
            return value.HasValue ? UnixStart.AddTicks(value.Value * TicksFactor) : default;
        }

        /// <summary>
        /// Converts the unix timestamp to a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="timestamp">The timestamp to convert.</param>
        /// <returns></returns>
        public static DateTimeOffset ConvertFromUnixTimestamp(this string timestamp)
        {
            return !long.TryParse(timestamp, out var value) ? UnixStart : ConvertFromUnixTicks(value);
        }
    }
}
