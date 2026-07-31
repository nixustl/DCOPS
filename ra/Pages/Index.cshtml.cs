using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Crypto;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using System.Text.Json;


namespace ra.Pages
{
    public class IndexModel : PageModel
    {

        [BindProperty]
        public static string? SensorData { get; set; } 
        public static string? SensorData8910 { get; set; } 
        public static string? SensorDataHarmon { get; set; } 
        public static General? metaData { get; set; }
        public static string? ChillerData { get; set; } 

        public void OnGet()
        {
        }
        public void OnPost()
        {
        }
        public async Task<JsonResult> OnPostSaveToDB()
        {
            await SaveToDB(Request.Form["hidEmailBody"], DateTime.Parse(Request.Form["hidRunStamp"]));

            return new JsonResult("ok");
        }
        public async Task<JsonResult> OnPostDBFetch()
        {
            await DBFetch();

            return new JsonResult("ok");
        }
        public async Task<JsonResult> OnGetPopulateDatesDropDown()
        {
            List<string> sDates = await Tools.PopulateDates();

                return new JsonResult(sDates);
        }
        public JsonResult OnGetGetData()
        {
            Task t = Task.Run(() => GetSensorData()); 
            t.Wait();

            Rootobject ds = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(IndexModel.SensorData);
            metaData = ds.general;

            List <Digitalsensor> SensorData = new List<Digitalsensor>();

            foreach (var r in ds.digitalSensors)
            {
                if (!String.IsNullOrEmpty(r.label) && r.label.IndexOf("ADSB 106 AC") > -1)
                {
                    r.temperature = Tools.ToFarenheit(r.temperature);
                    SensorData.Add(r);
                }
            }

            t = Task.Run(() => GetSensorData8910());
            t.Wait();

            Rootobject ds8910 = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(SensorData8910);

            foreach (var r in ds8910.digitalSensors)
            {
                if (!String.IsNullOrEmpty(r.label) && r.label.IndexOf("ADSB") > -1)
                {
                    r.temperature = Tools.ToFarenheit(r.temperature);
                    SensorData.Add(r);
                }
            }

            t = Task.Run(() => GetSensorDataHarmon());
            t.Wait();

            Rootobject dsHarmon = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(SensorDataHarmon);

            foreach (var r in dsHarmon.digitalSensors)
            {
                if (!String.IsNullOrEmpty(r.label) && r.label.IndexOf("HAPG") > -1)
                {
                    r.temperature = Tools.ToFarenheit(r.temperature);
                    SensorData.Add(r);
                }
            }

            t = Task.Run(() => GetChillerData());
            t.Wait();

            Digitalsensor Chiller = new Digitalsensor();
            Chiller.label = "Water Chiller Out";
            Chiller.temperature = float.Parse(ChillerData);
            SensorData.Add(Chiller);

            return new JsonResult(SensorData);
        }
        public JsonResult OnPostViewHistory()
        {
            string targetDate = Request.Form["hidViewDate"].ToString();
            string htmlReturned = Tools.GetHistory(targetDate);
            return new JsonResult(htmlReturned);
        }
        private static async Task<JsonResult> DBFetch()
        {
            DCOPS_Message returnMessage = await Tools.DBFetch(new DateTime(2026, 7, 20), new DateTime(2026, 7, 21));
            return new JsonResult(returnMessage);
        }
        private static async Task<JsonResult> SaveToDB(string html, DateTime runDate)
        {
            DCOPS_Message returnMessage = await Tools.SaveToDB(html, runDate);
            return new JsonResult(returnMessage);
        }
        public JsonResult OnGetGetGeneral()
        {
            return new JsonResult(metaData);
        }

        /// ADSB Room 106
        static async Task<string> GetSensorData()
        {
            const string apiBaseUrl = "http://10.200.7.101/status.json";

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);

            string jsonString = await httpClient.GetStringAsync(apiBaseUrl);
            SensorData = jsonString;

            return jsonString;
        }

        // ADSB Room 106A
        static async Task<string> GetSensorData8910()
        {
            const string apiBaseUrl = "http://10.200.7.102/status.json";

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);

            string jsonString = await httpClient.GetStringAsync(apiBaseUrl);
            SensorData8910 = jsonString;

            return jsonString;
        }

        // HAPG Room 408
        static async Task<string> GetSensorDataHarmon()
        {
            const string apiBaseUrl = "http://10.200.7.103/status.json";

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);

            string jsonString = await httpClient.GetStringAsync(apiBaseUrl);
            SensorDataHarmon = jsonString;

            return jsonString;
        }

        static async Task<string> GetChillerData()
        {
            string chillerValue = null;

            const string apiBaseUrl = "http://10.200.7.6/data.xml";

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);

            string watchdog1400 = await httpClient.GetStringAsync(apiBaseUrl);

            XDocument doc = XDocument.Parse(watchdog1400);

            var waterNode = doc.Descendants("device")
                 .FirstOrDefault(x => (string)x.Attribute("name") == "Water Chiller Out");

            foreach (XElement child in waterNode.Elements())
            {
                System.Diagnostics.Debug.WriteLine($"{child.Name}: {child.Value}");
                if (child.Attribute("niceName").Value == "Temperature (F)")
                {
                    chillerValue = child.Attribute("value").Value;  
                }
            }

            ChillerData = chillerValue;
            return chillerValue;
        }
        
        /// <summary>
        /// Send data to Teams RoomAlert channel
        /// </summary>
        /// <returns></returns>
        public JsonResult OnPostPublishData()
        {
            DCOPS_Message returnMessage = new DCOPS_Message();

            Task t = Task.Run(() => Tools.PublishData(Request.Form["hidEmailBody"], Request.Form["hidRunStamp"], returnMessage));
            t.Wait();

            return new JsonResult(returnMessage);
        }
    }
}

