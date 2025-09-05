using DataLayer;
using DataLayer.Models;
using Extensions;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Operations.BloodTetsUpdate
{
    public class UpdateBloodTestsOperation
    {
        public async Task Run()
        {
            List<MedSestriBloodTest> manipulations = new List<MedSestriBloodTest>();
            List<string> urls = GetUrls();

            using (var db = new DataBaseContext())
            using (var client = new HttpClient())
            {
                HtmlDocument doc = new HtmlDocument();
                foreach (var node in urls)
                {
                    try
                    {
                        var bodimedSite = await client.GetStringAsync(node);
                        doc.LoadHtml(bodimedSite);

                        var currentManipulations = doc.DocumentNode.SelectNodes("//*[contains(@class,'products columns-3')]");
                        var currentElement = currentManipulations.FirstOrDefault();
                        doc.LoadHtml(currentElement.InnerHtml);
                        var liNodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'product')]");

                        foreach (var item in liNodes)
                        {
                            try
                            {
                                var title = item.SelectSingleNode(".//*[@class='woocommerce-loop-product__title']")?.InnerText.Trim();
                                if (manipulations.Any(m => m.Name == title) || title == null) continue;

                                var euroPriceElement = item.SelectSingleNode(".//*[contains(@class, 'euro-price')]")?.InnerText.Trim();
                                var matchPrice = Regex.Match(euroPriceElement, @"\d+[.,]?\d*");
                                string euroPrice = matchPrice.Value.Replace(",", ".");

                                var levPriceElement = item.SelectSingleNode(".//*[contains(@class, 'woocommerce-Price-amount amount')]")?.InnerText.Trim();
                                matchPrice = Regex.Match(levPriceElement, @"\d+[.,]?\d*");
                                string levPrice = matchPrice.Value.Replace(",", ".");


                                var model = new MedSestriBloodTest()
                                {
                                    Name = title,
                                    BngPrice = double.Parse(levPrice),
                                    EuroPrice = double.Parse(euroPrice)
                                };

                                manipulations.Add(model);
                            }
                            catch (Exception ex)
                            {
                                WriteLog.Log(ex.Message, ex.StackTrace!);
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Log(ex.Message, ex.StackTrace!);
                        continue;
                    }
                }

                foreach (var manipulation in manipulations)
                {
                    var currentManipulation = await db.MedSestriBloodTests
                        .FirstOrDefaultAsync(m => m.Name == manipulation.Name);

                    if (currentManipulation == null)
                    {
                        db.MedSestriBloodTests.Add(manipulation);
                    }
                    else
                    {
                        currentManipulation.Name = manipulation.Name;
                        currentManipulation.EuroPrice = Math.Round(manipulation.EuroPrice, 2);
                        currentManipulation.BngPrice = Math.Round(manipulation.BngPrice, 2);
                    }
                    await db.SaveChangesAsync();
                }
               
                WriteLog.Log("Successfully updated Bodimed blood test prices!");
            }
        }

        private static List<string> GetUrls()
        {
            return new List<string>()
            {
                "https://bodimed.com/izsledvaniq-v-bodimed/paketi/",
                "https://bodimed.com/izsledvaniq-v-bodimed/hematologiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/koagulacziya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/klinichna-himiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/hormoni/",
                "https://bodimed.com/izsledvaniq-v-bodimed/tumorni-markeri/",
                "https://bodimed.com/izsledvaniq-v-bodimed/vitamini/",
                "https://bodimed.com/izsledvaniq-v-bodimed/speczifichni-markeri/",
                "https://bodimed.com/izsledvaniq-v-bodimed/urinni-analizi-bg/",
                "https://bodimed.com/izsledvaniq-v-bodimed/infekcziozni-bolesti-mikrobiologiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/mikrobiologiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/imunologiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/imunohematologiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/alergii-i-hranitelna-neponosimost/",
                "https://bodimed.com/izsledvaniq-v-bodimed/neinvazivna-chernodrobna-diagnostika/",
                "https://bodimed.com/izsledvaniq-v-bodimed/esenczialni-i-toksichni-elementi/",
                "https://bodimed.com/izsledvaniq-v-bodimed/neinvaziven-prenatalen-skrining/",
                "https://bodimed.com/izsledvaniq-v-bodimed/spermograma/",
                "https://bodimed.com/izsledvaniq-v-bodimed/czitopatologiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/patomorfologichni-izsledvaniya-ag-czit/",
                "https://bodimed.com/izsledvaniq-v-bodimed/molekulyarna-diagnostika-bg/",
                "https://bodimed.com/izsledvaniq-v-bodimed/speczializirana-feczes-diagnostika/",
                "https://bodimed.com/izsledvaniq-v-bodimed/parazitologiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/virusologiya/",
                "https://bodimed.com/izsledvaniq-v-bodimed/pharmacogenetics/",
                "https://bodimed.com/izsledvaniq-v-bodimed/babrechni-konkrementi/",
                "https://bodimed.com/izsledvaniq-v-bodimed/drugi-uslugi/",
            };
        }
    }
}
