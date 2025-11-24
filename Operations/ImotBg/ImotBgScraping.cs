using DataLayer;
using DataLayer.Models.ImotBg;
using Extensions;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace Operations.ImotBg
{
    public class ImotBgScraping
    {
        public async Task StartScraping(DataBaseContext db)
        {
            var client = new HttpClient();
            var settings = db.ImotBgSettings.ToDictionary(x => x.Name, x => x.Value);

            var city = settings["SerachCity"];
            var neighbourhood = settings["SerachNeighbour"].Split(",");
            var constructionType = settings["ConstructionType"].Split(",");
            var skippingTitleWords = settings["SkippingTitleWords"].Split(",");

            var baseURL = settings["BaseURL"];

            for (int i = 0; i < neighbourhood.Count(); i++)
            {
                for (int c = 0; c < constructionType.Count(); c++)
                {
                    var url = baseURL.Replace("#CITY#", city)
                                     .Replace("#CURRENTNEIGHBOURHOOD#", neighbourhood[i])
                                     .Replace("#CONSTRUCTION#", constructionType[c]);

                    string imotBgHTML = null;
                    try
                    {
                        var bytes = await client.GetByteArrayAsync(url);
                        imotBgHTML = Encoding.GetEncoding("windows-1251").GetString(bytes);
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Log(ex.Message, ex.StackTrace!);
                        continue;
                    }

                    var doc = new HtmlDocument();
                    doc.LoadHtml(imotBgHTML);

                    var allApartmentsForCurrentPage = doc.DocumentNode.SelectNodes("//div[contains(@class,'item')]");
                    while (allApartmentsForCurrentPage != null)
                    {
                        foreach (var apartmentItem in allApartmentsForCurrentPage)
                        {
                            try
                            {
                                var titleAnchor = apartmentItem.SelectSingleNode(".//div[contains(@class,'zaglavie')]//a[contains(@class,'title')]");

                                string shortTitle = titleAnchor?.ChildNodes.FirstOrDefault(n => n.NodeType == HtmlNodeType.Text)?.InnerText.Trim()!;


                                if (string.IsNullOrEmpty(apartmentItem.Id) ||
                                    skippingTitleWords.Any(x => shortTitle.ToLower().Trim().Contains(x.ToLower().Trim())))
                                    continue;

                                var price = apartmentItem.SelectSingleNode(".//div[contains(@class,'price')]//div").InnerText?.Trim()!;
                                var m = Regex.Match(price, @"([\d\s.,]+)\s*€");
                                if (!m.Success)
                                    continue;

                                var parsedPrice = double.Parse(m.Groups[1].Value.Replace(" ", ""));

                                var existingApartment = await db.ImotBgApartments.FirstOrDefaultAsync(x => x.ApartmentId == apartmentItem.Id);
                                if (existingApartment != null)
                                {
                                    existingApartment.Price = parsedPrice;
                                    existingApartment.IsNew = false;
                                    await db.SaveChangesAsync();
                                    continue;
                                }

                                var ap = new ImotBgApartment();
                                ap.ApartmentId = apartmentItem.Id;
                                ap.Neighbour = SetNeighbiurhood(neighbourhood[i]);
                                ap.City = SetCity(city);
                                ap.IsActive = true;
                                ap.IsNew = true;
                                ap.Price = parsedPrice;
                                ap.Construction = SetConstructionType(constructionType[c]);
                                ap.Title = shortTitle;

                                string href = titleAnchor?.GetAttributeValue("href", "")?.Trim()!;
                                ap.URl = href.Replace("//", "");

                                var info = apartmentItem.SelectSingleNode(".//div[contains(@class,'info')]");
                                ap.MoreInformation = info.InnerText.Trim();

                                var mArea = Regex.Match(info.InnerText, @"(\d+)\s*кв\.м");
                                if (mArea.Success) ap.SquareMetres = double.Parse(mArea.Groups[1].Value);

                                double pricePerSquareMetres = Math.Round((double)ap.Price / ap.SquareMetres, 2);
                                ap.PricePerSqMetre = pricePerSquareMetres;

                                var mFloor = Regex.Match(info.InnerText, @"(\d+)-\w*\sет");
                                if (mFloor.Success) ap.Floor = int.Parse(mFloor.Groups[1].Value);

                                await db.ImotBgApartments.AddAsync(ap);
                                await db.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                WriteLog.Log(ex.Message, ex.StackTrace!);
                            }
                        }

                        var nextPageNode = doc.DocumentNode.SelectSingleNode("//a[contains(@class, 'next')]");
                        if (nextPageNode == null)
                            break;

                        string nextUrl = nextPageNode.GetAttributeValue("href", null);
                        var bytesNextPage = await client.GetByteArrayAsync(nextUrl);
                        var imotBgHTMLNextPage = Encoding.GetEncoding("windows-1251").GetString(bytesNextPage);
                        doc.LoadHtml(imotBgHTMLNextPage);

                        allApartmentsForCurrentPage = doc.DocumentNode.SelectNodes("//div[contains(@class,'item')]");
                    }
                }
            }
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
        private string SetNeighbiurhood(string currentNeighbiurhood)
        {
            string neighbiurhood = null;
            switch (currentNeighbiurhood)
            {
                case "7-mi-11-ti-kilometar": neighbiurhood = "7-ми 11-ти километър"; break;
                case "abdovitsa": neighbiurhood = "Абдовица"; break;
                case "banishora": neighbiurhood = "Банишора"; break;
                case "belite-brezi": neighbiurhood = "Белите брези"; break;
                case "benkovski": neighbiurhood = "Бенковски"; break;
                case "borovo": neighbiurhood = "Борово"; break;
                case "botunets": neighbiurhood = "Ботунец"; break;
                case "botunets-2": neighbiurhood = "Ботунец 2"; break;
                case "boyana": neighbiurhood = "Бояна"; break;
                case "bakston": neighbiurhood = "Бъкстон"; break;
                case "vitosha": neighbiurhood = "Витоша"; break;
                case "voenna-rampa": neighbiurhood = "Военна рампа"; break;
                case "vrazhdebna": neighbiurhood = "Враждебна"; break;
                case "vrabnitsa-1": neighbiurhood = "Връбница 1"; break;
                case "vrabnitsa-2": neighbiurhood = "Връбница 2"; break;
                case "gevgelijski": neighbiurhood = "Гевгелийски"; break;
                case "geo-milev": neighbiurhood = "Гео Милев"; break;
                case "gorna-banya": neighbiurhood = "Горна баня"; break;
                case "gorublyane": neighbiurhood = "Горубляне"; break;
                case "gotse-delchev": neighbiurhood = "Гоце Делчев"; break;
                case "gradina": neighbiurhood = "Градина"; break;
                case "dianabad": neighbiurhood = "Дианабад"; break;
                case "dimitar-milenkov": neighbiurhood = "Димитър Миленков"; break;
                case "doktorski-pametnik": neighbiurhood = "Докторски паметник"; break;
                case "dragalevtsi": neighbiurhood = "Драгалевци"; break;
                case "drujba-1": neighbiurhood = "Дружба 1"; break;
                case "drujba-2": neighbiurhood = "Дружба 2"; break;
                case "darvenitsa": neighbiurhood = "Дървеница"; break;
                case "eksperimentalен": neighbiurhood = "Експериментален"; break;
                case "zapaden-park": neighbiurhood = "Западен парк"; break;
                case "zaharna-fabrika": neighbiurhood = "Захарна фабрика"; break;
                case "zona-b-18": neighbiurhood = "Зона Б-18"; break;
                case "zona-b-19": neighbiurhood = "Зона Б-19"; break;
                case "zona-b-5": neighbiurhood = "Зона Б-5"; break;
                case "zona-b-5-3": neighbiurhood = "Зона Б-5-3"; break;
                case "ivan-vazov": neighbiurhood = "Иван Вазов"; break;
                case "izgrev": neighbiurhood = "Изгрев"; break;
                case "iztok": neighbiurhood = "Изток"; break;
                case "ilinden": neighbiurhood = "Илинден"; break;
                case "iliyantsi": neighbiurhood = "Илиянци"; break;
                case "karpuzitsa": neighbiurhood = "Карпузица"; break;
                case "knyajevo": neighbiurhood = "Княжево"; break;
                case "krasna-polyana-1": neighbiurhood = "Красна поляна 1"; break;
                case "krasna-polyana-2": neighbiurhood = "Красна поляна 2"; break;
                case "krasna-polyana-3": neighbiurhood = "Красна поляна 3"; break;
                case "krasno-selo": neighbiurhood = "Красно село"; break;
                case "kremikovtsi": neighbiurhood = "Кремиковци"; break;
                case "krastova-vada": neighbiurhood = "Кръстова вада"; break;
                case "lagera": neighbiurhood = "Лагера"; break;
                case "levski": neighbiurhood = "Левски"; break;
                case "levski-v": neighbiurhood = "Левски В"; break;
                case "levski-g": neighbiurhood = "Левски Г"; break;
                case "letishte-sofiya": neighbiurhood = "Летище София"; break;
                case "lozenets": neighbiurhood = "Лозенец"; break;
                case "lyulin-centar": neighbiurhood = "Люлин - център"; break;
                case "lyulin-1": neighbiurhood = "Люлин 1"; break;
                case "lyulin-10": neighbiurhood = "Люлин 10"; break;
                case "lyulin-2": neighbiurhood = "Люлин 2"; break;
                case "lyulin-3": neighbiurhood = "Люлин 3"; break;
                case "lyulin-4": neighbiurhood = "Люлин 4"; break;
                case "lyulin-5": neighbiurhood = "Люлин 5"; break;
                case "lyulin-6": neighbiurhood = "Люлин 6"; break;
                case "lyulin-7": neighbiurhood = "Люлин 7"; break;
                case "lyulin-8": neighbiurhood = "Люлин 8"; break;
                case "lyulin-9": neighbiurhood = "Люлин 9"; break;
                case "malashevtsi": neighbiurhood = "Малашевци"; break;
                case "malinova-dolina": neighbiurhood = "Малинова долина"; break;
                case "manastirski-livadi": neighbiurhood = "Манастирски ливади"; break;
                case "meditsinska-akademiya": neighbiurhood = "Медицинска академия"; break;
                case "mladost-1": neighbiurhood = "Младост 1"; break;
                case "mladost-1a": neighbiurhood = "Младост 1А"; break;
                case "mladost-2": neighbiurhood = "Младост 2"; break;
                case "mladost-3": neighbiurhood = "Младост 3"; break;
                case "mladost-4": neighbiurhood = "Младост 4"; break;
                case "moderno-predgradie": neighbiurhood = "Модерно предградие"; break;
                case "musagenitsa": neighbiurhood = "Мусагеница"; break;
                case "nadejda-1": neighbiurhood = "Надежда 1"; break;
                case "nadejda-2": neighbiurhood = "Надежда 2"; break;
                case "nadejda-3": neighbiurhood = "Надежда 3"; break;
                case "nadejda-4": neighbiurhood = "Надежда 4"; break;
                case "npz-iztok": neighbiurhood = "НПЗ Изток"; break;
                case "npz-iskar": neighbiurhood = "НПЗ Искър"; break;
                case "npz-sredec": neighbiurhood = "НПЗ Средец"; break;
                case "npz-hadzhi-dimitar": neighbiurhood = "НПЗ Хаджи Димитър"; break;
                case "obelya": neighbiurhood = "Обеля"; break;
                case "obelya-1": neighbiurhood = "Обеля 1"; break;
                case "obelya-2": neighbiurhood = "Обеля 2"; break;
                case "oborishte": neighbiurhood = "Оборище"; break;
                case "ovcha-kupel": neighbiurhood = "Овча купел"; break;
                case "ovcha-kupel-1": neighbiurhood = "Овча купел 1"; break;
                case "ovcha-kupel-2": neighbiurhood = "Овча купел 2"; break;
                case "orlandovtsi": neighbiurhood = "Орландовци"; break;
                case "pavlovo": neighbiurhood = "Павлово"; break;
                case "pz-iliyantsi": neighbiurhood = "ПЗ Илиянци"; break;
                case "pz-hladilnika": neighbiurhood = "ПЗ Хладилника"; break;
                case "poduyane": neighbiurhood = "Подуяне"; break;
                case "poligona": neighbiurhood = "Полигона"; break;
                case "razsadnika": neighbiurhood = "Разсадника"; break;
                case "reduta": neighbiurhood = "Редута"; break;
                case "republika": neighbiurhood = "Република"; break;
                case "republika-2": neighbiurhood = "Република 2"; break;
                case "sveta-troitsa": neighbiurhood = "Света Троица"; break;
                case "svoboda": neighbiurhood = "Свобода"; break;
                case "serdika": neighbiurhood = "Сердика"; break;
                case "seslavtsi": neighbiurhood = "Сеславци"; break;
                case "simeonovo": neighbiurhood = "Симеоново"; break;
                case "slaviya": neighbiurhood = "Славия"; break;
                case "slatina": neighbiurhood = "Слатина"; break;
                case "spz-moderno-predgradie": neighbiurhood = "СПЗ Модерно предградие"; break;
                case "spz-slatina": neighbiurhood = "СПЗ Слатина"; break;
                case "strelbishte": neighbiurhood = "Стрелбище"; break;
                case "studentski-grad": neighbiurhood = "Студентски град"; break;
                case "suhata-reka": neighbiurhood = "Сухата река"; break;
                case "suhodol": neighbiurhood = "Суходол"; break;
                case "tolstoy": neighbiurhood = "Толстой"; break;
                case "trebich": neighbiurhood = "Требич"; break;
                case "triagalnika": neighbiurhood = "Триъгълника"; break;
                case "fakulteta": neighbiurhood = "Факултета"; break;
                case "filipovtsi": neighbiurhood = "Филиповци"; break;
                case "fondovi-zhilishta": neighbiurhood = "Фондови жилища"; break;
                case "hadzhi-dimitar": neighbiurhood = "Хаджи Димитър"; break;
                case "hipodruma": neighbiurhood = "Хиподрума"; break;
                case "hladilnika": neighbiurhood = "Хладилника"; break;
                case "hristo-botev": neighbiurhood = "Христо Ботев"; break;
                case "centar": neighbiurhood = "Център"; break;
                case "chelopechene": neighbiurhood = "Челопечене"; break;
                case "yavorov": neighbiurhood = "Яворов"; break;
            }

            return neighbiurhood;
        }
        private string SetConstructionType(string currentConstructionType)
        {
            string constructionType = null;
            switch (currentConstructionType)
            {
                case "PANEL":
                    constructionType = "Панел";
                    break;
                case "TUHLA":
                    constructionType = "Тухла";
                    break;
                case "EPK":
                    constructionType = "ЕПК";
                    break;
            }
            return constructionType;
        }
    }
}
