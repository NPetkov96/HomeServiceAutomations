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
        private static readonly HttpClient client = new HttpClient();
        private static readonly TimeSpan RequestDelay = TimeSpan.FromMilliseconds(300);

        // Fixed BGN/EUR conversion rate (Bulgaria's currency board / euro adoption peg).
        // Bodimed now lists prices only in EUR, so BGN is derived from this rate.
        private const double BgnPerEuro = 1.95583;

        public async Task Run()
        {
            List<MedSestriBloodTest> manipulations = new List<MedSestriBloodTest>();
            List<string> urls = GetUrls();
            int scrapeErrors = 0;

            using (var db = DataBaseContext.Create())
            {
                HtmlDocument doc = new HtmlDocument();
                foreach (var node in urls)
                {
                    try
                    {
                        var bodimedSite = await client.GetStringAsync(node);
                        doc.LoadHtml(bodimedSite);

                        var currentManipulations = doc.DocumentNode.SelectNodes("//*[contains(@class,'products columns-3')]");
                        var currentElement = currentManipulations?.FirstOrDefault();
                        if (currentElement == null)
                        {
                            scrapeErrors++;
                            WriteLog.Log($"[ERROR] No product list found for {node}");
                            continue;
                        }

                        doc.LoadHtml(currentElement.InnerHtml);
                        var liNodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'product')]");
                        if (liNodes == null)
                        {
                            scrapeErrors++;
                            WriteLog.Log($"[ERROR] No product items found for {node}");
                            continue;
                        }

                        foreach (var item in liNodes)
                        {
                            var title = item.SelectSingleNode(".//*[@class='woocommerce-loop-product__title']")?.InnerText.Trim();

                            try
                            {
                                if (title == null || manipulations.Any(m => m.Name == title)) continue;

                                var priceElement = item.SelectSingleNode(".//*[contains(@class, 'woocommerce-Price-amount')]")?.InnerText.Trim();
                                if (priceElement == null)
                                {
                                    scrapeErrors++;
                                    WriteLog.Log($"[ERROR] Price element missing for '{title}' ({node})");
                                    continue;
                                }

                                var priceMatch = Regex.Match(priceElement, @"\d+[.,]?\d*");
                                if (!priceMatch.Success)
                                {
                                    scrapeErrors++;
                                    WriteLog.Log($"[ERROR] Could not parse price for '{title}': \"{priceElement}\" ({node})");
                                    continue;
                                }

                                var euroPrice = Math.Round(double.Parse(priceMatch.Value.Replace(",", ".")), 2);
                                var bngPrice = Math.Round(euroPrice * BgnPerEuro, 2);

                                manipulations.Add(new MedSestriBloodTest()
                                {
                                    Name = title,
                                    EuroPrice = euroPrice,
                                    BngPrice = bngPrice
                                });
                            }
                            catch (Exception ex)
                            {
                                scrapeErrors++;
                                WriteLog.Log($"[ERROR] Failed to parse product '{title}' ({node}): {ex.Message}", ex.StackTrace ?? string.Empty);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        scrapeErrors++;
                        WriteLog.Log($"[ERROR] Failed to fetch {node}: {ex.Message}", ex.StackTrace ?? string.Empty);
                    }
                    finally
                    {
                        await Task.Delay(RequestDelay);
                    }
                }

                int added = 0, updated = 0, unchanged = 0, saveErrors = 0;

                foreach (var manipulation in manipulations)
                {
                    try
                    {
                        var currentManipulation = await db.MedSestriBloodTests
                            .FirstOrDefaultAsync(m => m.Name == manipulation.Name);

                        if (currentManipulation == null)
                        {
                            db.MedSestriBloodTests.Add(manipulation);
                            await db.SaveChangesAsync();
                            added++;
                            WriteLog.Log($"[OK][NEW] '{manipulation.Name}': EUR={manipulation.EuroPrice}, BGN={manipulation.BngPrice}");
                        }
                        else if (currentManipulation.EuroPrice != manipulation.EuroPrice || currentManipulation.BngPrice != manipulation.BngPrice)
                        {
                            WriteLog.Log($"[OK][UPDATE] '{manipulation.Name}': EUR {currentManipulation.EuroPrice}->{manipulation.EuroPrice}, BGN {currentManipulation.BngPrice}->{manipulation.BngPrice}");
                            currentManipulation.EuroPrice = manipulation.EuroPrice;
                            currentManipulation.BngPrice = manipulation.BngPrice;
                            await db.SaveChangesAsync();
                            updated++;
                        }
                        else
                        {
                            unchanged++;
                        }
                    }
                    catch (Exception ex)
                    {
                        saveErrors++;
                        WriteLog.Log($"[ERROR] Failed to save '{manipulation.Name}': {ex.Message}", ex.StackTrace ?? string.Empty);
                    }
                }

                WriteLog.Log($"Bodimed blood test update complete: {added} new, {updated} updated, {unchanged} unchanged, {scrapeErrors} scrape errors, {saveErrors} save errors (of {manipulations.Count} scraped items).");
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
                "https://bodimed.com/en/izsledvaniq-v-bodimed/pharmacogenetics2/",
                "https://bodimed.com/izsledvaniq-v-bodimed/babrechni-konkrementi/",
                "https://bodimed.com/izsledvaniq-v-bodimed/drugi-uslugi/",
            };
        }
    }
}
