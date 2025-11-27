using System.Text.Json.Serialization;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Models;

/// <summary>
/// アイテム情報を表します。
/// </summary>
public class Item
{
    /// <summary>
    /// アイテムのタイトルを取得または設定します。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの作者の名前を取得また設定します。
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのメモを取得または設定します。
    /// </summary>
    public string ItemMemo { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの作者のIDを取得または設定します。
    /// </summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのBooth IDを取得または設定します。
    /// </summary>
    public int BoothId { get; set; } = -1;

    /// <summary>
    /// アイテムフォルダがあるパスを取得または設定します。
    /// </summary>
    public string ItemPath { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのマテリアル用のフォルダのパスを取得または設定します。
    /// </summary>
    public string MaterialPath { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのサムネイルのURLを取得または設定します。
    /// </summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのサムネイルのファイルパスを取得または設定します。
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの作者のアイコンのURLを取得または設定します。
    /// </summary>
    public string AuthorImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの作者のアイコンのファイルパスを取得または設定します。
    /// </summary>
    public string AuthorImageFilePath { get; set; } = string.Empty;

    /// <summary>
    /// アイテムのタイプを取得または設定します。
    /// </summary>
    public ItemType Type { get; set; }

    /// <summary>
    /// もしタイプがカスタムカテゴリだった場合の、そのカスタムカテゴリ名を取得または設定します。
    /// </summary>
    public string CustomCategory { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの対応アバターを取得また設定します。
    /// </summary>
    public List<string> SupportedAvatar { get; set; } = new List<string>();

    /// <summary>
    /// アイテムの作成日時を取得または設定します。
    /// </summary>
    public string CreatedDate { get; set; } = string.Empty;

    /// <summary>
    /// アイテムの更新日時を取得または設定します。
    /// </summary>
    public string UpdatedDate { get; set; } = string.Empty;

    /// <summary>
    /// アイテムが実装済みかどうかを管理する配列を取得または設定します。
    /// </summary>
    public List<string> ImplementedAvatars { get; set; } = new List<string>();

    /// <summary>
    /// アイテムのタグを取得または設定します。
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// SupportedAvatarのエイリアスです。
    /// </summary>
    [JsonIgnore]
    public List<string> SupportedAvatars
        => SupportedAvatar;

    /// <summary>
    /// タイトルに含まれる括弧をbool値に応じて削除する機能です。
    /// </summary>
    public string GetTitle(bool removeBrackets)
        => removeBrackets ? AEUtils.RemoveBrackets(Title) : Title;
}
