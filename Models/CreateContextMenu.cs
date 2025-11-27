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
    internal CreateContextMenu AddItem(string text, Image icon, EventHandler onClick, Keys keys = Keys.None)
    {
        if (keys != Keys.None) text += $"({keys})";

        var item = new ToolStripMenuItem(text, icon);
        item.Click += onClick;
        item.Disposed += (_, _) => item.Click -= onClick;

        if (keys != Keys.None)
        {
            _contextMenuStrip.KeyDown += (sender, e) =>
            {
                if (e.KeyCode != keys) return;

                _contextMenuStrip.Close();
                onClick?.Invoke(null, EventArgs.Empty);
            };
        }

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

    /// <summary>
    /// 既にあるメニューに対して、DropDownTextBoxを追加します
    /// </summary>
    /// <param name="baseToolStripMenuItem"></param>
    /// <param name="toolStripTextBox"></param>
    /// <param name="onKeyDown"></param>
    internal static void AddDropDownTextBox(ToolStripMenuItem baseToolStripMenuItem, ToolStripTextBox toolStripTextBox, KeyEventHandler onKeyDown)
    {
        toolStripTextBox.KeyDown += onKeyDown;
        toolStripTextBox.Disposed += (_, _) => toolStripTextBox.KeyDown -= onKeyDown;
        baseToolStripMenuItem.DropDownItems.Add(toolStripTextBox);
    }

    /// <summary>
    /// 既にあるメニューに対して、Separatorを追加します
    /// </summary>
    /// <param name="baseToolStripMenuItem"></param>
    internal static void AddSeparator(ToolStripMenuItem baseToolStripMenuItem)
    {
        baseToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
    }
}
