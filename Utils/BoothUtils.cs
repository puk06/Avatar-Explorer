using Avatar_Explorer.Models;
using Avatar_Explorer.Models.Booth;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Avatar_Explorer.Utils;

internal static partial class BoothUtils
{
    private static readonly HttpClient _httpClient = new();
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
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

    [GeneratedRegex(@"https://(.*)\.booth\.pm/")]
    private static partial Regex BoothAuthorURLRegex();

    /// <summary>
    ///　Boothのアイテム情報を取得します。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static async Task<Item> GetBoothItemInfoAsync(string id)
    {
        var url = $"https://booth.pm/ja/items/{id}.json";
        var response = await _httpClient.GetStringAsync(url);

        var itemJson = JsonSerializer.Deserialize<BoothItemResponse>(response, jsonSerializerOptions);

        var title = itemJson?.Name ?? string.Empty;
        var author = itemJson?.Shop?.Name ?? string.Empty;
        var authorUrl = itemJson?.Shop?.Url ?? string.Empty;
        var imageUrl = itemJson?.Images?.FirstOrDefault()?.Original ?? string.Empty;
        var authorIcon = itemJson?.Shop?.ThumbnailUrl ?? string.Empty;
        var authorId = GetAuthorId(authorUrl);
        var category = itemJson?.Category?.Name ?? string.Empty;
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

    /// <summary>
    /// 指定されたBooth IDのアイテムのサムネイルURLを取得します。
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static async Task<string> GetThumbnailURL(int id)
    {
        var url = $"https://booth.pm/ja/items/{id}.json";
        var response = await _httpClient.GetStringAsync(url);

        var itemJson = JsonSerializer.Deserialize<BoothItemResponse>(response, jsonSerializerOptions);

        return itemJson?.Images?.FirstOrDefault()?.Original ?? string.Empty;
    }

    /// <summary>
    /// 指定された画像URL先のファイルのバイト配列を取得します。
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    internal async static Task<byte[]> GetImageBytes(string url)
        => await _httpClient.GetByteArrayAsync(url);

    private static string GetAuthorId(string url)
    {
        var match = BoothAuthorURLRegex().Match(url);
        return match.Success ? match.Groups[1].Value : string.Empty;
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

        var matchedType = TITLE_MAPPINGS
            .Where(mapping => mapping.Key.Any(title.Contains))
            .Select(mapping => mapping.Value)
            .FirstOrDefault();

        return matchedType != ItemType.Unknown && matchedType != default ? matchedType : suggestType;
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
                FileName = $"https://booth.pm/{LanguageUtils.GetCurrentLanguageCode(CurrentLanguage)}/items/" + item.BoothId,
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
            Clipboard.SetText($"https://booth.pm/{LanguageUtils.GetCurrentLanguageCode(CurrentLanguage)}/items/" + item.BoothId);
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
