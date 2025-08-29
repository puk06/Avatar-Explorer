namespace Avatar_Explorer.Utils;

internal static class FormUtils
{
    /// <summary>
    /// 親のToolStripを表示します。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal static void ShowParentToolStrip(object? sender, EventArgs e)
    {
        if (sender == null) return;
        if (((ToolStripMenuItem)sender).GetCurrentParent() is ToolStripDropDownMenu dropDown)
        {
            var ownerItem = dropDown.OwnerItem;
            if (ownerItem == null) return;
            dropDown.Show(ownerItem.Bounds.Location);
        }
    }

    /// <summary>
    /// DragEnterイベントを処理します。
    /// </summary>
    /// <param name="_"></param>
    /// <param name="e"></param>
    internal static void DragEnter(object? _, DragEventArgs e)
        => e.Effect = DragDropEffects.All;

    /// <summary>
    /// メッセージボックスを表示します。
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="error"></param>
    internal static void ShowMessageBox(string message, string title, bool error = false)
    {
        var messageType = error ? MessageBoxIcon.Error : MessageBoxIcon.Information;
        MessageBox.Show(message, title, MessageBoxButtons.OK, messageType);
    }

    /// <summary>
    /// 確認ダイアログを表示します。
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    internal static bool ShowConfirmDialog(string message, string title)
        => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
