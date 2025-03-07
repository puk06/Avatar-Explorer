using Avatar_Explorer.Forms;
using Avatar_Explorer.Classes;
using System.Diagnostics;

namespace Avatar_Explorer
{
    internal static class Program
    {
        private static readonly string REG_PROTCOL = "VRCAE";
        private static readonly string SCHEME_FILE_PATH = "./Datas/VRCAESCHEME.txt";

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

                //相対パスを取得し、カレントディレクトリを設定
                var currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess()?.MainModule?.FileName);
                if (currentDirectory != null) Directory.SetCurrentDirectory(currentDirectory);

                if (!File.Exists("./Datas/Fonts/NotoSansJP-Regular.ttf") ||
                    !File.Exists("./Datas/Fonts/NotoSans-Regular.ttf") ||
                    !File.Exists("./Datas/Fonts/NotoSansKR-Regular.ttf"))
                {
                    MessageBox.Show("必要なフォントがフォルダ内に存在しませんでした。ソフトをもう一度入れ直してください。", "エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(SCHEME_FILE_PATH))
                {
                    var result = MessageBox.Show("カスタムURLスキームを登録しますか？\n\n" +
                                                 "登録すると、ブラウザから「" + REG_PROTCOL + "://」でこのソフトを起動できます。\n" +
                                                 "登録しない場合は、URLスキームでの起動はできませんが、通常の起動は可能です。",
                        "カスタムURLスキーム登録", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        string? exePath = Process.GetCurrentProcess()?.MainModule?.FileName;

                        if (exePath != null)
                        {
                            try
                            {
                                if (!Helper.IsRunAsAdmin())
                                {
                                    var result2 = MessageBox.Show("カスタムURLスキームの登録には管理者権限が必要です。\n" +
                                                                  "再起動して管理者権限で起動しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (result2 == DialogResult.Yes)
                                        Helper.RestartAsAdmin();
                                    return;
                                }

                                Helper.RegisterCustomScheme(REG_PROTCOL, exePath);
                                File.WriteAllText(SCHEME_FILE_PATH, exePath);
                                MessageBox.Show("カスタムURLスキームの登録に成功しました。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("カスタムURLスキームの登録に失敗しました。\n\n" + ex,
                                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("カスタムURLスキームの登録をスキップしました。\nもし登録したければ、Datasフォルダ内のVRCAESCHEME.txtを削除してもう一度起動してください！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        File.WriteAllText(SCHEME_FILE_PATH, "false");
                    }
                }
                else
                {
                    string path = File.ReadAllText(SCHEME_FILE_PATH);

                    string? exePath = Process.GetCurrentProcess()?.MainModule?.FileName;

                    if (path != "false" && exePath != null && path != exePath)
                    {
                        var result = MessageBox.Show("カスタムURLスキームの登録先が変更されているため、再登録しますか？\n\n" +
                                                     "登録すると、ブラウザから「" + REG_PROTCOL + "://」でこのソフトを起動できます。\n" +
                                                     "登録しない場合は、URLスキームでの起動はできませんが、通常の起動は可能です。",
                            "カスタムURLスキーム登録", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                if (!Helper.IsRunAsAdmin())
                                {
                                    var result2 = MessageBox.Show("カスタムURLスキームの登録には管理者権限が必要です。\n" +
                                                                  "再起動して管理者権限で起動しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                    if (result2 == DialogResult.Yes)
                                        Helper.RestartAsAdmin();
                                    return;
                                }

                                Helper.RegisterCustomScheme(REG_PROTCOL, exePath);
                                File.WriteAllText(SCHEME_FILE_PATH, exePath);
                                MessageBox.Show("カスタムURLスキームの登録に成功しました。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("カスタムURLスキームの登録に失敗しました。\n\n" + ex,
                                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else if (result == DialogResult.No)
                        {
                            MessageBox.Show("カスタムURLスキームの再登録をスキップしました。\nもしもう一度登録したければ、Datasフォルダ内のVRCAESCHEME.txtを削除してもう一度起動してください！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            File.WriteAllText(SCHEME_FILE_PATH, "false");
                        }
                    }
                }

                var launchInfo = args.Length > 0 ? Helper.GetLaunchInfo(args[0]) : new LaunchInfo();

                ApplicationConfiguration.Initialize();
                Application.Run(new Main(launchInfo));
            }
            catch (Exception ex)
            {
                MessageBox.Show("ソフトの起動中にエラーが発生しました。\n\n" + ex,
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}