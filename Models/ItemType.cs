namespace Avatar_Explorer.Models;

/// <summary>
/// アイテムの種類を表します。
/// </summary>
public enum ItemType
{
    /// <summary>
    /// アバター
    /// </summary>
    Avatar,

    /// <summary>
    /// 衣装
    /// </summary>
    Clothing,

    /// <summary>
    /// テクスチャ
    /// </summary>
    Texture,

    /// <summary>
    /// ギミック
    /// </summary>
    Gimmick,

    /// <summary>
    /// アクセサリー
    /// </summary>
    Accessory,

    /// <summary>
    /// 髪型
    /// </summary>
    HairStyle,

    /// <summary>
    /// アニメーション
    /// </summary>
    Animation,

    /// <summary>
    /// ツール
    /// </summary>
    Tool,

    /// <summary>
    /// シェーダー
    /// </summary>
    Shader,

    /// <summary>
    /// カスタムカテゴリー
    /// </summary>
    Custom,

    /// <summary>
    /// 不明
    /// </summary>
    Unknown
}
