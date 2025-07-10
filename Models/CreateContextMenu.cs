namespace Avatar_Explorer.Models;

/// <summary>
/// ContextMenuの作成を楽にするクラスです
/// </summary>
internal class CreateContextMenu
{
    /// <summary>
    /// 現在作成されているContextMenuStripを取得します。
    /// </summary>
    internal ContextMenuStrip ContextMenuStrip => _contextMenuStrip;

    private readonly ContextMenuStrip _contextMenuStrip = new();

    /// <summary>
    /// アイテムを追加します
    /// </summary>
    /// <param name="text"></param>
    /// <param name="icon"></param>
    /// <param name="onClick"></param>
    /// <returns></returns>
    internal CreateContextMenu AddItem(string text, Image icon, EventHandler onClick)
    {
        var item = new ToolStripMenuItem(text, icon);
        item.Click += onClick;
        item.Disposed += (_, _) => item.Click -= onClick;
        _contextMenuStrip.Items.Add(item);

        return this;
    }

    /// <summary>
    /// アイテムを追加します
    /// </summary>
    /// <param name="text"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    internal ToolStripMenuItem AddItem(string text, Image icon)
    {
        var item = new ToolStripMenuItem(text, icon);
        _contextMenuStrip.Items.Add(item);

        return item;
    }

    /// <summary>
    /// 既にあるメニューに対して、DropDownアイテムを追加します
    /// </summary>
    /// <param name="baseToolStripMenuItem"></param>
    /// <param name="toolStripMenuItem"></param>
    /// <param name="onClick"></param>
    internal static void AddDropDownItem(ToolStripMenuItem baseToolStripMenuItem, ToolStripMenuItem toolStripMenuItem, EventHandler onClick)
    {
        toolStripMenuItem.Click += onClick;
        toolStripMenuItem.Disposed += (_, _) => toolStripMenuItem.Click -= onClick;
        baseToolStripMenuItem.DropDownItems.Add(toolStripMenuItem);
    }
}
