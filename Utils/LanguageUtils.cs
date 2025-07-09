using System.Text.Json;

namespace Avatar_Explorer.Utils;

internal static class LanguageUtils
{
    private static readonly Dictionary<string, Dictionary<string, string>> TranslateData = new();

    /// <summary>
    /// 文字列を指定された言語に翻訳します。なければそのまま返します。
    /// </summary>
    /// <param name="str"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    internal static string Translate(string str, string to)
    {
        if (to == "ja-JP") return str;
        if (!File.Exists($"./Translate/{to}.json")) return str;
        var data = GetTranslateData(to);
        return data.TryGetValue(str, out var translated) ? translated : str;
    }

    /// <summary>
    /// 翻訳データを取得します。
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    private static Dictionary<string, string> GetTranslateData(string lang)
    {
        try
        {
            if (TranslateData.TryGetValue(lang, out var data)) return data;
            var json = File.ReadAllText(($"./Translate/{lang}.json"));
            var translateData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (translateData == null) return new Dictionary<string, string>();
            TranslateData.Add(lang, translateData);
            return translateData;
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// 言語名からBoothのリンクの言語コードを取得します。
    /// </summary>
    /// <param name="language"></param>
    /// <returns></returns>
    internal static string GetCurrentLanguageCode(string language = "")
    {
        return language switch
        {
            "ja-JP" => "ja",
            "ko-KR" => "ko",
            "en-US" => "en",
            _ => "ja"
        };
    }
}
