using DataLayer;
using Extensions;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;

namespace Operations.ImotBg
{
    public class ImotBgValidation
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly TimeSpan RequestDelay = TimeSpan.FromMilliseconds(300);

        public async Task ValidateData(DataBaseContext db)
        {
            var apartments = await db.ImotBgApartments
                .Where(ap => ap.IsActive == true)
                .ToListAsync();

            foreach (var ap in apartments)
            {
                var fullUrl = $"https://{ap.URl}";

                try
                {
                    using var response = await client.GetAsync(fullUrl);
                    response.EnsureSuccessStatusCode();

                    var wasRedirected = response.RequestMessage?.RequestUri?.AbsoluteUri != fullUrl;

                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    var imotBgHTML = Encoding.GetEncoding("windows-1251").GetString(bytes);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(imotBgHTML);

                    var hasUnavailableMessage = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'pageMessageAlert page980 MT20')]") != null;

                    if (wasRedirected || hasUnavailableMessage)
                    {
                        ap.IsActive = false;
                    }
                    else
                    {
                        ap.Error = null;
                    }

                    ap.UpdatedDate = DateTime.Now;
                    await db.SaveChangesAsync();
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    ap.IsActive = false;
                    ap.Error = $"{ex.Message} \n {ex.StackTrace}";
                    ap.UpdatedDate = DateTime.Now;
                    await db.SaveChangesAsync();

                    WriteLog.Log($"Apartment {ap.Id} returned 404, marked inactive.", fullUrl);
                }
                catch (Exception ex)
                {
                    ap.Error = $"{ex.Message} \n {ex.StackTrace}";
                    ap.UpdatedDate = DateTime.Now;
                    await db.SaveChangesAsync();

                    WriteLog.Log($"Validation failed for apartment {ap.Id}.", ex.Message, ex.StackTrace ?? string.Empty, fullUrl);
                }
                finally
                {
                    await Task.Delay(RequestDelay);
                }
            }

            WriteLog.Log("Successfully updated ImotBg Validation!");
        }
    }
}
