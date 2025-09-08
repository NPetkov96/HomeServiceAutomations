using DataLayer;
using System.Diagnostics;
using System.Text.Json;

namespace Operations.Ngrok
{
    public class NgrokOperations
    {
        private Process _ngrokProcess;

        public async Task Run()
        {
            using (var db = new DataBaseContext())
            {
                var httpsPort = db.Settings.FirstOrDefault(w => w.Name == "PortHTTPs")!.Value;

                var startInfo = new ProcessStartInfo
                {
                    FileName = "ngrok.exe",
                    Arguments = $"http https://localhost:{httpsPort} --url homeserver.ngrok.pro",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _ngrokProcess = Process.Start(startInfo);

                using var client = new HttpClient();
                string url = null;
                var retries = 10;
                while (retries-- > 0)
                {
                    try
                    {
                        var response = await client.GetAsync("http://127.0.0.1:4040/api/tunnels");
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            using var doc = JsonDocument.Parse(json);
                            url = doc.RootElement
                                .GetProperty("tunnels")[0]
                                .GetProperty("public_url")
                                .GetString()!;
                            if (!string.IsNullOrEmpty(url)) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(500);
                        continue;
                    }
                }

                if (url != null)
                {
                    db.Settings.FirstOrDefault(w => w.Name == "NgrokDNS")!.Value = url;
                    db.SaveChanges();
                }
            }
        }

        public async Task Kill()
        {
            var processes = Process.GetProcessesByName("ngrok");
            foreach (var proc in processes)
            {
                proc.Kill();
                await proc.WaitForExitAsync();
                proc.Dispose();
            }
        }
    }
}
