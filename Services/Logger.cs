using System.IO;
using System.Text;

namespace CopyPush.Services;

/// <summary>
/// Минимальный журнал событий - помогает понять, сработал ли перехват.
/// Файл: %LOCALAPPDATA%\CopyPush\CopyPush.log, с ограничением размера.
/// </summary>
internal static class Logger
{
    private static readonly object Gate = new();
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CopyPush",
        "CopyPush.log");

    private const long MaxBytes = 512 * 1024;

    public static void Info(string message)
    {
        Write("INFO", message);
    }

    public static void Error(string message, Exception? ex = null)
    {
        Write("ERROR", ex is null ? message : $"{message} :: {ex.GetType().Name}: {ex.Message}");
    }

    private static void Write(string level, string message)
    {
        try
        {
            lock (Gate)
            {
                var dir = System.IO.Path.GetDirectoryName(Path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                if (File.Exists(Path) && new FileInfo(Path).Length > MaxBytes)
                {
                    File.Delete(Path);
                }

                var sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.Append(" [").Append(level).Append("] ");
                sb.AppendLine(message);

                File.AppendAllText(Path, sb.ToString(), Encoding.UTF8);
            }
        }
        catch
        {
            // Журнал не должен никогда ломать приложение.
        }
    }
}
