using DataLayer;
using Extensions;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Operations.ImotBg
{
    public class ImotBgValidation
    {
        public async Task ValidateData(DataBaseContext db)
        {
            var client = new HttpClient();
            var apartments = await db.ImotBgApartments
                .Where(ap => ap.IsActive == true)
                .ToListAsync();

            foreach (var ap in apartments)
            {
                var fullUrl = $"https://{ap.URl}";

                try
                {
                    var bytes = await client.GetByteArrayAsync(fullUrl);
                    var imotBgHTML = Encoding.GetEncoding("windows-1251").GetString(bytes);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(imotBgHTML);

                    var isAvailable = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'pageMessageAlert page980 MT20')]") == null;

                    if (!isAvailable)
                    {
                        ap.IsActive = false;
                        ap.UpdatedDate = DateTime.Now;
                        await db.SaveChangesAsync();
                    }

                    ap.UpdatedDate = DateTime.Now;
                    await db.SaveChangesAsync();
                    Console.WriteLine($"{ap.Id} - {fullUrl}");
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Response status code does not indicate success: 404 (Not Found).")
                    {
                        ap.IsActive = false;
                    }

                    ap.Error = $"{ex.Message} \n {ex.StackTrace}";
                    ap.UpdatedDate = DateTime.Now;
                    await db.SaveChangesAsync();

                    //WriteLog.Log($"{ap.Id} {ex.Message}, {ex.StackTrace!} {fullUrl}");
                    Console.WriteLine($"{ap.Id} - {ex.Message}, {ex.StackTrace!} {fullUrl}");
                }
            }

            WriteLog.Log("Successfully updated ImotBg Validation!");
        }
    }
}
