using System;

namespace CodeFex.NetCore.Data.DateTimeFormat
{
    public class DatetimeFormatter
    {
        public string WriteFormat { get; protected set; }
        public bool ReadUtc { get; set; }
        public bool WriteUtc { get; protected set; }

        public DatetimeFormatter(string format = DateTimeFormat.Iso8601_Local)
        {
            Compile(format);
        }

        public DatetimeFormatter Compile(string writeFormat)
        {
            WriteFormat = writeFormat ?? throw new ArgumentNullException(nameof(writeFormat));
            WriteUtc = WriteFormat.EndsWith("Z");

            return this;
        }

        public string Write(DateTime datetime)
        {
            if (WriteUtc && datetime.Kind != DateTimeKind.Utc)
            {
                datetime = datetime.ToUniversalTime();
            }

            // assume local or unspecified
            return datetime.ToString(WriteFormat);
        }

        public DateTime? TryRead(string value)
        {
            return DateTimeFormat.TryParse(value);
        }

        public DateTime? TryRead(long value)
        {
            return DateTimeFormat.TryParse(value);
        }
    }
}
