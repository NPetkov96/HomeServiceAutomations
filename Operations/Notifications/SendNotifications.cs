using DataLayer;
using DataLayer.Models.Common;
using Extensions;
using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Operations.Notifications
{
    public static class SendNotification
    {

        public static async Task Send(NotificationBody body)
        {
            await CreateAsync();

            using (var db = new DataBaseContext())
            using (var httpClient = new HttpClient())
            {
                var token = db.Settings.FirstOrDefault(s => s.Name == "NotificationOAuth2Token")?.Value;
                var url = db.Settings.FirstOrDefault(s => s.Name == "NotificationUrl")?.Value;
                var jsonMessage = JsonSerializer.Serialize(body);

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    WriteLog.Log($"Failed to send notification: {response.StatusCode}, {error}");
                }
            }
        }

        private static async Task CreateAsync()
        {
            using (var db = new DataBaseContext())
            {
                var jsonPath = db.Settings.FirstOrDefault(s => s.Name == "NotificationJSONFilePath")?.Value;

                GoogleCredential credential;
                using (var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential
                        .FromStream(stream)
                        .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                }

                var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                var tokenSetting = db.Settings.FirstOrDefault(s => s.Name == "NotificationOAuth2Token");
                tokenSetting!.Value = token;
                db.SaveChanges();

            }
        }
    }
}
