namespace Avatar_Explorer.Utils;

internal static class DarkModeUtils
{
    /// <summary>
    /// コントロールをダークモード仕様にします。Button、Panel、TextBoxに対応しています。
    /// </summary>
    /// <param name="targetObject"></param>
    internal static void SetDarkMode(object targetObject)
    {
        if (targetObject is Button button)
        {
            button.BackColor = Color.FromArgb(44, 44, 44);
            button.ForeColor = Color.FromArgb(224, 224, 224);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
        }
        else if (targetObject is Panel panel)
        {
            panel.BackColor = Color.FromArgb(30, 30, 30);
        }
        else if (targetObject is TextBox textBox)
        {
            textBox.BackColor = Color.FromArgb(24, 24, 24);
            textBox.ForeColor = Color.FromArgb(224, 224, 224);
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }
    }

    /// <summary>
    /// 選択時のボタンの色です
    /// </summary>
    /// <param name="darkMode"></param>
    /// <returns></returns>
    internal static Color GetSelectedButtonColor(bool darkMode) => darkMode ? Color.Green : Color.LightGreen;
    
    /// <summary>
    /// 未選択時のボタンの色です
    /// </summary>
    /// <param name="darkMode"></param>
    /// <returns></returns>
    internal static Color GetUnSelectedButtonColor(bool darkMode) => darkMode ? Color.IndianRed : Color.LightPink;
    
    /// <summary>
    /// 通常のボタンの色です
    /// </summary>
    /// <param name="darkMode"></param>
    /// <returns></returns>
    internal static Color GetNormalButtonColor(bool darkMode) => darkMode ? Color.FromArgb(44, 44, 44) : Color.FromKnownColor(KnownColor.Control);
}
