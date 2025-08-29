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

            // ���΃p�X���擾���A�J�����g�f�B���N�g����ݒ�
            var currentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess()?.MainModule?.FileName);
            if (currentDirectory != null) Directory.SetCurrentDirectory(currentDirectory);

            // Check if the schema is registered in the registry
            SchemeUtils.CheckScheme();

            var launchInfo = args.Length > 0 ? AEUtils.GetLaunchInfo(args[0]) : new LaunchInfo();
            ConfigurationManager configuration = new();
            configuration.Load("settings.cfg");

            ApplicationConfiguration.Initialize();

#pragma warning disable WFO5001 // ��ނ́A�]���̖ړI�ł̂ݒ񋟂���Ă��܂��B�����̍X�V�ŕύX�܂��͍폜����邱�Ƃ�����܂��B���s����ɂ́A���̐f�f���\���ɂ��܂��B

            if (configuration["DarkMode"] == "true") Application.SetColorMode(SystemColorMode.Dark);

#pragma warning restore WFO5001 // ��ނ́A�]���̖ړI�ł̂ݒ񋟂���Ă��܂��B�����̍X�V�ŕύX�܂��͍폜����邱�Ƃ�����܂��B���s����ɂ́A���̐f�f���\���ɂ��܂��B

            Application.Run(new MainForm(launchInfo, configuration));
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox(
                "�\�t�g�̋N�����ɃG���[���������܂����B\n\n" + ex,
                "�G���[",
                true
            );
        }
    }
}
