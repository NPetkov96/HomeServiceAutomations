using DataLayer;
using DataLayer.Models.ImotBg;
using HtmlAgilityPack;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace Operations.ImotBg
{
    public class ImotBgOperations
    {
        public async Task Run(DataBaseContext db)
        {
            try
            {
                var client = new HttpClient();
                var settings = db.ImotBgSettings.ToDictionary(x => x.Name, x => x.Value);

                var city = settings["SerachCity"];
                var neighborhood = settings["SerachNeighbour"].Split(",");

                for (int i = 0; i < neighborhood.Count(); i++)
                {
                    var currentNeighbiurhood = neighborhood[i];
                    var url = $"https://www.imot.bg/obiavi/prodazhbi/grad-{city}/{currentNeighbiurhood}";

                    var bytes = await client.GetByteArrayAsync(url);
                    var imotBgHTML = Encoding.GetEncoding("windows-1251").GetString(bytes);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(imotBgHTML);

                    ;
                    var allApartmentsForCurrentPage = doc.DocumentNode.SelectNodes("//div[contains(@class,'item')]");

                    foreach (var apartmentItem in allApartmentsForCurrentPage)
                    {
                        if(db.ImotBgApartments.Any(x=>x.ApartmentId == apartmentItem.Id))
                        {
                            continue;
                        }

                        var ap = new ImotBgApartment();
                        ap.ApartmentId = apartmentItem.Id;
                        ap.Neighbour = SetNeighbiurhood(currentNeighbiurhood);
                        ap.City = SetCity(city);
                        ap.IsActive = true;
                        ap.IsNew = true;

                        var titleAnchor = apartmentItem.SelectSingleNode(".//div[contains(@class,'zaglavie')]//a[contains(@class,'title')]");

                        string shortTitle = titleAnchor?.ChildNodes.FirstOrDefault(n => n.NodeType == HtmlNodeType.Text)?.InnerText.Trim()!;
                        ap.Title = shortTitle;

                        string href = titleAnchor?.GetAttributeValue("href", "")?.Trim()!;
                        ap.URl = href.Replace("//", "");

                        var priceNode = apartmentItem.SelectSingleNode(".//div[contains(@class,'price')]//div");
                        string priceRaw = priceNode?.InnerText?.Trim()!;

                        var m = Regex.Match(priceRaw, @"([\d\s.,]+)\s*€");
                        if (m.Success) ap.Price = double.Parse(m.Groups[1].Value.Replace(" ", "")); 

                        var info = apartmentItem.SelectSingleNode(".//div[contains(@class,'info')]");
                        ap.MoreInformation = info.InnerText.Replace(" ","").Trim();

                        var mArea = Regex.Match(info.InnerText, @"(\d+)\s*кв\.м");
                        if (mArea.Success) ap.SquareMetres = double.Parse(mArea.Groups[1].Value);

                        var pricePerSquareMetres = Math.Round(ap.Price / ap.SquareMetres, 2);
                        ap.PricePerSqMetre = pricePerSquareMetres;

                        var mFloor = Regex.Match(info.InnerText, @"(\d+)-\w*\sет");
                        if (mFloor.Success) ap.Floor = int.Parse(mFloor.Groups[1].Value);

                        db.ImotBgApartments.Add(ap);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ms)
            {

            }
        }

        private string SetNeighbiurhood(string currentNeighbiurhood)
        {
            string neighbiurhood = null;
            switch (currentNeighbiurhood)
            {
                case "mladost-1":
                    neighbiurhood = "Младост 1";
                    break;
                case "mladost-2":
                    neighbiurhood = "Младост 2";
                    break;
                case "mladost-3":
                    neighbiurhood = "Младост 3";
                    break;
                case "mladost-4":
                    neighbiurhood = "Младост 4";
                    break;
            }
            return neighbiurhood;
        }
        private string SetCity(string currentCity)
        {
            string City = null;
            switch (currentCity)
            {
                case "sofiya":
                    City = "София";
                    break;
            }
            return City;
        }
    }
}
