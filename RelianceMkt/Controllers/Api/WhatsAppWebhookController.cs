//using DocumentFormat.OpenXml.Wordprocessing;
//using RelianceMkt.Models;
//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Web.Http;
//using System.Web.Services.Description;
//using System.Web.UI.WebControls;

//namespace RelianceMkt.Controllers.Api
//{
//    public class WhatsAppWebhookController : ApiController
//    {
//        rglinixm_relEntities db = new rglinixm_relEntities();

//        // ✅ Test Method - GET
//        //[HttpGet]
//        //public HttpResponseMessage TestGet()
//        //{
//        //    return Request.CreateResponse(HttpStatusCode.OK, "GET Working!");
//        //}

//        //// ✅ Test Method - POST
//        //[HttpPost]
//        //public HttpResponseMessage TestPost()
//        //{
//        //    return Request.CreateResponse(HttpStatusCode.OK, "POST Working!");
//        //}

//        // ✅ Main Method
//        [HttpPost]
//        public HttpResponseMessage Receive(WhatsAppWebhookModel model)
//        {
//            try
//            {
//                if (model == null || string.IsNullOrEmpty(model.mobile))
//                {
//                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");
//                }

//                string msg = model.message.ToLower();
//                string finalResponse =
//                    msg.Contains("interested") ? "Interested" :
//                    msg.Contains("not") ? "Not Interested" :
//                    "Unknown";

//                var record = db.CampaignResponses
//                    .OrderByDescending(x => x.CreatedDate)
//                    .FirstOrDefault(x => x.Mobile == model.mobile);

//                if (record != null)
//                {
//                    record.Response = finalResponse;
//                    record.CreatedDate = DateTime.Now;
//                    db.SaveChanges();
//                }

//                return Request.CreateResponse(HttpStatusCode.OK, "Response Updated");
//            }
//            catch (Exception ex)
//            {
//                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
//            }
//        }
//    }

//    public class WhatsAppWebhookModel
//    {
//        public string mobile { get; set; }
//        public string message { get; set; }
//    }
//}


