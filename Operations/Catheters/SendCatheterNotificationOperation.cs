using DataLayer.Models;
using Operations.Notifications;

namespace Operations.Catheters
{
    public class SendCatheterNotificationOperation
    {
        public async Task Send(MedSestriCatheter catheters)
        {
            var notificatioBody = new NotificationBody()
            {
                Message = new AndroidMessage()
                {
                    Notification = new NotificationContent()
                    {
                        Title = "Катетър за смяна:",
                        Body = $"{catheters.Address}"
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "navigate", "catheter" },
                        //{ "Id", "12" }
                    }

                }
            };

            SendNotification.Send(notificatioBody).Wait();
        }

    }
}
