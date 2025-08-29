using Avatar_Explorer.Models;

namespace Avatar_Explorer.Utils;

internal static class FilterUtils
{
    /// <summary>
    /// フィルター名を取得します。
    /// </summary>
    /// <param name="leftWindow"></param>
    /// <returns></returns>
    internal static string GetFilterName(LeftWindow leftWindow, string currentLanguage)
    {
        return leftWindow switch
        {
            LeftWindow.Nothing => LanguageUtils.Translate("非表示", currentLanguage),
            LeftWindow.Default => LanguageUtils.Translate("アバター", currentLanguage),
            LeftWindow.Author => LanguageUtils.Translate("作者", currentLanguage),
            LeftWindow.Category => LanguageUtils.Translate("カテゴリ別", currentLanguage),
            _ => "不明"
        };
    }
}