using RelianceMkt.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace RelianceMkt.Controllers.Api
{
    public class WhatsAppWebhookController : ApiController
    {
        rglinixm_relEntities db = new rglinixm_relEntities();

        // ✅ Main Method with Detailed Logging
        [HttpPost]
        public HttpResponseMessage Receive(WhatsAppWebhookModel model)
        {
            ServerLog("=================================================================");
            ServerLog("🔔 NEW WEBHOOK REQUEST RECEIVED");
            ServerLog("=================================================================");

            try
            {
                // Log 1: Request received confirmation
                ServerLog($"📥 Raw Model Data: Mobile={model?.mobile ?? "NULL"}, Message={model?.message ?? "NULL"}");

                // Log 2: Validation check
                if (model == null)
                {
                    ServerLog("❌ ERROR: Model is NULL");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Model is null");
                }

                if (string.IsNullOrEmpty(model.mobile))
                {
                    ServerLog("❌ ERROR: Mobile is empty or null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Mobile is empty");
                }

                if (string.IsNullOrEmpty(model.message))
                {
                    ServerLog("❌ ERROR: Message is empty or null");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Message is empty");
                }

                ServerLog($"✅ Validation passed - Mobile: {model.mobile}, Message: {model.message}");

                // Log 3: Message processing
                string msg = model.message.ToLower();
                string finalResponse =
                    msg.Contains("interested") && !msg.Contains("not") ? "Interested" :
                    msg.Contains("not") ? "Not Interested" :
                    "Unknown";

                ServerLog($"🔍 Message Analysis: Original='{model.message}', Lowercase='{msg}', Result='{finalResponse}'");

                // Log 4: Database search
                ServerLog($"🔎 Searching database for mobile: {model.mobile}");

                // Try multiple mobile formats
                string cleanMobile = model.mobile.Trim();
                string withoutPlus = cleanMobile.Replace("+", "");
                string withPlus = cleanMobile.StartsWith("+") ? cleanMobile : "+" + cleanMobile;
                string withoutCountryCode = cleanMobile.Replace("+91", "").Replace("91", "");
                string with91 = withoutCountryCode.Length == 10 ? "91" + withoutCountryCode : cleanMobile;
                string withPlus91 = withoutCountryCode.Length == 10 ? "+91" + withoutCountryCode : cleanMobile;

                ServerLog($"📱 Trying formats: [{cleanMobile}], [{withoutPlus}], [{withPlus}], [{withoutCountryCode}], [{with91}], [{withPlus91}]");

                var record = db.CampaignResponses
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefault(x => x.Mobile == cleanMobile
                                      || x.Mobile == withoutPlus
                                      || x.Mobile == withPlus
                                      || x.Mobile == withoutCountryCode
                                      || x.Mobile == with91
                                      || x.Mobile == withPlus91);

                // Log 5: Database result
                if (record != null)
                {
                    ServerLog($"✅ RECORD FOUND!");
                    ServerLog($"   ├─ ID: {record.Id}");
                    ServerLog($"   ├─ Mobile in DB: {record.Mobile}");
                    ServerLog($"   ├─ Old Response: {record.Response}");
                    ServerLog($"   ├─ Old CreatedDate: {record.CreatedDate}");
                    ServerLog($"   └─ New Response: {finalResponse}");

                    record.Response = finalResponse;
                    record.CreatedDate = DateTime.Now;
                    db.SaveChanges();

                    ServerLog($"💾 Database updated successfully!");
                    ServerLog($"✅ API RESPONSE: 200 OK - Response Updated");
                    ServerLog("=================================================================\n");

                    return Request.CreateResponse(HttpStatusCode.OK, "Response Updated");
                }
                else
                {
                    ServerLog($"❌ NO RECORD FOUND in database for mobile: {model.mobile}");
                    ServerLog($"❌ Tried all format combinations but no match found");

                    // Log total records for reference
                    int totalRecords = db.CampaignResponses.Count();
                    ServerLog($"ℹ️  Total records in CampaignResponses table: {totalRecords}");

                    // Log recent 5 mobile numbers for comparison
                    var recentMobiles = db.CampaignResponses
                        .OrderByDescending(x => x.CreatedDate)
                        .Take(5)
                        .Select(x => x.Mobile)
                        .ToList();

                    ServerLog($"ℹ️  Recent 5 mobile numbers in DB:");
                    for (int i = 0; i < recentMobiles.Count; i++)
                    {
                        ServerLog($"   {i + 1}. {recentMobiles[i]}");
                    }

                    ServerLog($"❌ API RESPONSE: 404 Not Found");
                    ServerLog("=================================================================\n");

                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        $"No campaign record found for mobile: {model.mobile}");
                }
            }
            catch (Exception ex)
            {
                ServerLog($"💥 EXCEPTION OCCURRED!");
                ServerLog($"   ├─ Message: {ex.Message}");
                ServerLog($"   ├─ Source: {ex.Source}");
                ServerLog($"   └─ StackTrace: {ex.StackTrace}");
                ServerLog($"❌ API RESPONSE: 500 Internal Server Error");
                ServerLog("=================================================================\n");

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ✅ Test Endpoint to verify API is accessible
        [HttpGet]
        public HttpResponseMessage Test()
        {
            ServerLog("🧪 TEST ENDPOINT CALLED");
            ServerLog($"   ├─ Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            ServerLog($"   └─ Status: API is working!\n");

            return Request.CreateResponse(HttpStatusCode.OK,
                $"API is working! Server Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        // ✅ View Logs Endpoint (Access logs via browser)
        [HttpGet]
        [Route("api/WhatsAppWebhook/ViewLogs")]
        public HttpResponseMessage ViewLogs()
        {
            try
            {
                string path = HttpContext.Current.Server.MapPath("~/UploadExcel_Log.txt");

                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);

                    // Return last 10000 characters (recent logs)
                    if (content.Length > 10000)
                    {
                        content = "... (showing last 10000 characters)\n\n" +
                                  content.Substring(content.Length - 10000);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, content);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,
                        "Log file not found at: " + path);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ✅ Clear Logs Endpoint
        [HttpGet]
        [Route("api/WhatsAppWebhook/ClearLogs")]
        public HttpResponseMessage ClearLogs()
        {
            try
            {
                string path = HttpContext.Current.Server.MapPath("~/UploadExcel_Log.txt");

                if (File.Exists(path))
                {
                    File.WriteAllText(path, "");
                    ServerLog("🗑️  LOG FILE CLEARED\n");
                    return Request.CreateResponse(HttpStatusCode.OK, "Logs cleared successfully");
                }

                return Request.CreateResponse(HttpStatusCode.NotFound, "Log file not found");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // 📝 Logging Helper Method
        private void ServerLog(string msg)
        {
            try
            {
                string path = HttpContext.Current.Server.MapPath("~/UploadExcel_Log.txt");
                File.AppendAllText(
                    path,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " :: " + msg + Environment.NewLine
                );
            }
            catch { }
        }
    }

    public class WhatsAppWebhookModel
    {
        public string mobile { get; set; }
        public string message { get; set; }
    }
}