using NCrontab;
using System.ComponentModel;

namespace HomeService.Services
{
    public abstract class ScheduledTask : IHostedService
    {

        protected CrontabSchedule schedule;
        protected DateTime nextRun;
        private CancellationToken cancellationToken = new CancellationToken();
        protected bool active = false;

        public ScheduledTask(string cronScheduledPattern, bool serviceActive = true)
        {
            schedule = CrontabSchedule.Parse(cronScheduledPattern, new CrontabSchedule.ParseOptions() { IncludingSeconds = true });
            nextRun = schedule.GetNextOccurrence(DateTime.Now);
            this.active = serviceActive;
        }

        protected abstract Task ExecuteTask();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!active) return Task.CompletedTask;

            BackgroundWorker backgroundWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
            };
            backgroundWorker.DoWork += async (object sender, DoWorkEventArgs args) =>
            {
                do
                {
                    if (DateTime.Now > this.nextRun)
                    {
                        try
                        {
                            await ExecuteTask();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                        this.nextRun = schedule.GetNextOccurrence(DateTime.Now);
                        Console.WriteLine(nextRun.ToString());
                    }
                    var delay = (int)((this.nextRun - DateTime.Now).TotalSeconds) * 1000;
                    if (delay < 0) delay = 0;
                    await Task.Delay(delay, cancellationToken);
                } while (!cancellationToken.IsCancellationRequested);
            };
            backgroundWorker.RunWorkerAsync();
            return Task.CompletedTask;

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
