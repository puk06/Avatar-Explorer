namespace Avatar_Explorer.Models;

/// <summary>
/// 対応アバターかどうか、共通素体に含まれているか、どちらかを取得します。
/// </summary>
internal class SupportedOrCommonAvatar
{
    /// <summary>
    /// 対応アバターかどうかを取得または設定します。
    /// </summary>
    internal bool IsSupported { get; set; }

    /// <summary>
    /// 共通素体グループに含まれているかどうかを取得または設定します。
    /// </summary>
    internal bool IsCommon { get; set; }

    /// <summary>
    /// 対応アバターか共通素体に含まれているかどうかを取得します。
    /// </summary>
    internal bool IsSupportedOrCommon => IsSupported || IsCommon;

    /// <summary>
    /// 共通素体に含まれているが対応アバターではないかどうかを取得します。
    /// </summary>
    internal bool OnlyCommon => IsCommon && !IsSupported;

    /// <summary>
    /// もし共通素体グループに入っていれば、そのグループの名前を取得または設定します。
    /// </summary>
    internal string CommonAvatarName { get; set; } = string.Empty;
}
