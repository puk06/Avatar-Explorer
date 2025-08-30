using Avatar_Explorer.Forms;
using Avatar_Explorer.Models;
using System.Diagnostics;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            // 相対パスを取得し、カレントディレクトリを設定
            var currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess()?.MainModule?.FileName);
            if (currentDirectory != null) Directory.SetCurrentDirectory(currentDirectory);

            // Check if the schema is registered in the registry
            SchemeUtils.CheckScheme();

            var launchInfo = args.Length > 0 ? AEUtils.GetLaunchInfo(args[0]) : new LaunchInfo();
            ConfigurationManager configuration = new("settings.cfg");

            ApplicationConfiguration.Initialize();

#pragma warning disable WFO5001 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。

            if (configuration["DarkMode"] == "true") Application.SetColorMode(SystemColorMode.Dark);

#pragma warning restore WFO5001 // 種類は、評価の目的でのみ提供されています。将来の更新で変更または削除されることがあります。続行するには、この診断を非表示にします。

            Application.Run(new MainForm(launchInfo, configuration));
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                "ソフトの起動中にエラーが発生しました。\n\n" + ex,
                "エラー",
                true
            );
        }
    }
}
