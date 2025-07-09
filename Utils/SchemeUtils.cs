using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace Avatar_Explorer.Utils;

internal static class SchemeUtils
{
    private static readonly string REG_PROTCOL = "VRCAE";
    private static readonly string SCHEME_FILE_PATH = "./Datas/VRCAESCHEME.txt";

    /// <summary>
    /// カスタムURLスキームの登録用のヘルパー関数です。
    /// </summary>
    internal static void CheckScheme()
    {
        var isSchemeRegistered = IsSchemeRegistered(REG_PROTCOL);
        string? exePath = Process.GetCurrentProcess()?.MainModule?.FileName;

        if (exePath == null)
            return;

        if (!File.Exists(SCHEME_FILE_PATH))
        {
            if (!isSchemeRegistered)
            {
                if (PromptUserToRegisterScheme())
                    RegisterSchemeFlow(exePath);
                else
                    MarkSchemeSkipped();
            }
            else
            {
                if (PromptUserToReRegister("既に登録されていますが、内部的に未登録です。再登録しますか？"))
                    RegisterSchemeFlow(exePath);
                else
                    MarkSchemeSkipped();
            }
        }
        else
        {
            string path = File.ReadAllText(SCHEME_FILE_PATH);
            if (path != "false" && path != exePath)
            {
                if (PromptUserToReRegister("登録先が変更されているため、再登録しますか？"))
                    RegisterSchemeFlow(exePath);
                else
                    MarkSchemeSkipped();
            }
        }
    }

    private static bool PromptUserToRegisterScheme()
    {
        var result = MessageBox.Show(
            $"カスタムURLスキームを登録しますか？\n\n" +
            $"登録すると、ブラウザから「{REG_PROTCOL}://」でこのソフトを起動できます。\n" +
            $"登録しない場合は、URLスキームでの起動はできませんが、通常の起動は可能です。",
            "カスタムURLスキーム登録",
            MessageBoxButtons.YesNo, MessageBoxIcon.Information
        );

        return result == DialogResult.Yes;
    }

    private static bool PromptUserToReRegister(string reason)
    {
        var result = MessageBox.Show(
            $"カスタムURLスキームは{reason}\n\n" +
            $"登録すると、ブラウザから「{REG_PROTCOL}://」でこのソフトを起動できます。\n" +
            $"登録しない場合は、URLスキームでの起動はできませんが、通常の起動は可能です。",
            "カスタムURLスキーム登録",
            MessageBoxButtons.YesNo, MessageBoxIcon.Information
        );

        return result == DialogResult.Yes;
    }

    private static void RegisterSchemeFlow(string exePath)
    {
        try
        {
            if (!IsRunAsAdmin())
            {
                var result = MessageBox.Show(
                    "カスタムURLスキームの登録には管理者権限が必要です。\n" +
                    "再起動して管理者権限で起動しますか？",
                    "確認",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information
                );
                if (result == DialogResult.Yes)
                    RestartAsAdmin();
                return;
            }

            RegisterCustomScheme(REG_PROTCOL, exePath);
            File.WriteAllText(SCHEME_FILE_PATH, exePath);

            var exitResult = MessageBox.Show(
                "カスタムURLスキームの登録に成功しました。\n" +
                "ソフトを終了して、通常のユーザーとして起動することをおすすめします！\n\n" +
                "終了しないと、ソフト内のD&Dなどが正常に動作しない場合があります。\n" +
                "終了しますか？",
                "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information
            );

            if (exitResult == DialogResult.Yes)
                Environment.Exit(0);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "カスタムURLスキームの登録に失敗しました。\n\n" + ex,
                "エラー",
                MessageBoxButtons.OK, MessageBoxIcon.Error
            );
        }
    }

    private static void MarkSchemeSkipped()
    {
        File.WriteAllText(SCHEME_FILE_PATH, "false");
        MessageBox.Show(
            "カスタムURLスキームの登録をスキップしました。\n" +
            "もし登録したければ、Datasフォルダ内のVRCAESCHEME.txtを削除してもう一度起動してください！",
            "情報",
            MessageBoxButtons.OK, MessageBoxIcon.Information
        );
    }


    /// <summary>
    /// カスタムスキームを登録します。
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="exePath"></param>
    private static void RegisterCustomScheme(string protocol, string exePath)
    {
        using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(protocol))
        {
            key.SetValue("", "URL:" + protocol + " Protocol");
            key.SetValue("URL Protocol", "");
        }

        string commandKey = $@"{protocol}\shell\open\command";
        using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(commandKey))
        {
            key.SetValue("", $"\"{exePath}\" \"%1\"");
        }
    }

    /// <summary>
    /// 既にカスタムスキームが登録されているかどうかを取得します。
    /// </summary>
    /// <param name="protocol"></param>
    /// <returns></returns>
    private static bool IsSchemeRegistered(string protocol)
    {
        using RegistryKey? key = Registry.ClassesRoot.OpenSubKey(protocol);
        return key != null;
    }

    /// <summary>
    /// ソフトを管理者権限で起動しているかどうかを取得します。
    /// </summary>
    /// <returns></returns>
    private static bool IsRunAsAdmin()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// 管理者権限で再起動します。
    /// </summary>
    private static void RestartAsAdmin()
    {
        var exePath = Process.GetCurrentProcess()?.MainModule?.FileName;
        if (string.IsNullOrEmpty(exePath))
        {
            MessageBox.Show("再起動に失敗しました。手動で管理者としてソフトを実行してください！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ProcessStartInfo processStartInfo = new()
        {
            FileName = exePath,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(processStartInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            MessageBox.Show("再起動に失敗しました。手動で管理者としてソフトを実行してください。\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
    }
}
