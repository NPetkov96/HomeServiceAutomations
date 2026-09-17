using DataLayer;
using System.Text;

namespace Extensions
{
    public static class WriteLog
    {
        public static void Log(params string[] messages)
        {
            var text = string.Join(Environment.NewLine, messages);
            Console.WriteLine("[{0:u}] {1}", DateTime.UtcNow, text);

            try
            {
                var logsDirectory = Environment.GetEnvironmentVariable("HOME_SERVICE_LOGS_PATH");
                var isContainer = string.Equals(
                    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrWhiteSpace(logsDirectory) && !isContainer)
                {
                    using var db = DataBaseContext.Create();
                    logsDirectory = db.Settings.FirstOrDefault(s => s.Name == "LogsPath")?.Value;
                }

                if (string.IsNullOrWhiteSpace(logsDirectory))
                {
                    return;
                }

                Directory.CreateDirectory(logsDirectory);
                var logPath = Path.Combine(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}-Logs.txt");
                File.AppendAllText(logPath, $"[{DateTime.Now:u}] {text}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to write file log: {0}", ex.Message);
            }
        }
    }
}
