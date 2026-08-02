using Extensions;
using Operations.BloodTetsUpdate;
using Operations.Catheters;
using Operations.ImotBg;

var jobName = args.FirstOrDefault() ?? Environment.GetEnvironmentVariable("HOME_JOB_NAME");

if (string.IsNullOrWhiteSpace(jobName))
{
    Console.Error.WriteLine("A job name is required: blood-tests, expired-catheters, or imot-bg.");
    return 2;
}

try
{
    WriteLog.Log($"Starting cloud job '{jobName}'.");

    switch (jobName.ToLowerInvariant())
    {
        case "blood-tests":
            await new UpdateBloodTestsOperation().Run();
            break;
        case "expired-catheters":
            await new ExpiredCathetersOperation(new SendCatheterNotificationOperation()).Run();
            break;
        case "imot-bg":
            await new DailyImotBgOperation(new ImotBgScraping(), new ImotBgValidation()).Run();
            break;
        default:
            Console.Error.WriteLine($"Unknown job '{jobName}'.");
            return 2;
    }

    WriteLog.Log($"Cloud job '{jobName}' completed successfully.");
    return 0;
}
catch (Exception ex)
{
    WriteLog.Log($"Cloud job '{jobName}' failed.", ex.Message, ex.StackTrace ?? string.Empty);
    return 1;
}
