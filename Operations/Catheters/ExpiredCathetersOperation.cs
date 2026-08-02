using DataLayer;
using Extensions;

namespace Operations.Catheters
{
    public class ExpiredCathetersOperation
    {
        private readonly SendCatheterNotificationOperation _notificationOperation;

        public ExpiredCathetersOperation(SendCatheterNotificationOperation notificationOperation)
        {
            _notificationOperation = notificationOperation;
        }

        public async Task Run()
        {
            var monthAgo = DateTime.Now.AddMonths(-1);

            using var db = new DataBaseContext();
            var expiredCatheters = db.MedSestriCatheters
                .Where(c => c.Date <= monthAgo && !c.IsChecked)
                .ToList();

            foreach (var catheter in expiredCatheters)
            {
                catheter.IsOverdue = true;
                await db.SaveChangesAsync();
                await _notificationOperation.Send(catheter);
            }

            WriteLog.Log($"Processed {expiredCatheters.Count} expired catheter appointments.");
        }
    }
}
