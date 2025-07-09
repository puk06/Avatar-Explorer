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
    internal static void DragEnter(object _, DragEventArgs e) => e.Effect = DragDropEffects.All;
}
