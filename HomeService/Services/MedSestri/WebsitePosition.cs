
using OpenQA.Selenium.Chrome;
using System.CodeDom;
using System.Text.Json;
using static HomeService.Program;

namespace HomeService.Services.MedSestri
{
    public class WebsitePosition : ScheduledTask
    {
        public WebsitePosition() : base(Configuration.Appsettings.GetSection("WebsitePosition").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("WebsitePosition").GetValue<bool>("ServiceActive"))
        {
            
        }
        protected async override Task ExecuteTask()
        {
            //string apiKey = "18d59b83acf6911ca8bd175c1eac50c00af1578c0f1eb7b9584763fc21ca2ccc";
            //string query = "катетри по домовете";
            //string url = $"https://serpapi.com/search.json?" +
            // $"q={Uri.EscapeDataString("катетри по домовете")}" +
            // $"&engine=google" +
            // $"&hl=bg" +
            // $"&gl=bg" +
            // $"&location=София, България" +
            // $"&google_domain=google.bg" +
            // $"&api_key=18d59b83acf6911ca8bd175c1eac50c00af1578c0f1eb7b9584763fc21ca2ccc";


            //var client = new HttpClient();
            //var response = await client.GetStringAsync(url);
            //var json = JsonDocument.Parse(response);
            //;

            //if (json.RootElement.TryGetProperty("organic_results", out var results))
            //{
            //    foreach (var result in results.EnumerateArray())
            //    {
            //        string link = result.GetProperty("link").GetString();

            //        Console.WriteLine($"{link}");
            //    }
            //}

            throw new NotImplementedException();
        }
    }
}
