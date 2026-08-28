using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Crypto;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Models.Security;


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
            DCOPS_Message dcm = new DCOPS_Message();
            await Tools.SaveToDB(Request.Form["hidEmailBody"], DateTime.Parse(Request.Form["hidRunStamp"]), dcm);
            return new JsonResult(dcm);
        }
        public JsonResult OnPostForceError()
        {
            var x = 1;
            var y = 0;
            var z = x / y;
            return new JsonResult("ok");
        }
        public async Task<JsonResult> OnPostPopulateDatesDropDown()
        {
            JsonResult jr;
            DateTime dtStart = DateTime.Parse(Request.Form["datePickerDisplay"].ToString().Split(" - ")[0]);
            DateTime dtEnd = DateTime.Parse(Request.Form["datePickerDisplay"].ToString().Split(" - ")[1]);
            try
            {
                List<string> Dates = await Tools.PopulateDates(dtStart, dtEnd);
                jr = new JsonResult(Dates);
            }
            catch (Exception ex)
            {
                DCOPS_Message dcm = new DCOPS_Message();
                dcm.errorNumber = ex.HResult;
                dcm.isError = true;
                dcm.msg = ex.Message;
                jr= new JsonResult(dcm);
            }
            return  jr;
        }
        public JsonResult OnGetGetData()
        {
            JsonResult jr;
            try
            {
                Task t = Task.Run(() => GetSensorData());
                t.Wait();

                List<Digitalsensor> SensorData = new List<Digitalsensor>();

                Rootobject ds = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(IndexModel.SensorData);
                metaData = ds.general;


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
                jr = new JsonResult(SensorData);

            }
            catch (Exception ex)
            {
                DCOPS_Message dcm = new DCOPS_Message();
                dcm.isError = true;
                dcm.msg = ex.Message;
                jr = new JsonResult(dcm);
            }

            return jr;
        }
        public JsonResult OnPostViewHistory()
        {
            JsonResult jr;
            string htmlReturned = string.Empty;
            try
            {
                string targetDate = Request.Form["hidViewDate"].ToString();
                htmlReturned = Tools.GetHistory(targetDate);
                jr = new JsonResult(htmlReturned);
            }
            catch (Exception ex)
            {
                DCOPS_Message dcm = new DCOPS_Message();
                dcm.isError = true;
                dcm.msg = ex.Message;
                jr = new JsonResult(dcm);
            } finally
            {
            }
            return jr;
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

