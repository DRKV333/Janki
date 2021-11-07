using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace JankiCards.Converters
{
    internal static class UnixDateTimeValueConverter
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

        private static readonly ValueConverter<DateTime, long> instance = new ValueConverter<DateTime, long>(
            x => ((DateTimeOffset)x).ToUnixTimeSeconds(),
            x => UnixEpoch.AddSeconds(x).ToLocalTime()
        );

        public static ValueConverter<DateTime, long> Instance => instance;
    }
}