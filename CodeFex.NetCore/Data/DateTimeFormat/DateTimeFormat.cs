using System;
using System.Collections.Generic;
using System.Globalization;

namespace CodeFex.NetCore.Data.DateTimeFormat
{
    public enum DateTimeFormatType
    {
        Iso8601_Simple,
        Iso8601_Sql,
        Iso8601_Local,
        Iso8601_Local_Precision,
        Iso8601_Utc,
        Iso8601_Utc_Precision,
        Iso8601_Utc_Default
    };

    public static class DateTimeFormatExtension
    {
        public static string ToString(this DateTime value, DateTimeFormatType dateTimeFormatType)
        {
            return value.ToString(DateTimeFormat.Formats[dateTimeFormatType]);
        }

        public static string ToString(this DateTime? value, DateTimeFormatType dateTimeFormatType)
        {
            return value != null ? value.Value.ToString(dateTimeFormatType) : null;
        }
    }

    public class DateTimeFormat
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public const string Iso8601_Simple = "yyyy-MM-dd HH:mm:ss";
        public const string Iso8601_Sql = "yyyy-MM-dd HH:mm:ss.fff";
        public const string Iso8601_Local = "yyyy-MM-dd'T'HH:mm:sszzz";
        public const string Iso8601_Local_Precision = "yyyy-MM-dd'T'HH:mm:ss.fffzzz";

        public const string Iso8601_Utc = "yyyy-MM-dd'T'HH:mm:ssZ";
        public const string Iso8601_Utc_Precision = "yyyy-MM-dd'T'HH:mm:ss.fffZ";
        public const string Iso8601_Utc_Default = Iso8601_Utc_Precision;

        public static readonly Dictionary<DateTimeFormatType, string> Formats = new Dictionary<DateTimeFormatType, string>()
        {
            { DateTimeFormatType.Iso8601_Simple,            Iso8601_Simple },
            { DateTimeFormatType.Iso8601_Sql,               Iso8601_Sql },
            { DateTimeFormatType.Iso8601_Local,             Iso8601_Local },
            { DateTimeFormatType.Iso8601_Local_Precision,   Iso8601_Local_Precision },
            { DateTimeFormatType.Iso8601_Utc,               Iso8601_Utc },
            { DateTimeFormatType.Iso8601_Utc_Precision,     Iso8601_Utc_Precision },
            { DateTimeFormatType.Iso8601_Utc_Default,       Iso8601_Utc_Default }
        };

        public static DateTime? TryParse(string value)
        {
            if (value == null) return null;

            // dynamic utc handling
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, value.EndsWith("Z") ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal, out var result))
            {
                return result;
            }
            else if (long.TryParse(value, out var epochValue))
            {
                return TryParse(epochValue);
            }

            return null;
        }

        public static DateTime? TryParse(long value)
        {
            try
            {
                var result = UnixEpoch.AddSeconds(value).ToLocalTime();

                if (result.Year != 1970)
                {
                    return result;
                }
            }
            catch { /* ignore */ }

            try
            {
                var result = UnixEpoch.AddMilliseconds(value).ToLocalTime();
            }
            catch { /* ignore */ }

            return null;
        }

        public static DateTime Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var result = TryParse(value);
            if (result != null)
            {
                return result.Value;
            }

            throw new Exception(string.Concat("Failed to read datetime: ", value));
        }
    }
}
