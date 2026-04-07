using log4net;
using RelianceMkt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RelianceMkt.Controllers.Api
{
    public class CampaignResponseApiController : ApiController
    {
        public rglinixm_relEntities db = new rglinixm_relEntities();

        private static readonly ILog log = LogManager.GetLogger(typeof(CampaignResponseApiController));

        // ✅ URL: /api/CampaignResponseApi/SaveResponse
        [HttpPost]
        [Route("api/CampaignResponseApi/SaveResponse")]
        public async Task<HttpResponseMessage> SaveResponse(CampaignResponseModel model)
        {
            try
            {
                log.Info("SaveResponse API called");
                log.Info($"Request: Mobile={model?.MobileNumber}, Campaign={model?.CampaignName}, Response={model?.ResponseText}");

                if (model == null || string.IsNullOrEmpty(model.MobileNumber))
                {
                    log.Warn("Invalid data received");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");
                }

                var existing = db.CampaignResponses
                    .FirstOrDefault(x =>
                        x.Mobile == model.MobileNumber &&
                        x.CampaignName == model.CampaignName);

                if (existing != null)
                {
                    log.Info($"Updating record for Mobile={model.MobileNumber}");
                    existing.Response = model.ResponseText;
                    existing.CreatedDate = DateTime.Now;
                }
                else
                {
                    log.Info($"Inserting new record for Mobile={model.MobileNumber}");

                    CampaignRespons obj = new CampaignRespons
                    {
                        Mobile = model.MobileNumber,
                        CampaignName = model.CampaignName,
                        Response = model.ResponseText,
                        CreatedDate = DateTime.Now
                    };

                    db.CampaignResponses.Add(obj);
                }

                db.SaveChanges();
                log.Info("Database saved successfully");

                // ✅ FIXED PART
                if (model.ResponseText?.Trim().ToLower() == "interested")
                {
                    await LeadHelper.FindAndProcessLead(model.MobileNumber, model.CampaignName);
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Response Saved Successfully");
            }
            catch (Exception ex)
            {
                log.Error("Error in SaveResponse API", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ✅ URL: /api/CampaignResponseApi
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            log.Info("GetAll API called");

            var data = db.CampaignResponses.ToList();

            log.Info($"Total records: {data.Count}");

            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        // ✅ URL: /api/CampaignResponseApi/5
        [HttpGet]
        [Route("api/CampaignResponseApi/{id:int}")]
        public HttpResponseMessage GetById(int id)
        {
            log.Info($"GetById called Id={id}");

            var record = db.CampaignResponses.Find(id);

            if (record == null)
            {
                log.Warn($"Record not found Id={id}");
                return Request.CreateResponse(HttpStatusCode.NotFound, "Record not found");
            }

            return Request.CreateResponse(HttpStatusCode.OK, record);
        }

        // ✅ URL: /api/CampaignResponseApi
        [HttpPost]
        public HttpResponseMessage Add(CampaignRespons model)
        {
            log.Info("Add API called");

            if (model == null)
            {
                log.Warn("Invalid model");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid data");
            }

            model.CreatedDate = DateTime.Now;
            db.CampaignResponses.Add(model);
            db.SaveChanges();

            log.Info("Record inserted successfully");

            return Request.CreateResponse(HttpStatusCode.OK, "Inserted successfully");
        }


        //private async Task FindAndProcessLead(string mobileNumber, string campaignName)
        //{
        //    try
        //    {
        //        log.Info($"FindAndProcessLead called: Mobile={mobileNumber}, Campaign={campaignName}");

        //        // ✅ Mobile normalize — last 10 digits
        //        string normalizedMobile = mobileNumber?.Length > 10
        //            ? mobileNumber.Substring(mobileNumber.Length - 10)
        //            : mobileNumber;

        //        // ✅ CENTRAL_BLAST se find karo
        //        var blastRecord = db.CENTRAL_BLAST
        //            .FirstOrDefault(x =>
        //                x.CampaignName == campaignName &&
        //                x.Contact_Number.EndsWith(normalizedMobile));

        //        if (blastRecord == null)
        //        {
        //            log.Warn($"CENTRAL_BLAST not found: Mobile={normalizedMobile}, Campaign={campaignName}");
        //            return; // ✅ Task method mein sirf return likho
        //        }

        //        string channelCode = blastRecord.Channel?.Trim().ToUpper();
        //        string sapCode = blastRecord.L1_Code?.Trim();
        //        string userName = blastRecord.Name?.Trim();
        //        string userMobile = normalizedMobile;

        //        log.Info($"CENTRAL_BLAST found: Channel={channelCode}, SAP={sapCode}");

        //        if (string.IsNullOrEmpty(channelCode) || string.IsNullOrEmpty(sapCode))
        //        {
        //            log.Warn("Channel or SAP Code empty");
        //            return; // ✅
        //        }

        //        // ✅ Hierarchy se fetch karo
        //        var empData = db.NEW_TEMP_HIERARCHY
        //            .FirstOrDefault(x =>
        //                x.X_BM_EMP_CD == sapCode ||
        //                x.X_ZM_EMP_CD == sapCode ||
        //                x.X_RM_EMP_EMP_CD == sapCode ||
        //                x.X_SM_EMP_CD == sapCode);

        //        if (empData == null)
        //        {
        //            log.Warn($"Hierarchy not found for SAP={sapCode}");
        //            return; // ✅
        //        }

        //        // ✅ SAP Name resolve
        //        string sapName = blastRecord.L1_Name?.Trim();

        //        if (empData.X_BM_EMP_CD == sapCode)
        //            sapName = empData.X_BM_NM;
        //        else if (empData.X_ZM_EMP_CD == sapCode)
        //            sapName = empData.X_ZM_NM;
        //        else if (empData.X_RM_EMP_EMP_CD == sapCode)
        //            sapName = empData.X_RM_NM;
        //        else if (empData.X_SM_EMP_CD == sapCode)
        //            sapName = empData.X_SM_NM;

        //        log.Info($"SapName={sapName}");

        //        // ✅ Category fetch
        //        var categoryName = (from cm in db.campaign_master
        //                            join cc in db.campaign_category
        //                            on cm.campaign_category_id equals cc.campaign_category_id
        //                            where cm.channel_code == channelCode
        //                            select cc.campaign_category_name)
        //                            .FirstOrDefault();

        //        log.Info($"Category={categoryName}");

        //        // ✅ Channel wise API call
        //        var lsqChannels = new List<string> { "GR", "DL", "DP", "PC", "PM", "EN" };
        //        var seChannels = new List<string> { "AG", "CN", "GR", "NV", "ST", "CD", "EN" };

        //        if (lsqChannels.Contains(channelCode))
        //        {
        //            log.Info($"Calling LSQ API: Channel={channelCode}");
        //            var result = await TestAPI(userName, userMobile, sapCode, sapName);
        //            log.Info($"LSQ API done: RequestId={result.Response?.RequestId}");
        //        }
        //        else if (seChannels.Contains(channelCode))
        //        {
        //            log.Info($"Calling SE API: Channel={channelCode}");
        //            var result = await TestAPI_SE(userName, userMobile, sapCode, sapName, categoryName);
        //            log.Info($"SE API done: RequestId={result.Response?.RequestId}");
        //        }
        //        else
        //        {
        //            log.Warn($"Channel={channelCode} not mapped");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("Error in FindAndProcessLead", ex);
        //    }
        //}
        // ✅ WABA Provider Response Model — Nested Object Support
        public class CampaignResponseModel
        {
            public string MobileNumber { get; set; }
            public string CampaignName { get; set; }
            public ResponsePayload Response { get; set; }

            // ✅ "Not Interested" / "Interested" text nikalti hai automatically
            public string ResponseText => Response?.text ?? Response?.payload;
        }

        // ✅ WABA Provider nested Response object
        public class ResponsePayload
        {
            public string payload { get; set; }
            public string text { get; set; }
        }
    }
}