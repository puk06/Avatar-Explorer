using Avatar_Explorer.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Avatar_Explorer.Utils;

internal static class BoothUtils
{
    private static readonly HttpClient _httpClient = new();
    private static readonly Dictionary<string[], ItemType> TITLE_MAPPINGS = new()
    {
        { new[] { "オリジナル3Dモデル", "オリジナル", "Avatar", "Original" }, ItemType.Avatar },
        { new[] { "アニメーション", "Animation" }, ItemType.Animation },
        { new[] { "衣装", "Clothing" }, ItemType.Clothing },
        { new[] { "ギミック", "Gimmick" }, ItemType.Gimmick },
        { new[] { "アクセサリ", "Accessory" }, ItemType.Accessory },
        { new[] { "髪", "Hair" }, ItemType.HairStyle },
        { new[] { "テクスチャ", "Eye", "Texture" }, ItemType.Texture },
        { new[] { "ツール", "システム", "Tool", "System" }, ItemType.Tool },
        { new[] { "シェーダー", "Shader" }, ItemType.Shader }
    };

    /// <summary>
    ///　Boothのアイテム情報を取得します。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static async Task<Item> GetBoothItemInfoAsync(string id)
    {
        var url = $"https://booth.pm/ja/items/{id}.json";
        var response = await _httpClient.GetStringAsync(url);
        var json = JObject.Parse(response);

        var title = json["name"]?.ToString() ?? "";
        var author = json["shop"]?["name"]?.ToString() ?? "";
        var authorUrl = json["shop"]?["url"]?.ToString() ?? "";
        var imageUrl = json["images"]?.Count() > 0 ? json["images"]?[0]?["original"]?.ToString() ?? "" : "";
        var authorIcon = json["shop"]?["thumbnail_url"]?.ToString() ?? "";
        var authorId = GetAuthorId(authorUrl);
        var category = json["category"]?["name"]?.ToString() ?? "";
        var estimatedCategory = GetItemType(title, category);

        return new Item
        {
            Title = title,
            AuthorName = author,
            ThumbnailUrl = imageUrl,
            AuthorImageUrl = authorIcon,
            AuthorId = authorId,
            Type = estimatedCategory
        };
    }

    private static string GetAuthorId(string url)
    {
        var match = Regex.Match(url, @"https://(.*)\.booth\.pm/");
        return match.Success ? match.Groups[1].Value : "";
    }


    /// <summary>
    /// アイテムのデフォルトタイプを推測、取得します。
    /// </summary>
    /// <param name="title"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private static ItemType GetItemType(string title, string type)
    {
        var suggestType = type switch
        {
            "3Dキャラクター" => ItemType.Avatar,
            "3Dモデル（その他）" => ItemType.Avatar,
            "3Dモーション・アニメーション" => ItemType.Animation,
            "3D衣装" => ItemType.Clothing,
            "3D小道具" => ItemType.Gimmick,
            "3D装飾品" => ItemType.Accessory,
            "3Dテクスチャ" => ItemType.Texture,
            "3Dツール・システム" => ItemType.Tool,
            _ => ItemType.Unknown
        };

        foreach (var mapping in TITLE_MAPPINGS)
        {
            if (mapping.Key.Any(title.Contains))
            {
                return mapping.Value;
            }
        }

        return suggestType;
    }


    /// <summary>
    /// アイテムのBOOTHリンクを開きます。
    /// </summary>
    /// <param name="item"></param>
    /// <param name="CurrentLanguage"></param>
    internal static void OpenItenBoothLink(Item item, string CurrentLanguage)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"https://booth.pm/{LanguageUtils.GetCurrentLanguageCode(CurrentLanguage)}/items/" +
                           item.BoothId,
                UseShellExecute = true
            });
        }
        catch
        {
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("リンクを開けませんでした。", CurrentLanguage),
                LanguageUtils.Translate("エラー", CurrentLanguage),
                true
            );
        }
    }

    /// <summary>
    /// アイテムのBOOTHリンクをクリップボードにコピーします。
    /// </summary>
    /// <param name="item"></param>
    /// <param name="CurrentLanguage"></param>
    internal static void CopyItemBoothLink(Item item, string CurrentLanguage)
    {
        try
        {
            Clipboard.SetText(
                $"https://booth.pm/{LanguageUtils.GetCurrentLanguageCode(CurrentLanguage)}/items/" +
                item.BoothId);
        }
        catch (Exception ex)
        {
            if (ex is ExternalException) return;
            FormUtils.ShowMessageBox(
                LanguageUtils.Translate("クリップボードにコピーできませんでした", CurrentLanguage),
                LanguageUtils.Translate("エラー", CurrentLanguage),
                true
            );
        }
    }
}
