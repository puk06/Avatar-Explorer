namespace Avatar_Explorer.Utils;

internal static class CsvUtils
{
    /// <summary>
    /// 文字列をCSV形式にエスケープします。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
