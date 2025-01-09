using Avatar_Explorer.Forms;

namespace Avatar_Explorer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.

                if (!File.Exists("./Datas/Fonts/NotoSansJP-Regular.ttf") ||
                    !File.Exists("./Datas/Fonts/NotoSans-Regular.ttf") ||
                    !File.Exists("./Datas/Fonts/NotoSansKR-Regular.ttf"))
                {
                    MessageBox.Show("必要なフォントがフォルダ内に存在しませんでした。ソフトをもう一度入れ直してください。", "エラー",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ApplicationConfiguration.Initialize();
                Application.Run(new Main());
            }
            catch (Exception ex)
            {
                MessageBox.Show("ソフトの起動中にエラーが発生しました。\n\n" + ex,
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}