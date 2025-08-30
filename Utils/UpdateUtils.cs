using Avatar_Explorer.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Avatar_Explorer.Utils;

internal static class UpdateUtils
{
    private static readonly HttpClient _httpClient = new();

    private static readonly string ItemURL = Properties.Resources.ItemURL;
    private static readonly string UpdateCheckURL = Properties.Resources.UpdateCheckURL;

    private static readonly ProcessStartInfo UrlProcessStartInfo = new()
    {
        FileName = ItemURL,
        UseShellExecute = true
    };

    internal async static Task CheckUpdate(string currentVersion, string currentLanguage)
    {
        try
        {
            string response = await _httpClient.GetStringAsync(UpdateCheckURL);
            VersionData? versionData = JsonSerializer.Deserialize<VersionData>(response);
            if (versionData == null) return;

            if (versionData.LatestVersion != currentVersion)
            {
                bool result = FormUtils.ShowConfirmDialog(
                    LanguageUtils.Translate("新しいAvatar Explorerのバージョンが利用可能です！\n\n「はい」をクリックすると商品ページを開きます。\n\n現在のバージョン: {0}\n最新のバージョン: {1}\n\n以下は最新版（{1}）の変更内容です。\n\n", currentLanguage, currentVersion, versionData.LatestVersion) +
                    string.Join("\n", versionData.ChangeLog.Select(log => $"・{log}")),
                    LanguageUtils.Translate("アップデートのお知らせ", currentLanguage)
                );

                if (result)
                {
                    OpenItemURL(currentLanguage);
                }
            }
        }
        catch
        {
            // Ignored
        }
    }

    private static void OpenItemURL(string currentLanguage)
    {
        try
        {
            Process.Start(UrlProcessStartInfo);
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("リンクを開くことができませんでした。\n{0}", currentLanguage, ex.Message),
                LanguageUtils.Translate("エラー", currentLanguage),
                true
            );
        }
    }
}
