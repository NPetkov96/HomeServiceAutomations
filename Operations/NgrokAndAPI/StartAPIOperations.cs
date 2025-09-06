using DataLayer;
using Extensions;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Operations.NgrokAndAPI
{
    public class StartAPIOperations
    {
        private Process _apiProcess;

        public async Task<int> Run()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\HomeService\Version_Current\Api\publish\HomeApi.exe",
                WorkingDirectory = @"C:\HomeService\Version_Current\Api\publish",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = "http://localhost:5555;https://localhost:7777";

            _apiProcess = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            _apiProcess.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            _apiProcess.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);

            _apiProcess.Start();
            _apiProcess.BeginOutputReadLine();
            _apiProcess.BeginErrorReadLine();

            await Task.Delay(3000);
            return (int)_apiProcess.Id;

        }

        public async Task GetListeningPortForProcess(int pid)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = "-ano",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var db = new DataBaseContext())
            using (var process = Process.Start(startInfo))
            {
                string? output = process?.StandardOutput.ReadToEnd();
                process?.WaitForExit();

                if (output == null) return;

                var lines = output.Split(Environment.NewLine);

                var counter = 0;
                foreach (var line in lines)
                {
                    if (line.Contains("LISTENING") && line.Contains(pid.ToString()))
                    {
                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {

                            var localAddress = parts[1];
                            var port = localAddress.Split(':').Last();
                            counter++;

                            WriteLog.Log($"Api Host is {port}");
                            if (counter == 1)
                            {
                                db.Settings.FirstOrDefault(w => w.Name == "PortHTTP")!.Value = port;
                                continue;
                            }
                            else if (counter == 2)
                            {
                                db.Settings.FirstOrDefault(w => w.Name == "PortHTTPs")!.Value = port;
                                db.SaveChanges();
                                return;
                            }
                        }
                    }
                }
            }

            return;
        }

        public async Task Kill(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch
            {
                return;
            }
        }
    }
}
