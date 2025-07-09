namespace Avatar_Explorer.Utils;

internal static class LogUtils
{

    /// <summary>
    /// 渡されたエラーを記録します。
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    internal static void ErrorLogger(string message, Exception exception)
    {
        try
        {
            var currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            File.AppendAllText("./ErrorLog.txt", currentTime + " - " + message + "\n" + exception + "\n\n");
        }
        catch
        {
            Console.WriteLine("Failed to write error log.");
        }
    }
}
