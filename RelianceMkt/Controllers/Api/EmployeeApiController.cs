using RelianceMkt.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RelianceMkt.Controllers.Api
{
    public class CampaignResponseApiController : ApiController
    {
        public rglinixm_relEntities db = new rglinixm_relEntities();


        // URL: /api/CampaignResponseApi/SaveResponse
        [HttpPost]
        [Route("api/CampaignResponseApi/SaveResponse")] 
        public HttpResponseMessage SaveResponse(CampaignResponseModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.MobileNumber))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");
                }

                // 🔎 Check existing record (same Mobile + Campaign)
                var existing = db.CampaignResponses
                    .FirstOrDefault(x =>
                        x.Mobile == model.MobileNumber &&
                        x.CampaignName == model.CampaignName);

                if (existing != null)
                {
                    // 🔄 UPDATE (user replied Interested / Not Interested)
                    existing.Response = model.Response;
                    existing.CreatedDate = DateTime.Now;
                }
                else
                {
                    // ➕ INSERT (fallback case)
                    CampaignRespons obj = new CampaignRespons
                    {
                        Mobile = model.MobileNumber,
                        CampaignName = model.CampaignName,
                        Response = model.Response,
                        CreatedDate = DateTime.Now
                    };

                    db.CampaignResponses.Add(obj);
                }

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Response Saved Successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }




        // 🔹 1️⃣ GET : Fetch All Campaign Responses
        // URL: /api/CampaignResponseApi
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            var data = db.CampaignResponses.ToList();
            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        // 🔹 2️⃣ GET : Fetch By Id
        // URL: /api/CampaignResponseApi/5
        [HttpGet]
        public HttpResponseMessage GetById(int id)
        {
            var record = db.CampaignResponses.Find(id);

            if (record == null)
            {
                return Request.CreateResponse(
                    HttpStatusCode.NotFound,
                    "Record not found"
                );
            }

            return Request.CreateResponse(HttpStatusCode.OK, record);
        }

        // 🔹 3️⃣ POST : Insert Campaign Response
        // URL: /api/CampaignResponseApi
        [HttpPost]
        public HttpResponseMessage Add(CampaignRespons model)
        {
            if (model == null)
            {
                return Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    "Invalid data"
                );
            }

            model.CreatedDate = System.DateTime.Now;

            db.CampaignResponses.Add(model);
            db.SaveChanges();

            return Request.CreateResponse(
                HttpStatusCode.OK,
                "Campaign response inserted successfully"
            );
        }

        public class CampaignResponseModel
        {
            public string MobileNumber { get; set; }
            public string CampaignName { get; set; }
            public string Response { get; set; } // Interested / Not Interested
        }
    }
}
