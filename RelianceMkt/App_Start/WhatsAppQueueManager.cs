using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using RelianceMkt.Models;
using System.Web.Hosting;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.IO;

namespace RelianceMkt.App_Start
{
    public static class WhatsAppQueueManager
    {
        private static readonly ConcurrentQueue<WhatsAppJob> _queue
            = new ConcurrentQueue<WhatsAppJob>();

        private static volatile bool _isProcessing = false;
        private static readonly object _lock = new object();

        // ✅ Single job
        public static void Enqueue(WhatsAppJob job)
        {
            _queue.Enqueue(job);
            EnsureProcessorRunning();
        }

        // ✅ Bulk jobs — 1000 rows ke liye
        public static void EnqueueBulk(List<WhatsAppJob> jobs)
        {
            foreach (var job in jobs)
                _queue.Enqueue(job);

            EnsureProcessorRunning();
        }

        private static void EnsureProcessorRunning()
        {
            if (_isProcessing) return;

            lock (_lock)
            {
                if (_isProcessing) return;
                _isProcessing = true;

                HostingEnvironment.QueueBackgroundWorkItem(async ct =>
                {
                    await ProcessQueueAsync(ct);
                    _isProcessing = false;
                });
            }
        }

        private static async Task ProcessQueueAsync(CancellationToken ct)
        {
            string Constr = DBHelper.GetConnectionString();

            while (!ct.IsCancellationRequested && _queue.TryDequeue(out WhatsAppJob job))
            {
                try
                {
                    // ✅ Rate limit — 300ms gap = ~200 msg/min
                    // WhatsApp ban se bachao
                    await Task.Delay(300, ct);

                    await SendWhatsAppMessageAsync(
                        job.Mobile, job.Name, job.CampaignName, job.ImageLink);

                    // ✅ Status Sent update karo
                    UpdateStatus(Constr, job.Mobile, job.CampaignName, "Sent");

                    ServerLog($"✅ Sent: {job.Mobile}");
                }
                catch (Exception ex)
                {
                    job.RetryCount++;

                    if (job.RetryCount <= 3)
                    {
                        // Retry ke liye wapas queue mein daalo
                        await Task.Delay(5000, ct);
                        _queue.Enqueue(job);
                        ServerLog($"⚠️ Retry {job.RetryCount}/3: {job.Mobile}");
                    }
                    else
                    {
                        // 3 baar fail — Failed mark karo
                        UpdateStatus(Constr, job.Mobile, job.CampaignName, "Failed");
                        ServerLog($"❌ FAILED: {job.Mobile} | {ex.Message}");
                    }
                }
            }
        }

        private static void UpdateStatus(string conStr, string mobile,
                                          string campaign, string status)
        {
            try
            {
                using (var con = new SqlConnection(conStr))
                using (var cmd = new SqlCommand(@"
                UPDATE CampaignResponses 
                SET Response = @s, CreatedDate = GETDATE()
                WHERE Mobile = @m AND CampaignName = @c", con))
                {
                    con.Open();
                    cmd.Parameters.AddWithValue("@s", status);
                    cmd.Parameters.AddWithValue("@m", mobile);
                    cmd.Parameters.AddWithValue("@c", campaign);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private static async Task SendWhatsAppMessageAsync(
        string mobileNumber, string name, string campaignName, string imageLink)
        {
            var apiUrl = "https://kapi.omni-channel.in/fe/api/v1/iPMessage/One2Many";
            string fullImageUrl = GetFullImageUrl(imageLink);
            string cleanMobile = mobileNumber.Replace("+", "").Trim();

            var requestBody = new
            {
                mode = "waba",
                wabaPhoneNumber = "+919220363790",
                wabaTemplateId = "437824",
                campId = "66012",
                unicode = false,
                shortMessages = new[]
                {
                new
                {
                    recipient     = cleanMobile,
                    corelationId  = Guid.NewGuid().ToString("N").Substring(0, 10),
                    context       = new
                    {
                        waba_link     = fullImageUrl,
                        waba_name     = name,
                        waba_campaign = campaignName
                    }
                }
            }
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(
                    "Authorization", "Basic UmVsaWFuY2VXQUJBOjhVMU5x");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(apiUrl, content);
                    string result = await response.Content.ReadAsStringAsync();
                    ServerLog($"WA Response {mobileNumber}: {response.StatusCode} | {result}");
                }
                catch (Exception ex)
                {
                    ServerLog($"WA Error {mobileNumber}: {ex.Message}");
                    throw; // Retry ke liye exception rethrow karo
                }
            }
        }

        private static string GetFullImageUrl(string imageLink)
        {
            if (string.IsNullOrEmpty(imageLink)) return "";
            if (imageLink.StartsWith("http://") ||
                imageLink.StartsWith("https://")) return imageLink;
            string baseUrl = "https://pants-feminine-salvage.ngrok-free.dev";
            return baseUrl.TrimEnd('/') + "/" + imageLink.TrimStart('/');
        }

        private static void ServerLog(string msg)
        {
            try
            {
                string path = HttpContext.Current != null
                    ? HttpContext.Current.Server.MapPath("~/UploadExcel_Log.txt")
                    : @"C:\Logs\WhatsApp_Log.txt";
                File.AppendAllText(path,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " :: " + msg + "\n");
            }
            catch { }
        }

    }
}