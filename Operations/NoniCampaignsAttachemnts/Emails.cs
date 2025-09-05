using DataLayer;
using DataLayer.Models;
using Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Operations.NoniCampaignsAttachemnts
{
    public class Emails
    {
        public async Task ReadEmails()
        {
            using (var db = new DataBaseContext())
            {
                var settings = db.CampaignSettings.ToDictionary(s => s.Name!, s => s.Value)!;

                try
                {
                    var datetime = settings["LastDateTimeReadEmails"];

                    string[] Scopes = { GmailService.Scope.GmailModify, GmailService.Scope.GmailSend };
                    string ApplicationName = "Gmail API .NET Test";

                    UserCredential credential;


                    using (var stream =
                    new FileStream(settings["CredentialsPath"]!, FileMode.Open, FileAccess.Read))
                    {
                        string credPath = "C:\\NoniApp\\WorkHardNoni\\GmailEmailReaderService\\bin\\Debug\\net9.0\\token.json\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user";
                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                    }

                    var service = new GmailService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                    });

                    var request = service.Users.Messages.List("me");

                    var fromDate = DateTime.ParseExact(datetime, "yyyy-MM-dd-HH-mm-ss-f", CultureInfo.InvariantCulture);
                    var unixTime = ((DateTimeOffset)fromDate).ToUnixTimeSeconds();
                    request.Q = $"after:{unixTime} label:INBOX";

                    var messages = request.Execute().Messages;
                    if (messages == null) return;

                    string body = "";
                    foreach (var msg in messages)
                    {
                        var emailInfoReq = service.Users.Messages.Get("me", msg.Id);
                        emailInfoReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                        var emailInfoRes = emailInfoReq.Execute();
                        var part = emailInfoRes.Payload.Parts.FirstOrDefault(p => p.Filename.EndsWith(".xlsx"));

                        var subject = emailInfoRes.Payload.Headers
                                            .FirstOrDefault(h => h.Name.Equals("Subject", StringComparison.OrdinalIgnoreCase))!.Value ?? "Subject is empty";

                        if (part == null)
                        {
                            foreach (var item in emailInfoRes.Payload.Parts)
                            {
                                if (item.Parts == null) continue;
                                if (item.Parts.Any(at => at.Filename.EndsWith(".xlsx")))
                                {
                                    part = item.Parts.FirstOrDefault(at => at.Filename.EndsWith(".xlsx"));
                                }
                            }
                        }
                        if (part == null)
                        {
                            if (!db.CampaignEmails.Any(e => e.EmailId == msg.Id))
                            {
                                var model = new CampaignEmail()
                                {
                                    EmailId = msg.Id,
                                    ReceivedDate = DateTime.Now,
                                    IsCompleted = false,
                                    EmailStatus = Status.CouldNotExtractAttachment
                                };

                                db.CampaignEmails.Add(model);
                                db.SaveChanges();
                            }
                            continue;
                        }
                        string attachmentId = part.Body.AttachmentId;
                        string client = part.Filename.Replace(".xlsx", "").Trim();
                        var attachment = service.Users.Messages.Attachments.Get("me", msg.Id, attachmentId).Execute();

                        byte[] data = Convert.FromBase64String(attachment.Data.Replace("-", "+").Replace("_", "/"));

                        if (!db.CampaignEmails.Any(e => e.EmailId == msg.Id))
                        {
                            var model = new CampaignEmail()
                            {
                                EmailId = msg.Id,
                                Client = client,
                                CampaignId = subject,
                                Body = data,
                                ReceivedDate = DateTime.Now,
                                IsCompleted = false,
                                EmailStatus = Status.ExtractingAssets
                            };

                            db.CampaignEmails.Add(model);
                            db.SaveChanges();
                        }
                    }

                    db.CampaignSettings.FirstOrDefault(s => s.Name == "LastDateTimeReadEmails")!.Value = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-f");
                    db.SaveChanges();
                    return;
                }
                catch (Exception ex)
                {
                    WriteLog.Log(ex.Message, ex.StackTrace!);
                }

            }
        }

        public async Task SendEmailWithAttachment()
        {
            using (var db = new DataBaseContext())
            {
                //string toEmail = "nona@argent-bg.com";
                //string toEmail = "nikolaypetkow96@icloud.com";
                string toEmail = "nonaa.atanasova@gmail.com";
                string subject = "TOP SI NONI";

                //string bodyText = await GenerteBodyMessage();

                var settings = db.CampaignSettings.ToDictionary(s => s.Name!, s => s.Value)!;
                var records = db.CampaignEmails.Where(r => r.EmailStatus == Status.SendingFile).ToList();

                foreach (var record in records)
                {
                    string attachmentFilePath = record.AttachmentPath;

                    string[] Scopes = { GmailService.Scope.GmailSend };
                    string ApplicationName = "Gmail API .NET Send Email";

                    UserCredential credential;
                    using (var stream = new FileStream(settings["CredentialsPath"]!, FileMode.Open, FileAccess.Read))
                    {
                        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore("token.json", true));
                    }

                    var service = new GmailService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = ApplicationName,
                    });

                    var mimeMessage = new MimeMessage();
                    mimeMessage.From.Add(new MailboxAddress("NONA ATANASOVA", "workhardnona@gmail.com"));
                    mimeMessage.To.Add(new MailboxAddress("", toEmail));
                    mimeMessage.Subject = subject;

                    var builder = new BodyBuilder
                    {
                        //TextBody = bodyText
                    };

                    if (!string.IsNullOrEmpty(attachmentFilePath.ToString()) && File.Exists(attachmentFilePath.ToString()))
                    {
                        builder.Attachments.Add(attachmentFilePath.ToString());
                    }

                    mimeMessage.Body = builder.ToMessageBody();

                    using (var stream = new MemoryStream())
                    {
                        mimeMessage.WriteTo(stream);
                        var encodedMessage = Convert.ToBase64String(stream.ToArray())
                            .Replace("+", "-")
                            .Replace("/", "_")
                            .Replace("=", "");

                        var message = new Message
                        {
                            Raw = encodedMessage
                        };

                        await service.Users.Messages.Send(message, "me").ExecuteAsync();
                    }

                    WriteLog.Log("Email send successful");
                    record.IsCompleted = true;
                    record.EmailStatus = Status.Completed;
                    db.SaveChanges();
                }
            }
        }
        private static async Task<string> GenerteBodyMessage()
        {
            var apiKey = "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";
            var apiUrl = "https://api.openai.com/v1/chat/completions";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                       {
                new { role = "user", content = "Генерирай ми Body на имейл" }
            }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            string reply = "";

            if (root.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0 && choices[0].TryGetProperty("message", out JsonElement message) && message.TryGetProperty("content", out JsonElement replayContent))
            {
                reply = replayContent.GetString() ?? "";
            }

            return reply;
        }
    }
}
