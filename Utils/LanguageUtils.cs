using System.Text.Json;

namespace Avatar_Explorer.Utils;

internal static class LanguageUtils
{
    private static readonly Dictionary<string, Dictionary<string, string>> TranslationData = new();

    /// <summary>
    /// 文字列を指定された言語に翻訳します。なければそのまま返します。
    /// </summary>
    /// <param name="str"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    internal static string Translate(string str, string to, params string[] parameters)
    {
        if (to == "ja-JP") return ReplaceParameters(str, parameters);
        if (!File.Exists($"./Translate/{to}.json")) return str;
        var data = GetTranslationData(to);
        var translatedData = data.TryGetValue(str, out var translated) ? translated : str;

        return ReplaceParameters(translatedData, parameters);
    }

    private static string ReplaceParameters(string text, string[] parameters)
        => parameters.Length > 0 ? string.Format(text, parameters) : text;

    /// <summary>
    /// 翻訳データを取得します。
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    private static Dictionary<string, string> GetTranslationData(string lang)
    {
        try
        {
            if (TranslationData.TryGetValue(lang, out var data)) return data;

            var json = File.ReadAllText($"./Translate/{lang}.json");
            var translationData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (translationData == null) return new Dictionary<string, string>();

            TranslationData.Add(lang, translationData);
            
            return translationData;
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
