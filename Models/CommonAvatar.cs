namespace Avatar_Explorer.Models;

/// <summary>
/// 共通素体グループを表します。
/// </summary>
public class CommonAvatar
{
    /// <summary>
    /// 共通素体グループの名前を取得または設定します。
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 共通素体のアバターのパスを取得または設定します。
    /// </summary>
    public List<string> Avatars { get; set; } = new List<string>();
}
