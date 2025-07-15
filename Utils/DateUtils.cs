namespace Avatar_Explorer.Utils;

internal class DateUtils
{
    /// <summary>
    /// UnixTimeから日付文字列を取得します。
    /// </summary>
    /// <param name="unixTime"></param>
    /// <returns></returns>
    internal static string GetDateStringFromUnixTime(string unixTime)
    {
        if (string.IsNullOrEmpty(unixTime)) return "Invalid Date";

        if (long.TryParse(unixTime, out var unixTimeLong))
        {
            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeLong)
                                             .ToLocalTime()
                                             .DateTime;
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }

        return "Invalid Date";
    }

    /// <summary>
    /// UnixTimeを取得します。
    /// </summary>
    /// <returns></returns>
    internal static string GetUnixTime()
        => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

    internal static DateTime GetDate(string date)
    {
        try
        {
            if (date.All(char.IsDigit)) return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(date)).UtcDateTime;

            var allDigits = string.Empty;
            foreach (var c in date)
            {
                if (char.IsDigit(c)) allDigits += c;
            }

            if (allDigits.Length != 14) return DateTime.Now;

            var year = allDigits.Substring(0, 4);
            var month = allDigits.Substring(4, 2);
            var day = allDigits.Substring(6, 2);
            var hour = allDigits.Substring(8, 2);
            var minute = allDigits.Substring(10, 2);
            var second = allDigits.Substring(12, 2);

            var dateTime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute),
                int.Parse(second));

            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.Local);

            return utcDateTime;
        }
        catch
        {
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, TimeZoneInfo.Local);
        }
    }
}
