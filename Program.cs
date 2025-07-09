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

            if (!File.Exists("./Datas/Fonts/NotoSansJP-Regular.ttf") ||
                !File.Exists("./Datas/Fonts/NotoSans-Regular.ttf") ||
                !File.Exists("./Datas/Fonts/NotoSansKR-Regular.ttf"))
            {
                FormUtils.ShowMessageBox(
                    "必要なフォントがフォルダ内に存在しませんでした。ソフトをもう一度入れ直してください。",
                    "エラー",
                    true
                );
                return;
            }

            // Check if the schema is registered in the registry
            SchemeUtils.CheckScheme();

            var launchInfo = args.Length > 0 ? AEUtils.GetLaunchInfo(args[0]) : new LaunchInfo();

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(launchInfo));
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
