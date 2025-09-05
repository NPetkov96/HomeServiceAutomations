using DataLayer;
using System.Text;

namespace Extensions
{
    public static class WriteLog
    {
        public static void Log(params string[] messages)
        {
            using (var db = new DataBaseContext())
            {
                var logPath = Path.Combine(db.Settings.FirstOrDefault(s => s.Name == "LogsPath")!.Value!, $"{DateTime.Now.ToString("yyyy-MM-dd")}-Logs.txt");
                var sb = new StringBuilder();

                foreach (var m in messages)
                {
                    sb.AppendLine(m);
                }
                File.AppendAllText(logPath, $"[{DateTime.Now}] Note: {sb}");
            }
        }
    }
}
