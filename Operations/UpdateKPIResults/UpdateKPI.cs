using DataLayer;
using DataLayer.Models;
using System.Text.Json;

namespace Operations.UpdateKPIResults
{
    public class UpdateKPI
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task Update()
        {
            using (var db = new DataBaseContext())
            {
                var settings = db.CampaignSettings.ToDictionary(s => s.Name!, s => s.Value)!;
                var metaToken = settings["MetaAccessToken"];
                var clientsFirstPage = settings["FirstClientsPage"]!.Replace("<ACCESS_TOKEN>", metaToken);
                var template = settings["AccountInfomationCPC"]!.Replace("<ACCESS_TOKEN>", metaToken);
                try
                {
                    var adAccountsResponse = await client.GetStringAsync(clientsFirstPage);

                    List<AccountData> clients = JsonSerializer.Deserialize<KPIClientDTOResponse>(adAccountsResponse)!.Data;

                    FilterClients(ref clients);

                    List<decimal> cpcFacebook = new List<decimal>();
                    List<decimal> cpcInstagram = new List<decimal>();

                    List<decimal> cppeFacebook = new List<decimal>();
                    List<decimal> cppeInstagram = new List<decimal>();


                    foreach (var client in clients)
                    {
                        var currentUrl = template;
                        currentUrl = currentUrl.Replace("<ACCOUNT_ID>", client.Id);
                        var clientCPC = await UpdateKPI.client.GetStringAsync(currentUrl);
                        var clientsResults = JsonSerializer.Deserialize<KPICalculationsDTO>(clientCPC)!.Data.ToList();

                        if (clientsResults.Count == 0 || clientsResults == null) continue;

                        foreach (var ad in clientsResults)
                        {

                            try
                            {
                                if (ad.Objective.Contains("CLICKS") || ad.Objective.Contains("AWARENESS"))
                                {
                                    if (decimal.TryParse(ad.Cpc, out var cpc))
                                    {
                                        if (!string.IsNullOrEmpty(ad.PublisherPlatform))
                                        {
                                            switch (ad.PublisherPlatform.ToLower())
                                            {
                                                case "facebook":
                                                    cpcFacebook.Add(cpc);
                                                    break;
                                                case "instagram":
                                                    cpcInstagram.Add(cpc);
                                                    break;
                                            }
                                        }
                                    }
                                    continue;
                                }

                                if (ad.Objective.Contains("ENGAGEMENT"))
                                {
                                    if (decimal.TryParse(ad.Spend, out var spend) && decimal.TryParse(ad.Actions.FirstOrDefault(x => x.ActionType == "page_engagement")!.Value, out var pageEngagemetnValue))
                                    {
                                        var result = spend / pageEngagemetnValue;
                                        if (!string.IsNullOrEmpty(ad.PublisherPlatform))
                                        {
                                            switch (ad.PublisherPlatform.ToLower())
                                            {
                                                case "facebook":
                                                    cppeFacebook.Add(result);
                                                    break;
                                                case "instagram":
                                                    cppeInstagram.Add(result);
                                                    break;
                                            }
                                        }
                                    }
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }

                    var platforms = db.CampaignPlatforms.ToList();
                    string[] availablePlatforms = { "Facebook", "Instagram", "Facebook & Instagram" };


                    foreach (var plt in platforms)
                    {
                        switch (plt.Name)
                        {
                            case "Facebook":
                                plt.CPC = cpcFacebook.Average().ToString("F2");
                                plt.CostPerPostEngagment = cppeFacebook.Average().ToString("F2");
                                break;
                            case "Instagram":
                                plt.CPC = cpcInstagram.Average().ToString("F2");
                                plt.CostPerPostEngagment = cppeInstagram.Average().ToString("F2");
                                break;
                            case "Facebook & Instagram":
                                decimal cpcFbIn = (cpcInstagram.Average() + cpcFacebook.Average()) / 2;
                                decimal cppeFbIn = (cppeInstagram.Average() + cppeFacebook.Average()) / 2;

                                plt.CPC = cpcFbIn.ToString("F2");
                                plt.CostPerPostEngagment = cppeFbIn.ToString("F2");
                                break;
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(settings["ErrorFilePth"]!, $"[{DateTime.Now}] {ex.StackTrace}\n");
                }
            }
        }

        private static void FilterClients(ref List<AccountData> data)
        {
            string[] validClients = {
                "4048468718730268",
                "574489302274334",
                "1481991586103571",
                "2409152069443433",
                "1918990438940543",
                "1327625075368808",
                "666132849598004",
                "1232053344940608",
                "692107303459775",
                "1063850518943638",
                "628383174662635",
                "1563430127380974",
                "227343554847771",
                "883214792410975",
                "585242935293202",
                "1099225831562423",
                "2684967968312914",
                "645881047256709",
                "3220164924952345",
                "799743824585982",
                "806970807185247",
                "920236238619641",
                "1189106182807216",
                "975353383772646",
                "1215716350306792",
                "577643814869516",
                "1184110895884225",
                "275903031503293",
                "2402143663349393",
                "615543618279824",
                "900551403621803",
                "3978831169030538",
                "150448577261228",
                "1038426311629795",
                "1790858865161271"
            };

            data = data.Where(x => validClients.Contains(x.AccountId)).ToList();
        }
    }
}
