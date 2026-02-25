using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using log4net;
using Newtonsoft.Json;
//using OfficeOpenXml;
using RelianceMkt.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.WebControls;
//using static OfficeOpenXml.ExcelErrorValue;
using static RelianceMkt.Models.CustomModel;

namespace RelianceMkt.Controllers
{
    public class MarketingController : Controller
    {



        public rglinixm_relEntities db = new rglinixm_relEntities();
        private string defaultValue;


        private static readonly ILog logger = LogManager.GetLogger(typeof(MarketingController));

        //public async Task<ActionResult> Index()
        public ActionResult Index()
        {
            logger.Info("Logger Start");

            //var client = new HttpClient();
            //var request = new HttpRequestMessage(HttpMethod.Post, "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27");
            //var content = new StringContent("{\"LeadDetails\":[{\"Attribute\":\"SearchBy\",\"Value\":\"Phone\"},{\"Attribute\":\"__UseUserDefinedGuid__\",\"Value\":\"true\"},{\"Attribute\":\"Source\",\"Value\":\"DIGIMYIN\"},{\"Attribute\":\"Phone\",\"Value\":\"9875478985\"}],\"Opportunity\":{\"OpportunityEventCode\":12000,\"OpportunityNote\":\"Opportunity\",\"OverwriteFields\":true,\"DoNotPostDuplicateActivity\":false,\"DoNotChangeOwner\":true,\"Fields\":[{\"SchemaName\":\"mx_Custom_2\",\"Value\":\"New Lead\"},{\"SchemaName\":\"Status\",\"Value\":\"Open\"},{\"SchemaName\":\"mx_Custom_11\",\"Value\":\"Product Campaign\"},{\"SchemaName\":\"mx_Custom_1\",\"Value\":\"vikas\"},{\"SchemaName\":\"mx_Custom_35\",\"Value\":\"\"},{\"SchemaName\":\"mx_Custom_13\",\"Value\":\"1990-01-01\"},{\"SchemaName\":\"mx_Custom_12\",\"Value\":\"New Business\"},{\"SchemaName\":\"mx_Custom_43\",\"Value\":\"Reliance Nippon Life Super Suraksha\"},{\"SchemaName\":\"mx_Custom_44\",\"Value\":\"Health Plan\"},{\"SchemaName\":\"mx_Custom_45\",\"Value\":\"188\"},{\"SchemaName\":\"mx_Custom_15\",\"Value\":\"Up To 3 Lacs\"},{\"SchemaName\":\"mx_Custom_54\",\"Value\":\"Individual\"},{\"SchemaName\":\"mx_Custom_27\",\"Value\":\"DIGIMYIN\"},{\"SchemaName\":\"mx_Custom_46\",\"Value\":\"\",\"Fields\":[{\"SchemaName\":\"mx_CustomObject_1\",\"Value\":\"API\"},{\"SchemeName\":\"mx_Custom_36.mx_CustomObject_1\",\"Value\":\"70657623\"},{\"SchemaName\":\"mx_CustomObject_2\",\"Value\":\"DM\"}]}]}}", null, "application/json");
            //request.Content = content;
            //var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            //Console.WriteLine(await response.Content.ReadAsStringAsync());

            if (Session["userid"] != null)
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View();
            }

        }


        //================resertPassword==============

        public ActionResult ResetPassword()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("ResetPassword", "Marketing");
            }
            return View();
        }
        [HttpPost]
        public JsonResult ResetPasswordAjax(string oldPassword, string newPassword, string confirmPassword)
        {
            if (Session["userid"] == null)
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }

            decimal userId;
            if (!decimal.TryParse(Session["userid"].ToString(), out userId))
            {
                return Json(new { success = false, message = "Invalid session user ID." });
            }

            // 🔹 Fetch user from DB
            var user = db.authentications.FirstOrDefault(x => x.au_id == userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // ✅ Allow only Admin & SubAdmin
            if (user.Role != "Admin" && user.Role != "SubAdmin")
            {
                return Json(new { success = false, message = "Only Admin or SubAdmin can reset password." });
            }

            // Old password check
            if (user.au_password != oldPassword)
            {
                return Json(new { success = false, message = "Old password is incorrect." });
            }

            // Confirm password check
            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "New password and confirm password do not match." });
            }

            // ✅ Update password
            user.au_password = newPassword;
            db.SaveChanges();

            return Json(new { success = true, message = "Password reset successfully." });
        }



        public ActionResult Logout()
        {
            Session.Remove("userid");
            Session.Remove("username");
            return RedirectToAction("Index");

        }

        public ActionResult LogoutUser()
        {
            Session.Remove("SAPCODE");
            Session.Remove("LOGIN");
            Session.Remove("TYPE");
            return RedirectToAction("Index");

        }



        //======================== login ============kamlesh========

        //[HttpPost]
        //public ActionResult Index(authentication data)
        //{
        //    // Query to check username, password
        //    var qry = db.authentications
        //        .FirstOrDefault(x => x.au_username == data.au_username && x.au_password == data.au_password);

        //    if (qry != null)
        //    {
        //        // Store user details in session
        //        Session["userid"] = qry.au_id.ToString();
        //        Session["username"] = qry.au_username;
        //        Session["role"] = qry.Role;
        //        Session["subadmin_type"] = ""; // reset initially

        //        // Admin and SubAdmin (normal)
        //        if (qry.Role == "Admin" || qry.Role == "SubAdmin")
        //        {
        //            return RedirectToAction("Dashboard", "Marketing");
        //        }
        //        // Sub-Admin(Read-Only)
        //        else if (qry.Role == "Sub-Admin(Read-Only)")
        //        {
        //            if (string.IsNullOrEmpty(qry.Subchannel))
        //            {
        //                TempData["warning"] = "No subchannel assigned for this Sub-Admin!";
        //                return View();
        //            }

        //            Session["subadmin_type"] = "ReadOnly";
        //            Session["subchannel"] = qry.Subchannel; // Only for Read-Only

        //            return RedirectToAction("Dashboard", "Marketing");
        //        }
        //        // Sub-Admin(Campaign Creator)
        //        else if (qry.Role == "Sub-Admin(Campaign Creator)")
        //        {
        //            Session["subadmin_type"] = "CampaignCreator";
        //            // ❌ No subchannel check here

        //            return RedirectToAction("Dashboard", "Marketing");
        //        }
        //        else
        //        {
        //            TempData["warning"] = "Unauthorized role!";
        //            return View();
        //        }
        //    }
        //    else
        //    {
        //        TempData["warning"] = "Invalid credentials!";
        //        return View();
        //    }
        //}


        [HttpPost]
        public ActionResult Index(authentication data)
        {
            // 1️⃣ User check
            var qry = db.authentications
                .FirstOrDefault(x => x.au_username == data.au_username
                                  && x.au_password == data.au_password);

            // ❌ Invalid credentials
            if (qry == null)
            {
                TempData["warning"] = "Invalid credentials!";
                return View();
            }

            // 2️⃣ SESSION SET (IMPORTANT)
            Session["userid"] = qry.au_id;            // ❗ string में convert मत करो
            Session["username"] = qry.au_username;
            Session["role"] = qry.Role;
            Session["subadmin_type"] = "";
            Session["subchannel"] = null;

            // 3️⃣ ROLE BASED REDIRECT

            // ✅ Admin / SubAdmin (Normal)
            if (qry.Role == "Admin" || qry.Role == "SubAdmin")
            {
                return RedirectToAction("Dashboard", "Marketing");
            }

            // ✅ Sub-Admin (Read Only)
            if (qry.Role == "Sub-Admin(Read-Only)")
            {
                if (string.IsNullOrEmpty(qry.Subchannel))
                {
                    TempData["warning"] = "No subchannel assigned for this Sub-Admin!";
                    Session.Clear(); // ❗ session clear
                    return RedirectToAction("Index");
                }

                Session["subadmin_type"] = "ReadOnly";
                Session["subchannel"] = qry.Subchannel;

                return RedirectToAction("Dashboard", "Marketing");
            }

            // ✅ Sub-Admin (Campaign Creator)
            //if (qry.Role == "Sub-Admin(Campaign Creator)")
            //{
            //    if (string.IsNullOrEmpty(qry.Subchannel))
            //    {
            //        TempData["warning"] = "No subchannel assigned for this Sub-Admin!";
            //        Session.Clear(); // ❗ session clear
            //        return RedirectToAction("Index");
            //    }

            //    Session["subadmin_type"] = "CampaignCreator";
            //    Session["subchannel"] = qry.Subchannel;

            //    return RedirectToAction("Dashboard", "Marketing");
            //}
            if (qry.Role == "Sub-Admin(Campaign Creator)")
            {
                // Campaign Creator ke liye subchannel required nahi hai
                Session["subadmin_type"] = "CampaignCreator";
                Session["subchannel"] = qry.Subchannel; // null bhi ho sakta hai

                return RedirectToAction("Dashboard", "Marketing");
            }
            else if (qry.Role == "Sub-Admin")
            {
                // Normal Sub-Admin ke liye subchannel required hai
                if (string.IsNullOrWhiteSpace(qry.Subchannel))
                {
                    TempData["warning"] = "No subchannel assigned for this Sub-Admin!";
                    return RedirectToAction("Index");
                }

                Session["subadmin_type"] = "SubAdmin";
                Session["subchannel"] = qry.Subchannel;

                return RedirectToAction("Dashboard", "Marketing");
            }

            // ❌ Unknown / Unauthorized role
            TempData["warning"] = "Unauthorized role!";
            Session.Clear();
            return RedirectToAction("Index");
        }






        private readonly rglinixm_relEntities _context;

        // Constructor with dependency injection (optional, for better practice)
        public MarketingController()
        {
            _context = new rglinixm_relEntities();
        }

        // Optional: Dispose the context


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult AddSubAdmin()
        {
            try
            {
                // Fetch distinct subchannels using EF
                var subchannels = _context.NEW_TEMP_HIERARCHY
                    .Where(h => h.SUBCHANNEL != null)
                    .Select(h => h.SUBCHANNEL)
                    .Distinct()
                    .ToList();

                ViewBag.SubChannels = subchannels;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error fetching subchannels: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public JsonResult GetSubChannels()
        {
            try
            {
                // Fetch distinct subchannels using EF
                var subchannels = _context.NEW_TEMP_HIERARCHY
                    .Where(h => h.SUBCHANNEL != null)
                    .Select(h => h.SUBCHANNEL)
                    .Distinct()
                    .ToList();

                return Json(subchannels, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error fetching subchannels: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public JsonResult CreateUserAjax(string username, string password, string role, string subchannel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
                {
                    return Json(new { success = false, message = "All fields are required." });
                }

                // ✅ Subchannel सिर्फ Read-Only वाले sub-admin के लिए required
                if (role == "Sub-Admin(Read-Only)" && string.IsNullOrWhiteSpace(subchannel))
                {
                    return Json(new { success = false, message = "Subchannel is required for Sub-Admin (Read-Only)." });
                }

                // Create a new authentication record
                var newUser = new authentication
                {
                    au_username = username,
                    au_password = password, // Note: Consider hashing the password
                    Role = role,
                    au_delflag = "",
                    Subchannel = role == "Sub-Admin(Read-Only)" ? subchannel : ""  // ✅ सिर्फ read-only पर save करो
                };

                _context.authentications.Add(newUser);
                int rowsAffected = _context.SaveChanges();

                if (rowsAffected > 0)
                {
                    return Json(new { success = true, message = "User created successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create user." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        //====================
        public ActionResult CampaignCategoryList()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.lstCategory = db.campaign_category
                                         .Where(x => x.campaign_category_delflag == null
                                                 && x.Campaign_Category_Status == "0") // 👈 Only active
                                        .OrderBy(x => x.campaign_category_name.ToLower())
                                        .ToList();

                //return View();
                return View(new RelianceMkt.Models.campaign_category());
            }
        }

        [HttpPost]
        public ActionResult CampaignCategoryList(campaign_category data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    // 🔹 Correct property name use karo
                    data.Campaign_Category_Status = "0";   // Default 0
                    data.campaign_category_delflag = null; // Not deleted

                    db.campaign_category.Add(data);
                    db.SaveChanges();
                }

                return RedirectToAction("CampaignCategoryList");
            }
        }



        [HttpPost]
        public JsonResult DeleteCampaignCategory(int id)
        {
            try
            {
                var category = db.campaign_category.FirstOrDefault(x => x.campaign_category_id == id);
                if (category != null)
                {

                    category.Campaign_Category_Status = "1";
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Category not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        //====================correct========================//
        public ActionResult CampaignList()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                // Category ko alphabetically order by name
                //ViewBag.lstCategory = db.campaign_category
                //                        .Where(x => x.campaign_category_delflag == null)
                //                        .OrderBy(x => x.campaign_category_name)
                //                        .ToList();
                ViewBag.lstcategory = db.campaign_category
                                    .Where(x => x.campaign_category_delflag == null
                                             && x.Campaign_Category_Status == "0")
                                    .OrderBy(x => x.campaign_category_name.ToLower())
                                    .ToList();

                ViewBag.lstCampaign = (from a in db.campaigns
                                       join b in db.campaign_category on a.campaign_category_id equals b.campaign_category_id
                                       where a.campaign_delflag == null
                                       orderby b.campaign_category_name  // Yaha bhi order apply
                                       select new CustomModel.ViewModelCampaignCategory
                                       {
                                           cam = a,
                                           cc = b
                                       }).ToList();

                return View();
                //return View(new campaign());
            }
        }

        [HttpPost]
        public ActionResult CampaignList(campaign data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                // Category ko alphabetically order by name
                ViewBag.lstCategory = db.campaign_category
                                        .Where(x => x.campaign_category_delflag == null)
                                        .OrderBy(x => x.campaign_category_name.ToLower())
                                        .ToList();

                ViewBag.lstCampaign = (from a in db.campaigns
                                       join b in db.campaign_category on a.campaign_category_id equals b.campaign_category_id
                                       where a.campaign_delflag == null
                                       orderby b.campaign_category_name  // Yaha bhi order apply
                                       select new CustomModel.ViewModelCampaignCategory
                                       {
                                           cam = a,
                                           cc = b
                                       }).ToList();

                db.campaigns.Add(data);
                if (db.SaveChanges() > 0)
                {
                    return RedirectToAction("CampaignList");
                }
                else
                {
                    return View();
                }
            }
        }
        //===============================================================//

        [HttpPost]
        public JsonResult DeleteCampaign(int campaignId)
        {
            if (Session["userid"] == null)
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            var campaign = db.campaigns.FirstOrDefault(x => x.campaign_id == campaignId && x.campaign_delflag == null);

            if (campaign != null)
            {
                campaign.campaign_delflag = "true"; // Soft delete
                                                    //db.Entry(campaign).State = EntityState.Modified;
                                                    //db.Entry(campaign).State = EntityState.Modified;
                db.Entry(campaign).State = System.Data.Entity.EntityState.Modified;



                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Campaign not found." });
        }

        //public ActionResult SubCampaignList()
        //{
        //    if (Session["userid"] == null)
        //    {
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        ViewBag.lstCampaign = db.campaigns
        //                                .Where(x => x.campaign_delflag == null)
        //                                 .OrderBy(x => x.campaign_name)
        //                                .ToList();

        //        ViewBag.lstSubCampaign = (from a in db.campaigns
        //                                  join b in db.subcampaigns on a.campaign_id equals b.campaign_id
        //                                  where a.campaign_delflag == null
        //                                  orderby b.subcampaign_name // <-- OrderBy lagaya
        //                                  select new CustomModel.ViewModelCampaignCategory
        //                                  {
        //                                      cam = a,
        //                                      sc = b
        //                                  }).ToList();

        //        return View();
        //    }
        //}

        //[HttpPost]
        //public ActionResult SubCampaignList(subcampaign data)
        //{
        //    if (Session["userid"] == null)
        //    {
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        ViewBag.lstCampaign = db.campaigns
        //                                .Where(x => x.campaign_delflag == null)
        //                                 .OrderBy(x => x.campaign_name)
        //                                .ToList();

        //        ViewBag.lstSubCampaign = (from a in db.campaigns
        //                                  join b in db.subcampaigns on a.campaign_id equals b.campaign_id
        //                                  where a.campaign_delflag == null
        //                                  orderby b.subcampaign_name // <-- OrderBy lagaya
        //                                  select new CustomModel.ViewModelCampaignCategory
        //                                  {
        //                                      cam = a,
        //                                      sc = b
        //                                  }).ToList();

        //        db.subcampaigns.Add(data);
        //        if (db.SaveChanges() > 0)
        //        {
        //            return RedirectToAction("SubCampaignList");
        //        }
        //        else
        //        {
        //            return View();
        //        }
        //    }
        //}

        public ActionResult SubCampaignList()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Campaign Dropdown
            ViewBag.lstCampaign = db.campaigns
                                    .Where(x => x.campaign_delflag == null)
                                    .OrderBy(x => x.campaign_name)
                                    .ToList();

            // SubCampaign list (only NON-deleted)
            //ViewBag.lstSubCampaign = (from a in db.campaigns
            //                          join b in db.subcampaigns on a.campaign_id equals b.campaign_id
            //                          where a.campaign_delflag == null &&
            //                               (b.subcampaign_delflag == null)
            //                          orderby b.subcampaign_name
            //                          select new CustomModel.ViewModelCampaignCategory
            //                          {
            //                              cam = a,
            //                              sc = b
            //                          }).ToList();

            //return View();
            ViewBag.lstSubCampaign = (
    from a in db.campaigns
    join b in db.subcampaigns on a.campaign_id equals b.campaign_id
    where
        a.campaign_delflag == null &&         // campaign active
        (b.subcampaign_delflag == null)       // only NULL Sub-campaigns
    orderby b.subcampaign_name
    select new CustomModel.ViewModelCampaignCategory
    {
        cam = a,
        sc = b
    }
).ToList();

            return View();
            //return View(new subcampaign());


        }
        [HttpPost]
        public ActionResult SubCampaignList(subcampaign data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Do NOT set delete flag → it will remain NULL
            data.subcampaign_delflag = null;

            db.subcampaigns.Add(data);
            db.SaveChanges();

            return RedirectToAction("SubCampaignList");
        }



        public ActionResult Dashboard()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {

                ViewData["categories"] = db.campaign_category.Where(x => x.campaign_category_delflag == null).Count();
                //ViewData["campaigns"] = db.campaigns.Where(x => x.campaign_delflag == null).Count();
                ViewData["campaigns"] = db.campaign_master.Where(x => x.camapaign_master_status == "Active").Count();

                ViewData["subcampaigns"] = db.subcampaigns.Where(x => x.subcampaign_delflag == null).Count();
                ViewData["totalleads"] = db.Leads.Count();
                //ViewData["total hotleads"] = db.hotleads.Count();
                ViewData["totalshare"] = db.ENGAGE_SHARECOUNT.Count();

                ViewData["creatives"] = db.campaign_master.Count();
                return View();
            }
        }



        public void SetData()
        {

            //string Constr = ConfigurationManager.ConnectionStrings["Rel_connection"].ToString();

            //string Constr = "Data Source=10.126.143.86,1981;Initial Catalog=Webinar;User ID=reliance_user;Password=pass@123;MultipleActiveResultSets=True;Connection Timeout=10000;";
            string Constr = "Data Source=10.126.143.86,1981;Initial Catalog=DIGIMYIN;User ID=reliance_user;Password=pass@123;MultipleActiveResultSets=True;Connection Timeout=10000;";

            SqlConnection con = new SqlConnection(Constr);
            string SAPCODE = Session["SAPCODE"]?.ToString();
            string LOGIN = Session["LOGIN"]?.ToString();
            string TYPE = Session["TYPE"]?.ToString();


            decimal selfshare = 0;
            decimal selflead = 0;
            decimal teamshare = 0;
            decimal teamslead = 0;


            List<SelectListItem> downline = new List<SelectListItem>();

            switch (TYPE)
            {
                case "CH":
                    break;

                case "ZM":

                    ViewBag.NextType = "RM";
                    ViewBag.PrevType = "ZM";
                    var zm = db.NEW_TEMP_HIERARCHY.Where(x => x.X_ZM_EMP_CD == SAPCODE).FirstOrDefault();
                    if (zm != null)
                    {
                        ViewBag.username = zm.X_ZM_NM;
                        ViewBag.zone = zm.X_ZONE;
                        ViewBag.ZM_NAME = zm.X_ZONE;
                        ViewBag.ZM_CODE = zm.X_ZM_EMP_CD;
                        ViewBag.RM_NAME = zm.X_REGION;
                        ViewBag.RM_CODE = zm.X_RM_EMP_EMP_CD;
                        ViewBag.BM_NAME = zm.X_SALES_UNIT_NM;
                        ViewBag.BM_CODE = zm.X_SALES_UNIT_CD;
                        ViewBag.ARDM_NAME = zm.X_SM_NM;
                        ViewBag.ARDM_CODE = zm.X_SM_EMP_CD;
                        ViewBag.AGENT_CODE = zm.AGENT_CODE;
                        ViewBag.AGENT_NAME = zm.AGENT_NAME;

                    }

                    selfshare = db.ENGAGE_SHARECOUNT.Where(x => x.SHC_SAPCODE == SAPCODE).Count();
                    selflead = db.Leads.Where(x => x.leads_sapcode == SAPCODE).Count();

                    var listRM = (from a in db.NEW_TEMP_HIERARCHY
                                  where a.X_ZM_EMP_CD == SAPCODE && a.X_SM_STATUS == "IF" && a.X_RM_EMP_EMP_CD != "77777777"

                                  select new
                                  {
                                      a.X_REGION,

                                      a.X_RM_EMP_EMP_CD
                                  }
                                  ).ToList().Distinct();
                    if (listRM != null)
                    {

                        foreach (var item in listRM)
                        {
                            downline.Add(new SelectListItem { Text = item.X_REGION, Value = item.X_RM_EMP_EMP_CD });
                        }

                        ViewBag.lstDownline = downline;
                    }


                    var lstRegionalManagers = db.NEW_TEMP_HIERARCHY.Where(x => x.X_ZM_EMP_CD == SAPCODE).Select(x => x.X_RM_EMP_EMP_CD).ToList().Distinct();


                    string sql_zm_teamleads = @"SELECT COUNT(leads_id) FROM Leads  where leads_sapcode IN( 
                                    SELECT X_RM_EMP_EMP_CD FROM NEW_TEMP_HIERARCHY where X_ZM_EMP_CD=" + SAPCODE + " AND X_SM_STATUS='IF'  AND X_RM_EMP_EMP_CD!='77777777'"
                                   + " UNION SELECT X_BM_EMP_CD FROM NEW_TEMP_HIERARCHY where X_RM_EMP_EMP_CD IN(SELECT X_RM_EMP_EMP_CD FROM NEW_TEMP_HIERARCHY where X_ZM_EMP_CD= " + SAPCODE + " AND X_SM_STATUS='IF'  AND X_RM_EMP_EMP_CD!='77777777') AND X_SM_STATUS='IF'  AND X_BM_EMP_CD!='77777777'"
                                   + "  UNION SELECT X_SM_EMP_CD FROM NEW_TEMP_HIERARCHY where X_BM_EMP_CD IN(SELECT X_BM_EMP_CD FROM NEW_TEMP_HIERARCHY where X_RM_EMP_EMP_CD IN(SELECT X_RM_EMP_EMP_CD FROM NEW_TEMP_HIERARCHY where X_ZM_EMP_CD= " + SAPCODE + " AND X_SM_STATUS='IF'  AND X_RM_EMP_EMP_CD!='77777777') AND X_SM_STATUS='IF'  AND X_BM_EMP_CD!='77777777') AND X_SM_STATUS='IF'  AND X_SM_EMP_CD!='77777777')";

                    con.Open();
                    SqlCommand cmd = new SqlCommand(sql_zm_teamleads, con);
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter ad = new SqlDataAdapter();
                    ad.SelectCommand = cmd;
                    teamslead = Convert.ToDecimal(cmd.ExecuteScalar());
                    con.Close();

                    string sql_ZM_teamshare = @"SELECT COUNT(SHC_SHARECOUNT)  FROM ENGAGE_SHARECOUNT  where SHC_SAPCODE IN( 
                                    SELECT X_RM_EMP_EMP_CD FROM NEW_TEMP_HIERARCHY where X_ZM_EMP_CD=" + SAPCODE + " AND X_SM_STATUS='IF'  AND X_RM_EMP_EMP_CD!='77777777'"
                                   + " UNION SELECT X_BM_EMP_CD FROM NEW_TEMP_HIERARCHY where X_RM_EMP_EMP_CD IN(SELECT X_RM_EMP_EMP_CD FROM NEW_TEMP_HIERARCHY where X_ZM_EMP_CD= " + SAPCODE + " AND X_SM_STATUS='IF'  AND X_RM_EMP_EMP_CD!='77777777') AND X_SM_STATUS='IF'  AND X_BM_EMP_CD!='77777777'"
                                   + "  UNION SELECT X_SM_EMP_CD FROM NEW_TEMP_HIERARCHY where X_BM_EMP_CD IN(SELECT X_BM_EMP_CD FROM NEW_TEMP_HIERARCHY where X_RM_EMP_EMP_CD IN(SELECT X_RM_EMP_EMP_CD FROM NEW_TEMP_HIERARCHY where X_ZM_EMP_CD= " + SAPCODE + " AND X_SM_STATUS='IF'  AND X_RM_EMP_EMP_CD!='77777777') AND X_SM_STATUS='IF'  AND X_BM_EMP_CD!='77777777') AND X_SM_STATUS='IF'  AND X_SM_EMP_CD!='77777777')";
                    con.Open();
                    cmd = new SqlCommand(sql_ZM_teamshare, con);
                    cmd.CommandType = CommandType.Text;
                    ad.SelectCommand = cmd;
                    teamshare = Convert.ToDecimal(cmd.ExecuteScalar());
                    con.Close();


                    break;



                case "RM":
                    ViewBag.NextType = "BM";
                    ViewBag.PrevType = "RM";
                    var rm = db.NEW_TEMP_HIERARCHY.Where(x => x.X_RM_EMP_EMP_CD == SAPCODE).FirstOrDefault();
                    if (rm != null)
                    {
                        ViewBag.username = rm.X_RM_NM;
                        ViewBag.zone = rm.X_REGION;
                        ViewBag.ZM_NAME = rm.X_ZONE;
                        ViewBag.ZM_CODE = rm.X_ZM_EMP_CD;
                        ViewBag.RM_NAME = rm.X_REGION;
                        ViewBag.RM_CODE = rm.X_RM_EMP_EMP_CD;
                        ViewBag.BM_NAME = rm.X_SALES_UNIT_NM;
                        ViewBag.BM_CODE = rm.X_BM_EMP_CD;
                        ViewBag.ARDM_NAME = rm.X_SM_NM;
                        ViewBag.ARDM_CODE = rm.X_SM_EMP_CD;
                        ViewBag.AGENT_CODE = rm.AGENT_CODE;
                        ViewBag.AGENT_NAME = rm.AGENT_NAME;
                    }
                    var listBM = (from a in db.NEW_TEMP_HIERARCHY
                                  where a.X_RM_EMP_EMP_CD == SAPCODE && a.X_SM_STATUS == "IF" && a.X_BM_EMP_CD != "77777777"
                                  select new
                                  {
                                      a.X_SALES_UNIT_NM,
                                      a.X_BM_EMP_CD
                                  }
                                  ).ToList().Distinct();
                    if (listBM != null)
                    {

                        foreach (var item in listBM)
                        {
                            downline.Add(new SelectListItem { Text = item.X_SALES_UNIT_NM, Value = item.X_BM_EMP_CD });
                        }

                        ViewBag.lstDownline = downline;
                    }

                    var lstBranchManagers = db.NEW_TEMP_HIERARCHY.Where(x => x.X_RM_EMP_EMP_CD == SAPCODE).Select(x => x.X_BM_EMP_CD).ToList().Distinct();

                    selfshare = db.ENGAGE_SHARECOUNT.Where(x => x.SHC_SAPCODE == SAPCODE).Count();
                    selflead = db.Leads.Where(x => x.leads_sapcode == SAPCODE).Count();

                    //teamshare = Convert.ToDecimal((from a in db.ENGAGE_SHARECOUNT
                    //                               where lstBranchManagers.Contains(a.SHC_SAPCODE)
                    //                               select a.SHC_SHARECOUNT).Count());


                    //teamslead = Convert.ToDecimal((from a in db.Leads
                    //                               where lstBranchManagers.Contains(a.leads_sapcode)
                    //                               select a.leads_id).Count());


                    string sql = @"SELECT COUNT(leads_id)  FROM Leads  where leads_sapcode IN( SELECT X_BM_EMP_CD FROM NEW_TEMP_HIERARCHY WHERE  X_SM_STATUS='IF' AND X_RM_EMP_EMP_CD=" + SAPCODE + " AND X_RM_EMP_EMP_CD!='7777777' UNION SELECT X_SM_EMP_CD FROM NEW_TEMP_HIERARCHY WHERE X_BM_EMP_CD IN(SELECT X_BM_EMP_CD FROM NEW_TEMP_HIERARCHY WHERE  X_SM_STATUS='IF' AND X_RM_EMP_EMP_CD=" + SAPCODE + " AND X_RM_EMP_EMP_CD!='7777777') AND X_SM_STATUS='IF'  AND X_RM_EMP_EMP_CD!='7777777')";
                    //string Constr = ConfigurationManager.ConnectionStrings["Rel_connection"].ToString();

                    //SqlConnection con = new SqlConnection(Constr);
                    con.Open();
                    SqlCommand cmd_sm = new SqlCommand(sql, con);
                    cmd_sm.CommandType = CommandType.Text;

                    SqlDataAdapter ad_sm = new SqlDataAdapter();
                    ad_sm.SelectCommand = cmd_sm;
                    teamslead = Convert.ToDecimal(cmd_sm.ExecuteScalar());
                    con.Close();

                    string sql_teamshare = @"SELECT COUNT(SHC_SHARECOUNT)  FROM ENGAGE_SHARECOUNT  where SHC_SAPCODE IN( SELECT X_BM_EMP_CD FROM NEW_TEMP_HIERARCHY WHERE  X_SM_STATUS='IF' AND X_RM_EMP_EMP_CD=" + SAPCODE + " AND X_RM_EMP_EMP_CD!='7777777' UNION SELECT X_SM_EMP_CD FROM NEW_TEMP_HIERARCHY WHERE X_BM_EMP_CD IN(SELECT X_BM_EMP_CD FROM NEW_TEMP_HIERARCHY WHERE  X_SM_STATUS='IF' AND X_RM_EMP_EMP_CD=" + SAPCODE + " AND X_RM_EMP_EMP_CD!='7777777') AND X_SM_STATUS='IF'  AND X_RM_EMP_EMP_CD!='7777777')";

                    con.Open();
                    cmd = new SqlCommand(sql_teamshare, con);
                    cmd.CommandType = CommandType.Text;


                    ad_sm.SelectCommand = cmd;
                    teamshare = Convert.ToDecimal(cmd.ExecuteScalar());
                    con.Close();



                    break;



                case "BM":

                    ViewBag.NextType = "ARDM";
                    ViewBag.PrevType = "BM";
                    var bm = db.NEW_TEMP_HIERARCHY.Where(x => x.X_BM_EMP_CD == SAPCODE).FirstOrDefault();
                    if (bm != null)
                    {
                        ViewBag.username = bm.X_BM_NM;
                        ViewBag.zone = bm.X_BRANCH;
                        ViewBag.ZM_NAME = bm.X_ZONE;
                        ViewBag.ZM_CODE = bm.X_ZM_EMP_CD;
                        ViewBag.RM_NAME = bm.X_REGION;
                        ViewBag.RM_CODE = bm.X_RM_EMP_EMP_CD;
                        ViewBag.BM_NAME = bm.X_SALES_UNIT_NM;
                        ViewBag.BM_CODE = bm.X_BM_EMP_CD;
                        ViewBag.ARDM_NAME = bm.X_SM_NM;
                        ViewBag.ARDM_CODE = bm.X_SM_EMP_CD;
                        ViewBag.AGENT_CODE = bm.AGENT_CODE;
                        ViewBag.AGENT_NAME = bm.AGENT_NAME;
                    }
                    var listARDM = (from a in db.NEW_TEMP_HIERARCHY
                                    where a.X_BM_EMP_CD == SAPCODE && a.X_SM_STATUS == "IF" && a.X_SM_EMP_CD != "77777777"
                                    select new
                                    {
                                        a.X_SM_EMP_CD,
                                        a.X_SALES_UNIT_CD,
                                        a.X_SM_NM
                                    }
                                  ).ToList().Distinct();
                    if (listARDM != null)
                    {

                        foreach (var item in listARDM)
                        {
                            downline.Add(new SelectListItem { Text = item.X_SM_NM, Value = item.X_SM_EMP_CD });
                        }

                        ViewBag.lstDownline = downline;
                    }

                    var lstARDManager = db.NEW_TEMP_HIERARCHY.Where(x => x.X_BM_EMP_CD == SAPCODE).Select(x => x.X_SM_EMP_CD).ToList().Distinct();

                    selfshare = db.ENGAGE_SHARECOUNT.Where(x => x.SHC_SAPCODE == SAPCODE).Count();
                    selflead = db.Leads.Where(x => x.leads_sapcode == SAPCODE).Count();
                    teamshare = Convert.ToDecimal((from a in db.ENGAGE_SHARECOUNT
                                                   where lstARDManager.Contains(a.SHC_SAPCODE)
                                                   select a.SHC_SHARECOUNT).Count());


                    teamslead = Convert.ToDecimal((from a in db.Leads
                                                   where lstARDManager.Contains(a.leads_sapcode)
                                                   select a.leads_id).Count());
                    break;

                case "ARDM":

                    ViewBag.NextType = "AGENT";
                    ViewBag.PrevType = "ARDM";
                    var ardm = db.NEW_TEMP_HIERARCHY.Where(x => x.X_SM_EMP_CD == SAPCODE).FirstOrDefault();
                    if (ardm != null)
                    {
                        ViewBag.username = ardm.X_BM_NM;
                        ViewBag.zone = ardm.X_BRANCH;
                        ViewBag.ZM_NAME = ardm.X_ZONE;
                        ViewBag.ZM_CODE = ardm.X_ZM_EMP_CD;
                        ViewBag.RM_NAME = ardm.X_REGION;
                        ViewBag.RM_CODE = ardm.X_RM_EMP_EMP_CD;
                        ViewBag.BM_NAME = ardm.X_SALES_UNIT_NM;
                        ViewBag.BM_CODE = ardm.X_BM_EMP_CD;
                        ViewBag.ARDM_NAME = ardm.X_SM_NM;
                        ViewBag.ARDM_CODE = ardm.X_SM_EMP_CD;
                        ViewBag.AGENT_CODE = ardm.AGENT_CODE;
                        ViewBag.AGENT_NAME = ardm.AGENT_NAME;
                    }
                    var listAgent = (from a in db.NEW_TEMP_HIERARCHY
                                     where a.X_SM_EMP_CD == SAPCODE && a.X_SM_STATUS == "IF" && a.AGENT_CODE != "77777777"
                                     select new
                                     {
                                         a.X_SM_EMP_CD,
                                         a.AGENT_CODE,
                                         a.AGENT_NAME
                                     }
                                  ).ToList().Distinct();
                    if (listAgent != null)
                    {

                        foreach (var item in listAgent)
                        {
                            downline.Add(new SelectListItem { Text = item.AGENT_NAME, Value = item.AGENT_CODE });
                        }

                        ViewBag.lstDownline = downline;
                    }

                    var lstAgentManager = db.NEW_TEMP_HIERARCHY.Where(x => x.X_BM_EMP_CD == SAPCODE).Select(x => x.X_SM_EMP_CD).ToList().Distinct();

                    selfshare = db.ENGAGE_SHARECOUNT.Where(x => x.SHC_SAPCODE == SAPCODE).Count();
                    selflead = db.Leads.Where(x => x.leads_sapcode == SAPCODE).Count();
                    teamshare = Convert.ToDecimal((from a in db.ENGAGE_SHARECOUNT
                                                   where lstAgentManager.Contains(a.SHC_SAPCODE)
                                                   select a.SHC_SHARECOUNT).Count());


                    teamslead = Convert.ToDecimal((from a in db.Leads
                                                   where lstAgentManager.Contains(a.leads_sapcode)
                                                   select a.leads_id).Count());
                    break;



                case "AGENT":
                    var agent = db.NEW_TEMP_HIERARCHY.Where(x => x.AGENT_CODE == SAPCODE).FirstOrDefault();
                    if (agent != null)
                    {
                        ViewBag.username = agent.X_SM_NM;
                        ViewBag.ZM_NAME = agent.X_ZONE;
                        ViewBag.ZM_CODE = agent.X_ZM_EMP_CD;
                        ViewBag.RM_NAME = agent.X_REGION;
                        ViewBag.RM_CODE = agent.X_RM_EMP_EMP_CD;
                        ViewBag.BM_NAME = agent.X_SALES_UNIT_NM;
                        ViewBag.BM_CODE = agent.X_BM_EMP_CD;
                        ViewBag.ARDM_NAME = agent.X_SM_NM;
                        ViewBag.ARDM_CODE = agent.X_SM_EMP_CD;
                        ViewBag.AGENT_CODE = agent.AGENT_CODE;
                        ViewBag.AGENT_NAME = agent.AGENT_NAME;
                    }

                    selfshare = db.ENGAGE_SHARECOUNT.Where(x => x.SHC_SAPCODE == SAPCODE).Count();
                    selflead = db.Leads.Where(x => x.leads_sapcode == SAPCODE).Count();
                    break;

            }


            ViewBag.selfshare = selfshare;
            ViewBag.selflead = selflead;
            ViewBag.teamshare = teamshare;
            ViewBag.teamslead = teamslead;
        }


        //====================================================================

        //        public ActionResult UserDashboard(string REF_Key)
        //        {
        //            // ===============================
        //            // 1️⃣ Decode REF_Key → SAPCODE
        //            // ===============================
        //            Session["REF_KEY"] = REF_Key;
        //            string SAPCODE = string.Empty;

        //            var valueBytes = Convert.FromBase64String(REF_Key);
        //            string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

        //            foreach (var item in str_REFKEY.Split('&'))
        //            {
        //                if (item.Contains("SAPCODE"))
        //                {
        //                    SAPCODE = item.Split('=')[1];
        //                }
        //            }

        //            Session["SAPCODE"] = SAPCODE;

        //            // ===============================
        //            // 2️⃣ Get user from hierarchy
        //            // ===============================
        //            var userData = db.NEW_TEMP_HIERARCHY.FirstOrDefault(x =>
        //                x.X_ZM_EMP_CD == SAPCODE ||
        //                x.X_RM_EMP_EMP_CD == SAPCODE ||
        //                x.X_BM_EMP_CD == SAPCODE ||
        //                x.X_SM_EMP_CD == SAPCODE ||
        //                x.AGENT_CODE == SAPCODE
        //            );

        //            if (userData != null)
        //            {
        //                if (userData.X_ZM_EMP_CD == SAPCODE)
        //                {
        //                    Session["TYPE"] = "ZM";
        //                    if (Session["LOGIN"] == null)
        //                        Session["LOGIN"] = "ZM";
        //                }
        //                else if (userData.X_RM_EMP_EMP_CD == SAPCODE)
        //                {
        //                    Session["TYPE"] = "RM";
        //                    if (Session["LOGIN"] == null)
        //                        Session["LOGIN"] = "RM";
        //                }
        //                else if (userData.X_BM_EMP_CD == SAPCODE)
        //                {
        //                    Session["TYPE"] = "BM";
        //                    if (Session["LOGIN"] == null)
        //                        Session["LOGIN"] = "BM";
        //                }
        //                else if (userData.X_SM_EMP_CD == SAPCODE)
        //                {
        //                    Session["TYPE"] = "ARDM";
        //                    if (Session["LOGIN"] == null)
        //                        Session["LOGIN"] = "ARDM";
        //                }
        //                else if (userData.AGENT_CODE == SAPCODE)
        //                {
        //                    Session["TYPE"] = "AGENT";
        //                    if (Session["LOGIN"] == null)
        //                        Session["LOGIN"] = "AGENT";
        //                }

        //                // 🔥 Set user's channel
        //                Session["X_CHANNEL"] = userData.X_CHANNEL;
        //            }
        //            else
        //            {
        //                Session.Clear();
        //            }

        //            // ===============================
        //            // 🔔 3️⃣ Notifications (Channel + SAPCODE)
        //            // ===============================
        //            string userChannel = Convert.ToString(Session["X_CHANNEL"]);

        //            var notifications = db.UserNotifications
        //                .Where(n =>
        //                    n.SAPCode == SAPCODE &&
        //                    n.Channel_code == userChannel &&
        //                    n.IsRead == false)
        //                .OrderByDescending(n => n.CreatedDate)
        //                .ToList();

        //            ViewBag.Notifications = notifications;
        //            ViewBag.NotificationCount = notifications.Count;

        //            // ===============================
        //            // 4️⃣ Existing Dashboard Data
        //            // ===============================
        //            SetData();

        //            // Share leaderboard
        //            ViewBag.ShareLeaderBoard = db.ENGAGE_SHARECOUNT.SqlQuery(@"
        //        SELECT TOP 10 * FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) SHC_ID,
        //                CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) SHC_SHARECOUNT,
        //                SHC_SAPCODE,
        //                '' SHC_PLATEFORM,
        //                CAST('2022-01-01' AS DATE) SHC_DATE,
        //                CREATIVE_ID
        //            FROM ENGAGE_SHARECOUNT
        //            GROUP BY SHC_SAPCODE, CREATIVE_ID
        //        ) A").ToList<ENGAGE_SHARECOUNT>();

        //            ViewBag.ShareLeaderBoardMyRank = db.ENGAGE_SHARECOUNT.SqlQuery(@"
        //        SELECT * FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) SHC_ID,
        //                CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) SHC_SHARECOUNT,
        //                SHC_SAPCODE,
        //                '' SHC_PLATEFORM,
        //                CAST('2022-01-01' AS DATE) SHC_DATE,
        //                CREATIVE_ID
        //            FROM ENGAGE_SHARECOUNT
        //            GROUP BY SHC_SAPCODE, CREATIVE_ID
        //        ) A WHERE A.SHC_SAPCODE = @SAPCODE",
        //                new SqlParameter("@SAPCODE", SAPCODE))
        //                .FirstOrDefault<ENGAGE_SHARECOUNT>();

        //            // Leads leaderboard
        //            ViewBag.LeadLeaderBoardMyRank = db.Leads.SqlQuery(@"
        //    SELECT * FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS leads_id,
        //            CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS leads_sapcode,
        //            '' AS LEADS_NAME,
        //            CAST(0 AS DECIMAL(18,2)) AS LEADS_MOBILE,
        //            CAST(0 AS DECIMAL(18,2)) AS LEADS_CREATIVEID,
        //            CAST(NULL AS DATETIME) AS LEADS_DATE,
        //            '' AS LEADS_PLATEFORM,
        //            leads_email,
        //            api_leads_id
        //        FROM leads
        //        GROUP BY leads_SAPCODE, leads_email, api_leads_id
        //    ) A
        //    WHERE A.leads_SAPCODE = @SAPCODE
        //", new SqlParameter("@SAPCODE", SAPCODE))
        //  .FirstOrDefault<Lead>();


        //            ViewBag.LeadLeaderBoardMyRank = db.Leads.SqlQuery(@"
        //        SELECT * FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) leads_id,
        //                CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) leads_sapcode,
        //                '' LEADS_NAME,
        //                0 LEADS_MOBILE,
        //                0 LEADS_CREATIVEID,
        //                '' LEADS_DATE,
        //                '' LEADS_PLATEFORM,
        //                leads_email,
        //                api_leads_id
        //            FROM leads
        //            GROUP BY leads_SAPCODE, leads_email, api_leads_id
        //        ) A WHERE A.leads_SAPCODE = @SAPCODE",
        //                new SqlParameter("@SAPCODE", SAPCODE))
        //                .FirstOrDefault<Lead>();

        //            return View();
        //        }





        //====================================
        public ActionResult UserDashboard(string REF_Key)
        {
            try
            {
                // 🔹 Always set REF_KEY first
                if (!string.IsNullOrEmpty(REF_Key))
                {
                    Session["REF_KEY"] = REF_Key;
                }

                string SAPCODE = string.Empty;

                if (string.IsNullOrEmpty(REF_Key))
                {
                    ViewBag.Error = "Invalid Reference Key";
                    return View();
                }

                // 🔹 Decode Base64 REF_Key
                var valueBytes = Convert.FromBase64String(REF_Key);
                string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

                // 🔹 Extract SAPCODE
                string[] separate_params = str_REFKEY.Split('&');
                foreach (var item in separate_params)
                {
                    if (item.Contains("SAPCODE"))
                    {
                        SAPCODE = item.Split('=')[1];
                    }
                }

                // 🔹 Set SAPCODE Session
                Session["SAPCODE"] = SAPCODE;
                Session["REF_KEY"] = REF_Key;

                // 🔹 Determine user type and name
                string userType = string.Empty;
                string userName = string.Empty;

                var userData = db.NEW_TEMP_HIERARCHY
                    .Where(x => x.X_ZM_EMP_CD == SAPCODE
                             || x.X_RM_EMP_EMP_CD == SAPCODE
                             || x.X_BM_EMP_CD == SAPCODE
                             || x.X_SM_EMP_CD == SAPCODE
                             || x.AGENT_CODE == SAPCODE)
                    .FirstOrDefault();

                if (userData != null)
                {
                    if (userData.X_ZM_EMP_CD == SAPCODE)
                    {
                        userType = "ZM";
                        userName = userData.X_ZM_NM;
                    }
                    else if (userData.X_RM_EMP_EMP_CD == SAPCODE)
                    {
                        userType = "RM";
                        userName = userData.X_RM_NM;
                    }
                    else if (userData.X_BM_EMP_CD == SAPCODE)
                    {
                        userType = "BM";
                        userName = userData.X_BM_NM;
                    }
                    else if (userData.X_SM_EMP_CD == SAPCODE)
                    {
                        userType = "ARDM";
                        userName = userData.X_SM_NM;
                    }
                    else if (userData.AGENT_CODE == SAPCODE)
                    {
                        userType = "AGENT";
                        userName = userData.AGENT_NAME;
                    }
                }

                // 🔹 Set Sessions
                Session["TYPE"] = userType;
                Session["USERNAME"] = userName;
                Session["X_CHANNEL"] = userData?.X_CHANNEL;
                Session["REF_KEY"] = REF_Key;

                if (Session["LOGIN"] == null)
                    Session["LOGIN"] = userType;

                // 🔹 Set Dashboard Data
                SetData();

                // 🔹 Share Leaderboard
                var qry = db.ENGAGE_SHARECOUNT.SqlQuery(@"
            SELECT TOP 10 *
            FROM (
                SELECT 
                    CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS 'SHC_ID',
                    CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS 'SHC_SHARECOUNT',
                    SHC_SAPCODE AS 'SHC_SAPCODE',
                    '' AS 'SHC_PLATEFORM',
                    CAST('2022-01-01' AS DATE) AS 'SHC_DATE',
                    CREATIVE_ID
                FROM ENGAGE_SHARECOUNT
                GROUP BY SHC_SAPCODE, CREATIVE_ID
            ) A
        ").ToList<ENGAGE_SHARECOUNT>();

                var qry2 = db.ENGAGE_SHARECOUNT.SqlQuery(@"
            SELECT *
            FROM (
                SELECT 
                    CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS 'SHC_ID',
                    CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS 'SHC_SHARECOUNT',
                    SHC_SAPCODE AS 'SHC_SAPCODE',
                    '' AS 'SHC_PLATEFORM',
                    CAST('2022-01-01' AS DATE) AS 'SHC_DATE',
                    CREATIVE_ID
                FROM ENGAGE_SHARECOUNT
                GROUP BY SHC_SAPCODE, CREATIVE_ID
            ) A
            WHERE A.SHC_SAPCODE = @SAPCODE",
                    new SqlParameter("@SAPCODE", SAPCODE)).FirstOrDefault<ENGAGE_SHARECOUNT>();

                ViewBag.ShareLeaderBoard = qry;
                ViewBag.ShareLeaderBoardMyRank = qry2;

                // 🔹 Leads Leaderboard
                var qry3 = db.Leads.SqlQuery(@"
            SELECT TOP 10 *
            FROM (
                SELECT 
                    CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS 'leads_id',
                    CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS 'leads_sapcode',
                    '' AS LEADS_NAME,
                    CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE,
                    CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID,
                    CAST('' AS DATETIME) AS LEADS_DATE,
                    '' AS LEADS_PLATEFORM,
                    leads_email,
                    api_leads_id,
                  api_response_json,
                  LeadType
                FROM leads
                GROUP BY leads_SAPCODE, leads_email, api_leads_id,api_response_json, LeadType
            ) A
        ").ToList<Lead>();

                var qry4 = db.Leads.SqlQuery(@"
            SELECT *
            FROM (
                SELECT 
                    CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS 'leads_id',
                    CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS 'leads_sapcode',
                    '' AS LEADS_NAME,
                    CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE,
                    CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID,
                    CAST('' AS DATETIME) AS LEADS_DATE,
                    '' AS LEADS_PLATEFORM,
                    leads_email,
                    api_leads_id,
                  api_response_json,
                  LeadType
                FROM leads
                GROUP BY leads_SAPCODE, leads_email, api_leads_id,api_response_json, LeadType
            ) A
            WHERE A.leads_SAPCODE = @SAPCODE",
                    new SqlParameter("@SAPCODE", SAPCODE)).FirstOrDefault<Lead>();

                ViewBag.LeadLeaderBoard = qry3;
                ViewBag.LeadLeaderBoardMyRank = qry4;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Something went wrong: " + ex.Message;

                // ❌ Don't remove REF_KEY
                Session.Remove("SAPCODE");
                Session.Remove("LOGIN");
                Session.Remove("TYPE");

                return View();
            }
        }


        //public ActionResult UserDashboard(string REF_Key)
        //{
        //    try
        //    {


        //        Session["REF_KEY"] = REF_Key;
        //        string SAPCODE = string.Empty;

        //        // Decode Base64 REF_Key
        //        var valueBytes = Convert.FromBase64String(REF_Key);
        //        string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

        //        // Extract SAPCODE
        //        string[] separate_params = str_REFKEY.Split('&');
        //        foreach (var item in separate_params)
        //        {
        //            if (item.Contains("SAPCODE"))
        //            {
        //                SAPCODE = item.Split('=')[1];
        //            }
        //        }

        //        Session["SAPCODE"] = SAPCODE;

        //        // 🔹 Determine user type and name from NEW_TEMP_HIERARCHY
        //        string userType = string.Empty;
        //        string userName = string.Empty;

        //        var userData = db.NEW_TEMP_HIERARCHY
        //            .Where(x => x.X_ZM_EMP_CD == SAPCODE
        //                     || x.X_RM_EMP_EMP_CD == SAPCODE
        //                     || x.X_BM_EMP_CD == SAPCODE
        //                     || x.X_SM_EMP_CD == SAPCODE
        //                     || x.AGENT_CODE == SAPCODE)
        //            .FirstOrDefault();

        //        if (userData != null)
        //        {
        //            if (userData.X_ZM_EMP_CD == SAPCODE)
        //            {
        //                userType = "ZM";
        //                userName = userData.X_ZM_NM;
        //            }
        //            else if (userData.X_RM_EMP_EMP_CD == SAPCODE)
        //            {
        //                userType = "RM";
        //                userName = userData.X_RM_NM;
        //            }
        //            else if (userData.X_BM_EMP_CD == SAPCODE)
        //            {
        //                userType = "BM";
        //                userName = userData.X_BM_NM;
        //            }
        //            else if (userData.X_SM_EMP_CD == SAPCODE)
        //            {
        //                userType = "ARDM";
        //                userName = userData.X_SM_NM;
        //            }
        //            else if (userData.AGENT_CODE == SAPCODE)
        //            {
        //                userType = "AGENT";
        //                userName = userData.AGENT_NAME;
        //            }
        //        }

        //        Session["TYPE"] = userType;
        //        Session["USERNAME"] = userName;
        //        if (Session["LOGIN"] == null) Session["LOGIN"] = userType;

        //        // 🔹 Get user's X_CHANNEL
        //        var userChannel = userData?.X_CHANNEL;
        //        Session["X_CHANNEL"] = userChannel;

        //        // Set additional dashboard data (if any)
        //        SetData();

        //        // 🔹 Share Leaderboard
        //        var qry = db.ENGAGE_SHARECOUNT.SqlQuery(@"
        //    SELECT TOP 10 *
        //    FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS 'SHC_ID',
        //            CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS 'SHC_SHARECOUNT',
        //            SHC_SAPCODE AS 'SHC_SAPCODE',
        //            '' AS 'SHC_PLATEFORM',
        //            CAST('2022-01-01' AS DATE) AS 'SHC_DATE',
        //            CREATIVE_ID
        //        FROM ENGAGE_SHARECOUNT
        //        GROUP BY SHC_SAPCODE, CREATIVE_ID
        //    ) A
        //").ToList<ENGAGE_SHARECOUNT>();

        //        var qry2 = db.ENGAGE_SHARECOUNT.SqlQuery(@"
        //    SELECT *
        //    FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS 'SHC_ID',
        //            CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS 'SHC_SHARECOUNT',
        //            SHC_SAPCODE AS 'SHC_SAPCODE',
        //            '' AS 'SHC_PLATEFORM',
        //            CAST('2022-01-01' AS DATE) AS 'SHC_DATE',
        //            CREATIVE_ID
        //        FROM ENGAGE_SHARECOUNT
        //        GROUP BY SHC_SAPCODE, CREATIVE_ID
        //    ) A
        //    WHERE A.SHC_SAPCODE = @SAPCODE",
        //            new SqlParameter("@SAPCODE", SAPCODE)).FirstOrDefault<ENGAGE_SHARECOUNT>();

        //        ViewBag.ShareLeaderBoard = qry;
        //        ViewBag.ShareLeaderBoardMyRank = qry2;

        //        // 🔹 Leads Leaderboard
        //        var qry3 = db.Leads.SqlQuery(@"
        //    SELECT TOP 10 *
        //    FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS 'leads_id',
        //            CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS 'leads_sapcode',
        //            '' AS LEADS_NAME,
        //            CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE,
        //            CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID,
        //            CAST('' AS DATETIME) AS LEADS_DATE,
        //            '' AS LEADS_PLATEFORM,
        //            leads_email,
        //            api_leads_id
        //        FROM leads
        //        GROUP BY leads_SAPCODE, leads_email, api_leads_id
        //    ) A
        //").ToList<Lead>();

        //        var qry4 = db.Leads.SqlQuery(@"
        //    SELECT *
        //    FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS 'leads_id',
        //            CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS 'leads_sapcode',
        //            '' AS LEADS_NAME,
        //            CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE,
        //            CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID,
        //            CAST('' AS DATETIME) AS LEADS_DATE,
        //            '' AS LEADS_PLATEFORM,
        //            leads_email,
        //            api_leads_id
        //        FROM leads
        //        GROUP BY leads_SAPCODE, leads_email, api_leads_id
        //    ) A
        //    WHERE A.leads_SAPCODE = @SAPCODE",
        //            new SqlParameter("@SAPCODE", SAPCODE)).FirstOrDefault<Lead>();

        //        ViewBag.LeadLeaderBoard = qry3;
        //        ViewBag.LeadLeaderBoardMyRank = qry4;

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Something went wrong: " + ex.Message;
        //        Session.Remove("SAPCODE");
        //        Session.Remove("LOGIN");
        //        Session.Remove("TYPE");
        //        return View();
        //    }
        //}


        //===================================================


        //public ActionResult UserDashboard(string REF_Key)
        //{
        //    try
        //    {
        //        Session["REF_KEY"] = REF_Key;
        //        string SAPCODE = string.Empty;

        //        // Decode Base64
        //        var valueBytes = Convert.FromBase64String(REF_Key);
        //        string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

        //        // Split parameters
        //        string[] separate_params = str_REFKEY.Split('&');
        //        foreach (var item in separate_params)
        //        {
        //            if (item.Contains("SAPCODE"))
        //            {
        //                SAPCODE = item.Split('=')[1];
        //            }
        //        }

        //        Session["SAPCODE"] = SAPCODE;

        //        // Determine user type
        //        if (db.NEW_TEMP_HIERARCHY.Any(x => x.X_ZM_EMP_CD == SAPCODE))
        //        {
        //            Session["TYPE"] = "ZM";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "ZM";
        //        }
        //        else if (db.NEW_TEMP_HIERARCHY.Any(x => x.X_RM_EMP_EMP_CD == SAPCODE))
        //        {
        //            Session["TYPE"] = "RM";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "RM";
        //        }
        //        else if (db.NEW_TEMP_HIERARCHY.Any(x => x.X_BM_EMP_CD == SAPCODE))
        //        {
        //            Session["TYPE"] = "BM";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "BM";
        //        }
        //        else if (db.NEW_TEMP_HIERARCHY.Any(x => x.X_SM_EMP_CD == SAPCODE))
        //        {
        //            Session["TYPE"] = "ARDM";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "ARDM";
        //        }
        //        else if (db.NEW_TEMP_HIERARCHY.Any(x => x.AGENT_CODE == SAPCODE))
        //        {
        //            Session["TYPE"] = "AGENT";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "AGENT";
        //        }
        //        else
        //        {
        //            Session.Remove("SAPCODE");
        //            Session.Remove("LOGIN");
        //            Session.Remove("TYPE");
        //        }

        //        // 🔹 Get user's X_CHANNEL
        //        var userChannel = db.NEW_TEMP_HIERARCHY
        //            .Where(x => x.X_ZM_EMP_CD == SAPCODE || x.X_RM_EMP_EMP_CD == SAPCODE
        //                     || x.X_BM_EMP_CD == SAPCODE || x.X_SM_EMP_CD == SAPCODE
        //                     || x.AGENT_CODE == SAPCODE)
        //            .Select(x => x.X_CHANNEL)
        //            .FirstOrDefault();

        //        Session["X_CHANNEL"] = userChannel;

        //        // Set additional data
        //        SetData();

        //        // Share Leaderboard
        //        var qry = db.ENGAGE_SHARECOUNT
        //            .SqlQuery(@"
        //        SELECT TOP 10 *
        //        FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS 'SHC_ID', 
        //                CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS 'SHC_SHARECOUNT', 
        //                SHC_SAPCODE AS 'SHC_SAPCODE', 
        //                '' AS 'SHC_PLATEFORM',  
        //                CAST('2022-01-01' AS DATE) AS 'SHC_DATE',  
        //                CREATIVE_ID 
        //            FROM ENGAGE_SHARECOUNT 
        //            GROUP BY SHC_SAPCODE, CREATIVE_ID
        //        ) A")
        //            .ToList<ENGAGE_SHARECOUNT>();

        //        var qry2 = db.ENGAGE_SHARECOUNT
        //            .SqlQuery(@"
        //        SELECT * 
        //        FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS 'SHC_ID', 
        //                CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS 'SHC_SHARECOUNT', 
        //                SHC_SAPCODE AS 'SHC_SAPCODE', 
        //                '' AS 'SHC_PLATEFORM',  
        //                CAST('2022-01-01' AS DATE) AS 'SHC_DATE',  
        //                CREATIVE_ID 
        //            FROM ENGAGE_SHARECOUNT 
        //            GROUP BY SHC_SAPCODE, CREATIVE_ID
        //        ) A 
        //        WHERE A.SHC_SAPCODE = @SAPCODE", new SqlParameter("@SAPCODE", Session["SAPCODE"]))
        //            .FirstOrDefault<ENGAGE_SHARECOUNT>();

        //        ViewBag.ShareLeaderBoard = qry;
        //        ViewBag.ShareLeaderBoardMyRank = qry2;

        //        // Lead Leaderboard
        //        var qry3 = db.Leads
        //            .SqlQuery(@"
        //        SELECT TOP 10 *
        //        FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS 'leads_id', 
        //                CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS 'leads_sapcode', 
        //                '' AS LEADS_NAME, 
        //                CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE, 
        //                CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID, 
        //                CAST('' AS DATETIME) AS LEADS_DATE, 
        //                '' AS LEADS_PLATEFORM,
        //                leads_email, 
        //                api_leads_id
        //            FROM leads 
        //            GROUP BY leads_SAPCODE, leads_email, api_leads_id
        //        ) A")
        //            .ToList<Lead>();

        //        var qry4 = db.Leads
        //            .SqlQuery(@"
        //        SELECT * 
        //        FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS 'leads_id', 
        //                CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS 'leads_sapcode', 
        //                '' AS LEADS_NAME, 
        //                CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE, 
        //                CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID, 
        //                CAST('' AS DATETIME) AS LEADS_DATE, 
        //                '' AS LEADS_PLATEFORM, 
        //                leads_email, 
        //                api_leads_id
        //            FROM leads 
        //            GROUP BY leads_SAPCODE, leads_email, api_leads_id
        //        ) A 
        //        WHERE A.leads_SAPCODE = @SAPCODE", new SqlParameter("@SAPCODE", Session["SAPCODE"]))
        //            .FirstOrDefault<Lead>();

        //        ViewBag.LeadLeaderBoard = qry3;
        //        ViewBag.LeadLeaderBoardMyRank = qry4;

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Something went wrong: " + ex.Message;
        //        Session.Remove("SAPCODE");
        //        Session.Remove("LOGIN");
        //        Session.Remove("TYPE");
        //        return View();
        //    }

        //}

        //=====================================================================
        public JsonResult GetLeadsCounts(string period)
        {
            try
            {
                DateTime today = DateTime.Today;
                DateTime startDate, endDate;

                // 🔹 Determine Financial Year start
                DateTime fyStart = (today.Month >= 4)
                    ? new DateTime(today.Year, 4, 1)     // April to Dec → FY starts April 1 of this year
                    : new DateTime(today.Year - 1, 4, 1); // Jan to Mar → FY started April 1 last year

                endDate = today;

                // 🔹 Determine date range based on period
                switch (period)
                {
                    case "Yesterday":
                        startDate = endDate = today.AddDays(-1);
                        break;

                    case "MTD": // Last 1 month
                        startDate = endDate.AddMonths(-1).AddDays(1);
                        if (startDate < fyStart) startDate = fyStart; // limit to FY start
                        break;

                    case "QTD": // Last 3 months
                        startDate = endDate.AddMonths(-3).AddDays(1);
                        if (startDate < fyStart) startDate = fyStart;
                        break;

                    case "YTD": // FY to date
                        startDate = fyStart;
                        break;

                    default:
                        startDate = endDate = today;
                        break;
                }

                // 🔹 Get SAP code from session

                //string sapCode = Session["SAPCODE"]?.ToString();
                //if (string.IsNullOrEmpty(sapCode))
                //    return Json(new { error = "SAPCODE not found in session" }, JsonRequestBehavior.AllowGet);
                string sapCode = Session["SAPCODE"]?.ToString();

                if (string.IsNullOrEmpty(sapCode))
                {
                    string refKey = Session["REF_KEY"]?.ToString();

                    if (!string.IsNullOrEmpty(refKey))
                    {
                        var valueBytes = Convert.FromBase64String(refKey);
                        string decoded = System.Text.Encoding.UTF8.GetString(valueBytes);

                        var parts = decoded.Split('&');

                        foreach (var item in parts)
                        {
                            if (item.StartsWith("SAPCODE="))
                            {
                                sapCode = item.Replace("SAPCODE=", "");
                                Session["SAPCODE"] = sapCode;  // session me store
                                break;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(sapCode))
                {
                    return Json(new { error = "SAPCODE not found" }, JsonRequestBehavior.AllowGet);
                }

                // 🔹 Platforms
                var platforms = new[] { "WHATSAPP", "FACEBOOK", "INSTAGRAM", "TWITTER", "LINKEDIN" };

                // 🔹 Query ENGAGE_SHARECOUNT table
                var data = db.ENGAGE_SHARECOUNT
                    .Where(l => l.SHC_SAPCODE == sapCode &&
                                platforms.Contains(l.SHC_PLATEFORM.ToUpper()) &&
                                DbFunctions.TruncateTime(l.SHC_DATE) >= startDate &&
                                DbFunctions.TruncateTime(l.SHC_DATE) <= endDate)
                    .GroupBy(l => l.SHC_PLATEFORM.ToUpper())
                    .Select(g => new
                    {
                        Platform = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                // 🔹 Fill missing platforms with 0
                var result = platforms.ToDictionary(
                    p => p,
                    p => data.FirstOrDefault(x => x.Platform == p)?.Count ?? 0
                );

                // 🔹 Return JSON
                return Json(new
                {
                    WhatsApp = result["WHATSAPP"],
                    Facebook = result["FACEBOOK"],
                    Instagram = result["INSTAGRAM"],
                    Twitter = result["TWITTER"],
                    LinkedIn = result["LINKEDIN"]
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //public JsonResult GetLeadsCounts(string period)
        //{
        //    DateTime startDate, endDate;

        //    // 🔹 Determine date range based on selected period
        //    switch (period)
        //    {
        //        case "Yesterday":
        //            startDate = endDate = DateTime.Today.AddDays(-1);
        //            break;
        //        case "MTD":
        //            startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        //            endDate = DateTime.Today;
        //            break;
        //        case "QTD":
        //            var quarter = (DateTime.Today.Month - 1) / 3 + 1;
        //            startDate = new DateTime(DateTime.Today.Year, (quarter - 1) * 3 + 1, 1);
        //            endDate = DateTime.Today;
        //            break;
        //        case "YTD":
        //            startDate = new DateTime(DateTime.Today.Year, 4, 1); // Assuming FY starts April 1
        //            endDate = DateTime.Today;
        //            break;
        //        default:
        //            startDate = endDate = DateTime.Today;
        //            break;
        //    }

        //    // 🔹 Fetch data from campaign_master and count platforms
        //    var result = db.campaign_master
        //        .Where(c => c.camapaign_master_status == "Active" &&
        //                    c.campaign_master_start_date >= startDate &&
        //                    c.campaign_master_start_date <= endDate)
        //        .GroupBy(g => 1)
        //        .Select(g => new
        //        {
        //            WhatsApp = g.Sum(x => (x.campaign_master_whatsapp ?? false) ? 1L : 0L),
        //            Facebook = g.Sum(x => (x.campaign_master_facebook ?? false) ? 1L : 0L),
        //            Instagram = g.Sum(x => (x.campaign_master_instagram ?? false) ? 1L : 0L),
        //            Twitter = g.Sum(x => (x.campaign_master_twitter ?? false) ? 1L : 0L),
        //            LinkedIn = g.Sum(x => (x.campaign_master_linkedin ?? false) ? 1L : 0L) // ✅ Added LinkedIn
        //        })
        //        .FirstOrDefault();

        //    // 🔹 Return result as JSON (fallback to zero values)
        //    return Json(result ?? new
        //    {
        //        WhatsApp = 0L,
        //        Facebook = 0L,
        //        Instagram = 0L,
        //        Twitter = 0L,
        //        LinkedIn = 0L
        //    }, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetPlatformLeads(string period, int? year = null, int? month = null, int? quarter = null)
        {
            try
            {
                DateTime startDate, endDate;
                DateTime today = DateTime.Today;

                // ✅ Financial Year Start (April)
                DateTime fyStart = (year.HasValue && year > 0)
                    ? new DateTime(year.Value, 4, 1)
                    : (today.Month >= 4 ? new DateTime(today.Year, 4, 1) : new DateTime(today.Year - 1, 4, 1));

                DateTime fyEnd = fyStart.AddYears(1).AddDays(-1);

                // Default to FY Range
                startDate = fyStart;
                endDate = fyEnd;

                switch (period)
                {
                    case "Yesterday":
                        startDate = endDate = today.AddDays(-1);
                        break;

                    case "MTD":
                        if (year.HasValue && month.HasValue)
                        {
                            int selectedYear = (month >= 4) ? year.Value : year.Value + 1; // Adjust Jan–Mar
                            startDate = new DateTime(selectedYear, month.Value, 1);
                            endDate = startDate.AddMonths(1).AddDays(-1);
                            if (endDate > fyEnd) endDate = fyEnd;
                        }
                        else
                        {
                            startDate = new DateTime(today.Year, today.Month, 1);
                            endDate = today;
                        }
                        break;

                    case "QTD":
                        if (year.HasValue && quarter.HasValue)
                        {
                            int selectedYear = year.Value;
                            switch (quarter)
                            {
                                case 1:
                                    startDate = new DateTime(selectedYear, 4, 1);
                                    endDate = new DateTime(selectedYear, 6, 30);
                                    break;
                                case 2:
                                    startDate = new DateTime(selectedYear, 7, 1);
                                    endDate = new DateTime(selectedYear, 9, 30);
                                    break;
                                case 3:
                                    startDate = new DateTime(selectedYear, 10, 1);
                                    endDate = new DateTime(selectedYear, 12, 31);
                                    break;
                                case 4:
                                    startDate = new DateTime(selectedYear + 1, 1, 1);
                                    endDate = new DateTime(selectedYear + 1, 3, 31);
                                    break;
                            }
                        }
                        break;

                    case "YTD":
                        // ✅ Show FY data (April–March) based on dropdown-selected year
                        if (year.HasValue)
                        {
                            startDate = new DateTime(year.Value, 4, 1);
                            endDate = new DateTime(year.Value + 1, 3, 31);
                            if (endDate > today) endDate = today; // Stop at current date if FY still running
                        }
                        break;
                }

                var platforms = new[] { "WHATSAPP", "FACEBOOK", "INSTAGRAM", "TWITTER", "LINKEDIN" };
                var sapCode = Session["SAPCODE"]?.ToString();

                if (string.IsNullOrEmpty(sapCode))
                {
                    return Json(new { error = "No SAPCODE found in session" }, JsonRequestBehavior.AllowGet);
                }

                var data = db.Leads
                    .Where(l => l.leads_sapcode == sapCode
                                && platforms.Contains(l.leads_plateform.ToUpper())
                                && DbFunctions.TruncateTime(l.leads_date) >= startDate
                                && DbFunctions.TruncateTime(l.leads_date) <= endDate)
                    .GroupBy(l => l.leads_plateform.ToUpper())
                    .Select(g => new
                    {
                        Platform = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                var result = platforms.ToDictionary(
                    p => p,
                    p => data.FirstOrDefault(x => x.Platform == p)?.Count ?? 0
                );

                return Json(new
                {
                    WhatsApp = result["WHATSAPP"],
                    Facebook = result["FACEBOOK"],
                    Instagram = result["INSTAGRAM"],
                    Twitter = result["TWITTER"],
                    LinkedIn = result["LINKEDIN"],
                    StartDate = startDate.ToString("dd-MMM-yyyy"),
                    EndDate = endDate.ToString("dd-MMM-yyyy")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //======================================
        //    private string GetUserChannel()
        //    {
        //        string sapCode = Session["SAP_CODE"]?.ToString();

        //        if (string.IsNullOrWhiteSpace(sapCode))
        //            return string.Empty;

        //        sapCode = sapCode.Trim();

        //        var user = db.NEW_TEMP_HIERARCHY
        //                     .AsNoTracking()
        //                     .FirstOrDefault(x => x.X_BM_EMP_CD == sapCode);

        //        return user?.X_CHANNEL?.Trim().ToUpper() ?? string.Empty;
        //    }


        //    public ActionResult campaignview()
        //    {
        //        SetData();

        //        // dropdowns (same as your code)
        //        ViewBag.lstStatus = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Start", Value="Start"},
        //    new SelectListItem{Text="Pause", Value="Pause"}
        //};

        //        ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
        //        ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
        //        ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
        //        ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

        //        ViewBag.lstlanguage = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="English", Value="English"},
        //    new SelectListItem{Text="Hindi", Value="Hindi"},
        //    new SelectListItem{Text="Marathi", Value="Marathi"},
        //    new SelectListItem{Text="Tamil", Value="Tamil"}
        //};

        //        ViewBag.lsttype = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Creative", Value="Creative"},
        //    new SelectListItem{Text="Video", Value="Video"},
        //    new SelectListItem{Text="Blogs", Value="Blogs"}
        //};

        //        DateTime dt = DateTime.UtcNow.AddMinutes(330);

        //        // 🔹 MAIN FIX: SAP_CODE → X_CHANNEL
        //        string userChannel = GetUserChannel();

        //        var qry = db.campaign_master
        //                    .Where(x =>
        //                           x.channel_code.Trim().ToUpper() == userChannel &&
        //                           (
        //                             x.camapaign_master_status == "Pause" ||
        //                             (x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt) ||
        //                             x.campaign_master_start_date > dt
        //                           ))
        //                    .OrderByDescending(x => x.campaign_master_id)
        //                    .ToList();

        //        ViewBag.lstAllCampaigns = qry;

        //        return View();
        //    }
        //    [HttpPost]
        //    public ActionResult campaignview(FormCollection form)
        //    {
        //        SetData();

        //        var predicate = PredicateBuilder.True<campaign_master>();

        //        // form filters
        //        if (!string.IsNullOrEmpty(form["category"]))
        //            predicate = predicate.And(x => x.campaign_category_id == Convert.ToDecimal(form["category"]));

        //        if (!string.IsNullOrEmpty(form["campaign"]))
        //            predicate = predicate.And(x => x.campaign_id == Convert.ToDecimal(form["campaign"]));

        //        if (!string.IsNullOrEmpty(form["subcampaign"]))
        //            predicate = predicate.And(x => x.subcampaign_id == Convert.ToDecimal(form["subcampaign"]));

        //        if (!string.IsNullOrEmpty(form["language"]))
        //            predicate = predicate.And(x => x.campaign_master_lang == form["language"]);

        //        if (!string.IsNullOrEmpty(form["campaigntype"]))
        //            predicate = predicate.And(x => x.campaign_master_type == form["campaigntype"]);

        //        if (!string.IsNullOrEmpty(form["campaignstatus"]))
        //            predicate = predicate.And(x => x.camapaign_master_status == form["campaignstatus"]);

        //        DateTime dt = DateTime.UtcNow.AddMinutes(330);

        //        predicate = predicate.And(x =>
        //            x.camapaign_master_status == "Pause" ||
        //            (x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt) ||
        //            x.campaign_master_start_date > dt
        //        );

        //        // 🔹 MAIN FIX: SAP_CODE → X_CHANNEL
        //        string userChannel = GetUserChannel();

        //        if (!string.IsNullOrEmpty(userChannel))
        //            predicate = predicate.And(x => x.channel_code.Trim().ToUpper() == userChannel);

        //        ViewBag.lstAllCampaigns = db.campaign_master
        //                                    .Where(predicate)
        //                                    .OrderByDescending(x => x.campaign_master_id)
        //                                    .ToList();

        //        // reload dropdowns
        //        ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
        //        ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
        //        ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
        //        ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

        //        return View();
        //    }
        //==============================================================

        //    public ActionResult campaignview()
        //    {
        //        SetData();

        //        // Status
        //        ViewBag.lstStatus = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Start", Value="Start"},
        //    new SelectListItem{Text="Pause", Value="Pause"}
        //};

        //        ViewBag.lstchannels = db.channels
        //            .Where(x => x.channel_delflag == null)
        //            .ToList();

        //        ViewBag.lstcategory = db.campaign_category
        //            .Where(x => x.campaign_category_delflag == null)
        //            .ToList();

        //        ViewBag.lstcampaign = db.campaigns
        //            .Where(x => x.campaign_delflag == null)
        //            .ToList();

        //        ViewBag.lstsubcampaign = db.subcampaigns
        //            .Where(x => x.subcampaign_delflag == null)
        //            .ToList();

        //        ViewBag.lstlanguage = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="English", Value="English"},
        //    new SelectListItem{Text="Hindi", Value="Hindi"},
        //    new SelectListItem{Text="Marathi", Value="Marathi"},
        //    new SelectListItem{Text="Tamil", Value="Tamil"}
        //};

        //        ViewBag.lsttype = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Creative", Value="Creative"},
        //    new SelectListItem{Text="Video", Value="Video"},
        //    new SelectListItem{Text="Blogs", Value="Blogs"}
        //};

        //        DateTime dt = DateTime.UtcNow.AddMinutes(330);

        //        // 🔹 SAPCODE based X_CHANNEL
        //        string sapCode = (Session["SAPCODE"]?.ToString() ?? "").Trim();
        //        string userChannel = (Session["X_CHANNEL"]?.ToString() ?? "").Trim().ToUpper();

        //        // 🔴 Agar channel hi nahi mila → blank list
        //        if (string.IsNullOrEmpty(userChannel))
        //        {
        //            ViewBag.lstAllCampaigns = new List<campaign_master>();
        //            return View();
        //        }

        //        var qry = db.campaign_master
        //            .Where(x =>
        //                (
        //                    (x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt)
        //                    || x.campaign_master_start_date > dt
        //                )
        //                && x.channel_code != null
        //                && x.channel_code.Trim().ToUpper() == userChannel
        //            )
        //            .OrderByDescending(x => x.campaign_master_id)
        //            .ToList();

        //        ViewBag.lstAllCampaigns = qry;

        //        return View();
        //    }


        //    [HttpPost]
        //    public ActionResult campaignview(FormCollection form)
        //    {
        //        SetData();

        //        ViewBag.lstStatus = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Start", Value="Start"},
        //    new SelectListItem{Text="Pause", Value="Pause"}
        //};

        //        ViewBag.lstlanguage = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="English", Value="English"},
        //    new SelectListItem{Text="Hindi", Value="Hindi"},
        //    new SelectListItem{Text="Marathi", Value="Marathi"},
        //    new SelectListItem{Text="Tamil", Value="Tamil"}
        //};

        //        ViewBag.lsttype = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Creative", Value="Creative"},
        //    new SelectListItem{Text="Video", Value="Video"},
        //    new SelectListItem{Text="Blogs", Value="Blogs"}
        //};

        //        var predicate = PredicateBuilder.True<campaign_master>();

        //        if (!string.IsNullOrEmpty(form["channel"]))
        //            predicate = predicate.And(x => x.channel_id == Convert.ToDecimal(form["channel"]));

        //        if (!string.IsNullOrEmpty(form["category"]))
        //            predicate = predicate.And(x => x.campaign_category_id == Convert.ToDecimal(form["category"]));

        //        if (!string.IsNullOrEmpty(form["campaign"]))
        //            predicate = predicate.And(x => x.campaign_id == Convert.ToDecimal(form["campaign"]));

        //        if (!string.IsNullOrEmpty(form["subcampaign"]))
        //            predicate = predicate.And(x => x.subcampaign_id == Convert.ToDecimal(form["subcampaign"]));

        //        if (!string.IsNullOrEmpty(form["language"]))
        //            predicate = predicate.And(x => x.campaign_master_lang == form["language"]);

        //        if (!string.IsNullOrEmpty(form["campaigntype"]))
        //            predicate = predicate.And(x => x.campaign_master_type == form["campaigntype"]);

        //        if (!string.IsNullOrEmpty(form["campaignstatus"]))
        //            predicate = predicate.And(x => x.camapaign_master_status == form["campaignstatus"]);

        //        DateTime dt = DateTime.UtcNow.AddMinutes(330);
        //        predicate = predicate.And(x =>
        //            x.camapaign_master_status == "Pause"
        //            || (x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt)
        //            || x.campaign_master_start_date > dt
        //        );

        //        // 🔹 SAPCODE → X_CHANNEL security filter
        //        string userChannel = (Session["X_CHANNEL"]?.ToString() ?? "").Trim().ToUpper();

        //        if (!string.IsNullOrEmpty(userChannel))
        //        {
        //            predicate = predicate.And(x =>
        //                x.channel_code != null &&
        //                x.channel_code.Trim().ToUpper() == userChannel
        //            );
        //        }
        //        else
        //        {
        //            ViewBag.lstAllCampaigns = new List<campaign_master>();
        //            return View();
        //        }

        //        ViewBag.lstAllCampaigns = db.campaign_master
        //            .Where(predicate)
        //            .OrderByDescending(x => x.campaign_master_id)
        //            .ToList();

        //        return View();
        //    }
        //===========================================
        public ActionResult campaignview()
        {
            SetData();

            // Status
            List<SelectListItem> status = new List<SelectListItem>()
    {
        new SelectListItem{Text="Start", Value="Start"},
        new SelectListItem{Text="Pause", Value="Pause"}
    };
            ViewBag.lstStatus = status;

            // Channels
            ViewBag.lstchannels = db.channels
                                    .Where(x => x.channel_delflag == null)
                                    .ToList();

            // ✅ Categories (ONLY those having campaign_master data)
            var categories = db.campaign_category
                .Where(x => x.campaign_category_delflag == null
                    && db.campaign_master
                         .Any(c => c.campaign_category_id == x.campaign_category_id))
                .ToList();

            ViewBag.lstcategory = categories;

            // Campaigns
            ViewBag.lstcampaign = db.campaigns
                                    .Where(x => x.campaign_delflag == null)
                                    .ToList();

            ViewBag.lstsubcampaign = db.subcampaigns
                                       .Where(x => x.subcampaign_delflag == null)
                                       .ToList();

            // Languages
            ViewBag.lstlanguage = new List<SelectListItem>()
    {
        new SelectListItem{Text="English", Value="English"},
        new SelectListItem{Text="Hindi", Value="Hindi"},
        new SelectListItem{Text="Marathi", Value="Marathi"},
        new SelectListItem{Text="Tamil", Value="Tamil"}
    };

            // Types
            ViewBag.lsttype = new List<SelectListItem>()
    {
        new SelectListItem{Text="Creative", Value="Creative"},
        new SelectListItem{Text="Video", Value="Video"},
        new SelectListItem{Text="Blogs", Value="Blogs"}
    };

            ViewBag.channel = "";
            ViewBag.category = "";
            ViewBag.campaign = "";
            ViewBag.subcampaign = "";
            ViewBag.language = "";
            ViewBag.type = "";

            DateTime dt = System.DateTime.UtcNow.AddMinutes(330);

            string userChannel = (Session["X_CHANNEL"]?.ToString() ?? "")
                                 .Trim()
                                 .ToUpper();

            var qry1 = db.campaign_master
                         .Where(x => (
                                        (x.campaign_master_start_date <= dt
                                         && x.campaign_master_end_date >= dt)
                                        || x.campaign_master_start_date > dt
                                     )
                                     && x.channel_code.Trim().ToUpper() == userChannel)
                         .OrderByDescending(x => x.campaign_master_id)
                         .ToList();

            ViewBag.lstAllCampaigns = qry1;

            return View();
        }


        //public ActionResult campaignview()
        //{
        //    SetData();

        //    // Status
        //    List<SelectListItem> status = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Start", Value="Start"},
        //    new SelectListItem{Text="Pause", Value="Pause"}
        //};
        //    ViewBag.lstStatus = status;

        //    // Channels, Categories, etc.
        //    ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();

        //    var categories = db.campaign_category
        //                       .Where(x => x.campaign_category_delflag == null)
        //                       .ToList();

        //    foreach (var cat in categories)
        //    {
        //        bool hasData = db.campaign_master.Any(c => c.campaign_category_id == cat.campaign_category_id);
        //        cat.Campaign_Category_Status = hasData ? "0" : "1";
        //    }
        //    categories = categories
        //    .Where(x => x.Campaign_Category_Status == "0")
        //    .ToList();

        //    db.SaveChanges();
        //    ViewBag.lstcategory = categories.Where(x => x.Campaign_Category_Status == "0").ToList();

        //    ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
        //    ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

        //    // Languages & Types
        //    ViewBag.lstlanguage = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="English", Value="English"},
        //    new SelectListItem{Text="Hindi", Value="Hindi"},
        //    new SelectListItem{Text="Marathi", Value="Marathi"},
        //    new SelectListItem{Text="Tamil", Value="Tamil"}
        //};
        //    ViewBag.lsttype = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Creative", Value="Creative"},
        //    new SelectListItem{Text="Video", Value="Video"},
        //    new SelectListItem{Text="Blogs", Value="Blogs"}
        //};

        //    ViewBag.channel = "";
        //    ViewBag.category = "";
        //    ViewBag.campaign = "";
        //    ViewBag.subcampaign = "";
        //    ViewBag.language = "";
        //    ViewBag.type = "";

        //    DateTime dt = System.DateTime.UtcNow.AddMinutes(330);
        //    //string sapCode = (Session["SAPCODE"]?.ToString() ?? "").Trim();
        //    // 🔹 Filter by user's X_CHANNEL
        //    string userChannel = (Session["X_CHANNEL"]?.ToString() ?? "").Trim().ToUpper();

        //    var qry1 = db.campaign_master
        //                 .Where(x => ((x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt)
        //                               || x.campaign_master_start_date > dt)
        //                             && x.channel_code.Trim().ToUpper() == userChannel) // normalize here
        //                 .OrderByDescending(x => x.campaign_master_id)
        //                 .ToList();

        //    ViewBag.lstAllCampaigns = qry1;

        //    return View();
        //}



        [HttpPost]
        public ActionResult campaignview(FormCollection form)
        {
            SetData();

            // Same setup as GET (status, language, type, etc.)
            ViewBag.lstStatus = new List<SelectListItem>()
        {
            new SelectListItem{Text="Start", Value="Start"},
            new SelectListItem{Text="Pause", Value="Pause"}
        };

            ViewBag.lstlanguage = new List<SelectListItem>()
        {
            new SelectListItem{Text="English", Value="English"},
            new SelectListItem{Text="Hindi", Value="Hindi"},
            new SelectListItem{Text="Marathi", Value="Marathi"},
            new SelectListItem{Text="Tamil", Value="Tamil"}
        };

            ViewBag.lsttype = new List<SelectListItem>()
        {
            new SelectListItem{Text="Creative", Value="Creative"},
            new SelectListItem{Text="Video", Value="Video"},
            new SelectListItem{Text="Blogs", Value="Blogs"}
        };

            ViewBag.channel = form["channel"] ?? string.Empty;
            ViewBag.category = form["category"] ?? string.Empty;
            ViewBag.campaign = form["campaign"] ?? string.Empty;
            ViewBag.subcampaign = form["subcampaign"] ?? string.Empty;
            ViewBag.language = form["language"] ?? string.Empty;
            ViewBag.type = form["campaigntype"] ?? string.Empty;
            ViewBag.status = form["campaignstatus"] ?? string.Empty;

            var predicate = PredicateBuilder.True<campaign_master>();

            // Filters based on form
            if (!string.IsNullOrEmpty(form["channel"]))
                predicate = predicate.And(i => i.channel_id == Convert.ToDecimal(form["channel"]));
            if (!string.IsNullOrEmpty(form["category"]))
                predicate = predicate.And(i => i.campaign_category_id == Convert.ToDecimal(form["category"]));
            if (!string.IsNullOrEmpty(form["campaign"]))
                predicate = predicate.And(i => i.campaign_id == Convert.ToDecimal(form["campaign"]));
            if (!string.IsNullOrEmpty(form["subcampaign"]))
                predicate = predicate.And(i => i.subcampaign_id == Convert.ToDecimal(form["subcampaign"]));
            if (!string.IsNullOrEmpty(form["language"]))
                predicate = predicate.And(i => i.campaign_master_lang == form["language"]);
            if (!string.IsNullOrEmpty(form["campaigntype"]))
                predicate = predicate.And(i => i.campaign_master_type == form["campaigntype"]);
            if (!string.IsNullOrEmpty(form["campaignstatus"]))
                predicate = predicate.And(i => i.camapaign_master_status == form["campaignstatus"]);

            DateTime dt = System.DateTime.UtcNow.AddMinutes(330);
            predicate = predicate.And(i => i.camapaign_master_status == "Pause"
                                           || (i.campaign_master_start_date <= dt && i.campaign_master_end_date >= dt)
                                           || i.campaign_master_start_date > dt);

            // 🔹 Filter by user's X_CHANNEL
            string userChannel = (Session["X_CHANNEL"]?.ToString() ?? "").Trim().ToUpper();
            if (!string.IsNullOrEmpty(userChannel))
            {
                predicate = predicate.And(i => i.channel_code.Trim().ToUpper() == userChannel);
            }

            // Load dropdowns
            ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
            ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
            ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
            ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

            var qry1 = db.campaign_master.Where(predicate).OrderByDescending(x => x.campaign_master_id).ToList();
            ViewBag.lstAllCampaigns = qry1;

            return View();
        }







        //=====================================================
        //public ActionResult UserDashboard(string REF_Key)
        //{
        //    try
        //    {
        //        Session["REF_KEY"] = REF_Key;
        //        string SAPCODE = string.Empty;

        //        // Decode Base64
        //        var valueBytes = Convert.FromBase64String(REF_Key);
        //        string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

        //        // Split parameters
        //        string[] separate_params = str_REFKEY.Split('&');
        //        foreach (var item in separate_params)
        //        {
        //            if (item.Contains("SAPCODE"))
        //            {
        //                SAPCODE = item.Split('=')[1];
        //            }
        //        }

        //        Session["SAPCODE"] = SAPCODE;

        //        // Determine user type
        //        if (db.NEW_TEMP_HIERARCHY.Any(x => x.X_ZM_EMP_CD == SAPCODE))
        //        {
        //            Session["TYPE"] = "ZM";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "ZM";
        //        }
        //        else if (db.NEW_TEMP_HIERARCHY.Any(x => x.X_RM_EMP_EMP_CD == SAPCODE))
        //        {
        //            Session["TYPE"] = "RM";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "RM";
        //        }
        //        else if (db.NEW_TEMP_HIERARCHY.Any(x => x.X_BM_EMP_CD == SAPCODE))
        //        {
        //            Session["TYPE"] = "BM";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "BM";
        //        }
        //        else if (db.NEW_TEMP_HIERARCHY.Any(x => x.X_SM_EMP_CD == SAPCODE))
        //        {
        //            Session["TYPE"] = "ARDM";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "ARDM";
        //        }
        //        else if (db.NEW_TEMP_HIERARCHY.Any(x => x.AGENT_CODE == SAPCODE))
        //        {
        //            Session["TYPE"] = "AGENT";
        //            if (Session["LOGIN"] == null) Session["LOGIN"] = "AGENT";
        //        }
        //        else
        //        {
        //            Session.Remove("SAPCODE");
        //            Session.Remove("LOGIN");
        //            Session.Remove("TYPE");
        //        }

        //        // Set additional data
        //        SetData();

        //        // Share Leaderboard
        //        var qry = db.ENGAGE_SHARECOUNT
        //            .SqlQuery(@"
        //        SELECT TOP 10 *
        //        FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS 'SHC_ID', 
        //                CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS 'SHC_SHARECOUNT', 
        //                SHC_SAPCODE AS 'SHC_SAPCODE', 
        //                '' AS 'SHC_PLATEFORM',  
        //                CAST('2022-01-01' AS DATE) AS 'SHC_DATE',  
        //                CREATIVE_ID 
        //            FROM ENGAGE_SHARECOUNT 
        //            GROUP BY SHC_SAPCODE, CREATIVE_ID
        //        ) A")
        //            .ToList<ENGAGE_SHARECOUNT>();

        //        var qry2 = db.ENGAGE_SHARECOUNT
        //            .SqlQuery(@"
        //        SELECT * 
        //        FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS 'SHC_ID', 
        //                CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS 'SHC_SHARECOUNT', 
        //                SHC_SAPCODE AS 'SHC_SAPCODE', 
        //                '' AS 'SHC_PLATEFORM',  
        //                CAST('2022-01-01' AS DATE) AS 'SHC_DATE',  
        //                CREATIVE_ID 
        //            FROM ENGAGE_SHARECOUNT 
        //            GROUP BY SHC_SAPCODE, CREATIVE_ID
        //        ) A 
        //        WHERE A.SHC_SAPCODE = @SAPCODE", new SqlParameter("@SAPCODE", Session["SAPCODE"]))
        //            .FirstOrDefault<ENGAGE_SHARECOUNT>();

        //        ViewBag.ShareLeaderBoard = qry;
        //        ViewBag.ShareLeaderBoardMyRank = qry2;

        //        // Lead Leaderboard
        //        var qry3 = db.Leads
        //            .SqlQuery(@"
        //        SELECT TOP 10 *
        //        FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS 'leads_id', 
        //                CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS 'leads_sapcode', 
        //                '' AS LEADS_NAME, 
        //                CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE, 
        //                CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID, 
        //                CAST('' AS DATETIME) AS LEADS_DATE, 
        //                '' AS LEADS_PLATEFORM,
        //                leads_email, 
        //                api_leads_id
        //            FROM leads 
        //            GROUP BY leads_SAPCODE, leads_email, api_leads_id
        //        ) A")
        //            .ToList<Lead>();

        //        var qry4 = db.Leads
        //            .SqlQuery(@"
        //        SELECT * 
        //        FROM (
        //            SELECT 
        //                CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS 'leads_id', 
        //                CAST(COUNT(leads_SAPCODE) AS VARCHAR(50)) AS 'leads_sapcode', 
        //                '' AS LEADS_NAME, 
        //                CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE, 
        //                CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID, 
        //                CAST('' AS DATETIME) AS LEADS_DATE, 
        //                '' AS LEADS_PLATEFORM, 
        //                leads_email, 
        //                api_leads_id
        //            FROM leads 
        //            GROUP BY leads_SAPCODE, leads_email, api_leads_id
        //        ) A 
        //        WHERE A.leads_SAPCODE = @SAPCODE", new SqlParameter("@SAPCODE", Session["SAPCODE"]))
        //            .FirstOrDefault<Lead>();

        //        ViewBag.LeadLeaderBoard = qry3;
        //        ViewBag.LeadLeaderBoardMyRank = qry4;

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error or handle it appropriately
        //        // Example: TempData["ErrorMessage"] = ex.Message;
        //        // You can also log to file/db
        //        ViewBag.Error = "Something went wrong: " + ex.Message;

        //        // Clear session if needed
        //        Session.Remove("SAPCODE");
        //        Session.Remove("LOGIN");
        //        Session.Remove("TYPE");

        //        return View();
        //    }
        //}











        public ActionResult HotLeads()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                DateTime dt1 = System.DateTime.UtcNow.AddMinutes(330);
                DateTime dt2 = System.DateTime.UtcNow.AddMinutes(330);

                List<SelectListItem> plateform = new List<SelectListItem>()
                {
                    new SelectListItem{Text="Facebook", Value="Facebook"},
                     new SelectListItem{Text="Whatsapp", Value="Whatsapp"},
                    new SelectListItem{Text="Instagram", Value="Instagram"},
                    new SelectListItem{Text="Twitter", Value="Twitter"},
                    new SelectListItem{Text="Linkedin", Value="Linkedin"}
                };

                ViewBag.lstPlateform = plateform;

                ViewBag.dt1 = dt1;
                ViewBag.dt2 = dt2;
                var lstLeads = (from a in db.Leads
                                where a.leads_date >= dt1 && a.leads_date <= dt2
                                join b in db.campaign_master on a.leads_creativeid equals b.campaign_master_id
                                join c in (from temp in db.NEW_TEMP_HIERARCHY
                                           where temp.X_BM_EMP_CD != null
                                           select new { SHC_SAPCODE = temp.X_BM_EMP_CD, temp.X_CHANNEL, SAP_NAME = temp.X_BM_NM, ROLE = "BM" })
                                          .Union
                                          (from temp in db.NEW_TEMP_HIERARCHY
                                           where temp.X_SM_EMP_CD != null
                                           select new { SHC_SAPCODE = temp.X_SM_EMP_CD, temp.X_CHANNEL, SAP_NAME = temp.X_SM_NM, ROLE = "SM" })
                                          //Added by vikas 
                                          .Union
                                          (from temp in db.NEW_TEMP_HIERARCHY
                                           where temp.AGENT_CODE != null
                                           select new { SHC_SAPCODE = temp.AGENT_CODE, temp.X_CHANNEL, SAP_NAME = temp.AGENT_NAME, ROLE = "AGENT" })
                                // code end here
                                on a.leads_sapcode equals c.SHC_SAPCODE
                                select new CustomModel.ViewModelLeads
                                {
                                    cm = b,
                                    l = a,
                                    X_CHANNEL = c.X_CHANNEL
                                }).OrderByDescending(xx => xx.l.leads_date).ToList();
                // .Distinct().ToList();






                ViewBag.lstLeads = lstLeads;

                return View();
            }
        }


        [HttpPost]
        public ActionResult HotLeads(string fromdate, string todate, string plateform)
        {

            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);

            ViewBag.plateform = plateform ?? string.Empty;

            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;
            List<SelectListItem> lstplateform = new List<SelectListItem>()
                {
                    new SelectListItem{Text="Facebook", Value="Facebook"},
                    new SelectListItem{Text="Whatsapp", Value="Whatsapp"},
                    new SelectListItem{Text="Instagram", Value="Instagram"},
                    new SelectListItem{Text="Twitter", Value="Twitter"},
                    new SelectListItem{Text="Linkedin", Value="Linkedin"}
                };

            ViewBag.lstPlateform = lstplateform;


            if (plateform.Trim().Length > 0)
            {
                ViewBag.lstLeads = (from a in db.Leads
                                    where a.leads_date >= dt1 && a.leads_date <= dt2
                                    && a.leads_plateform == plateform
                                    join b in db.campaign_master on a.leads_creativeid equals b.campaign_master_id
                                    join c in (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.X_BM_EMP_CD != null
                                               select new { SHC_SAPCODE = temp.X_BM_EMP_CD, temp.X_CHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.X_BM_NM, ROLE = "BM" })
                .Union
                (from temp in db.NEW_TEMP_HIERARCHY
                 where temp.X_SM_EMP_CD != null
                 select new { SHC_SAPCODE = temp.X_SM_EMP_CD, temp.X_CHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.X_SM_NM, ROLE = "SM" })

                //Added by vikas 
                .Union
                (from temp in db.NEW_TEMP_HIERARCHY
                 where temp.AGENT_CODE != null
                 select new { SHC_SAPCODE = temp.AGENT_CODE, temp.X_CHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.AGENT_NAME, ROLE = "AGENT" })

                                    //Code end here
                                    on a.leads_sapcode equals c.SHC_SAPCODE
                                    select new CustomModel.ViewModelLeads
                                    {
                                        cm = b,
                                        l = a,
                                        X_CHANNEL = c.X_CHANNEL,
                                        X_ZONE = c.X_ZONE,
                                        X_REGION = c.X_REGION,
                                        X_SALES_UNIT_NM = c.X_SALES_UNIT_NM,
                                        X_SALES_UNIT_CD = c.X_SALES_UNIT_CD,
                                        SAP_NAME = c.SAP_NAME
                                        ///Role = c.ROLE
                                    }).OrderByDescending(xx => xx.l.leads_date).ToList();
                //   .Distinct().ToList();



            }
            else
            {
                ViewBag.lstLeads = (from a in db.Leads
                                    where a.leads_date >= dt1 && a.leads_date <= dt2
                                    join b in db.campaign_master on a.leads_creativeid equals b.campaign_master_id
                                    join c in (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.X_BM_EMP_CD != null
                                               select new { SHC_SAPCODE = temp.X_BM_EMP_CD, temp.X_CHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.X_BM_NM, ROLE = "BM" })
                                              .Union
                                              (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.X_SM_EMP_CD != null
                                               select new { SHC_SAPCODE = temp.X_SM_EMP_CD, temp.X_CHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.X_SM_NM, ROLE = "SM" })
                                             //Added by vikas 
                                             .Union
                                              (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.AGENT_CODE != null
                                               select new { SHC_SAPCODE = temp.AGENT_CODE, temp.X_CHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.AGENT_NAME, ROLE = "AGENT" })

                                    //Code end here
                                    on a.leads_sapcode equals c.SHC_SAPCODE
                                    select new CustomModel.ViewModelLeads
                                    {
                                        cm = b,
                                        l = a,
                                        X_CHANNEL = c.X_CHANNEL,
                                        X_ZONE = c.X_ZONE,
                                        X_REGION = c.X_REGION,
                                        X_SALES_UNIT_NM = c.X_SALES_UNIT_NM,
                                        X_SALES_UNIT_CD = c.X_SALES_UNIT_CD,
                                        SAP_NAME = c.SAP_NAME
                                        ///Role = c.ROLE
                                    }).OrderByDescending(xx => xx.l.leads_date).ToList();
                //.Distinct().ToList();


            }




            return View();
        }

        public ActionResult HotLeadsUser()
        {
            SetData();
            // Session["SAPCODE"] = SAPCODE;
            string SAPCODE = Session["SAPCODE"]?.ToString();
            DateTime dt1 = System.DateTime.UtcNow.AddMinutes(330);
            DateTime dt2 = System.DateTime.UtcNow.AddMinutes(330);

            List<SelectListItem> plateform = new List<SelectListItem>()
                {
                    new SelectListItem{Text="Facebook", Value="Facebook"},
                    new SelectListItem{Text="Whatsapp", Value="Whatsapp"},
                    new SelectListItem{Text="Instagram", Value="Instagram"},
                    new SelectListItem{Text="Twitter", Value="Twitter"},
                    new SelectListItem{Text="Linkedin", Value="Linkedin"}
                };

            ViewBag.lstPlateform = plateform;

            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;
            ViewBag.lstLeads = (from a in db.Leads
                                where a.leads_date >= dt1 && a.leads_date <= dt2
                                join b in db.campaign_master on a.leads_creativeid equals b.campaign_master_id
                                join c in (from temp in db.NEW_TEMP_HIERARCHY
                                           where temp.X_BM_EMP_CD != null
                                           select new { SHC_SAPCODE = temp.X_BM_EMP_CD, temp.X_CHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.X_BM_NM, ROLE = "BM" })
                                          .Union
                                          (from temp in db.NEW_TEMP_HIERARCHY
                                           where temp.X_SM_EMP_CD != null
                                           select new { SHC_SAPCODE = temp.X_SM_EMP_CD, temp.X_CHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.X_SM_NM, ROLE = "SM" })
                                           //Added by vikas 
                                           .Union
                                          (from temp in db.NEW_TEMP_HIERARCHY
                                           where temp.AGENT_CODE != null
                                           // select new { SHC_SAPCODE = temp.AGENT_CODE, X_CHANNEL = temp.SUBCHANNEL, temp.X_ZONE, temp.X_REGION, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.AGENT_NAME, ROLE = "AGENT" })
                                           // Added by vikas
                                           select new { SHC_SAPCODE = temp.AGENT_CODE, X_CHANNEL = temp.SUBCHANNEL, temp.X_ZONE, temp.X_REGION, X_SALES_UNIT_NM = temp.AGENT_NAME, X_SALES_UNIT_CD = temp.AGENT_CODE, SAP_NAME = temp.AGENT_NAME, ROLE = "AGENT" })

                                //Code end here 

                                on a.leads_sapcode equals c.SHC_SAPCODE
                                //Added by vikas singh
                                where a.leads_sapcode == SAPCODE
                                select new CustomModel.ViewModelLeads
                                {
                                    cm = b,
                                    l = a,
                                    X_CHANNEL = c.X_CHANNEL,
                                    X_ZONE = c.X_ZONE,
                                    X_REGION = c.X_REGION,
                                    X_SALES_UNIT_NM = c.X_SALES_UNIT_NM,
                                    X_SALES_UNIT_CD = c.X_SALES_UNIT_CD,
                                    SAP_NAME = c.SAP_NAME,



                                    ///Role = c.ROLE
                                }).OrderByDescending(xx => xx.l.leads_date).ToList();
            //.Distinct().ToList();

            return View();
        }

        [HttpPost]
        public ActionResult HotLeadsUser(string fromdate, string todate, string plateform)
        {
            SetData();
            //string SAPCODE = Session["SAPCODE"].ToString();
            string SAPCODE = Session["SAPCODE"]?.ToString();

            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);

            ViewBag.plateform = plateform ?? string.Empty;

            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;
            List<SelectListItem> lstplateform = new List<SelectListItem>()
                {
                    new SelectListItem{Text="Facebook", Value="Facebook"},
                    new SelectListItem{Text="Whatsapp", Value="Whatsapp"},
                    new SelectListItem{Text="Instagram", Value="Instagram"},
                    new SelectListItem{Text="Twitter", Value="Twitter"},
                    new SelectListItem{Text="Linkedin", Value="Linkedin"}
                };

            ViewBag.lstPlateform = lstplateform;


            if (plateform.Trim().Length > 0)
            {
                ViewBag.lstLeads = (from a in db.Leads
                                    where a.leads_date >= dt1 && a.leads_date <= dt2 && a.leads_plateform == plateform
                                    join b in db.campaign_master on a.leads_creativeid equals b.campaign_master_id
                                    join c in (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.X_BM_EMP_CD != null
                                               select new { SHC_SAPCODE = temp.X_BM_EMP_CD, temp.X_CHANNEL, SAP_NAME = temp.X_BM_NM, ROLE = "BM" })
                                              //select new { SHC_SAPCODE = temp.X_ZM_EMP_CD, temp.X_CHANNEL, SAP_NAME = temp.X_BM_NM, ROLE = "BM" })
                                              .Union
                                              (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.X_SM_EMP_CD != null
                                               select new { SHC_SAPCODE = temp.X_SM_EMP_CD, temp.X_CHANNEL, SAP_NAME = temp.X_SM_NM, ROLE = "SM" })
                                              //select new { SHC_SAPCODE = temp.X_ZM_EMP_CD, temp.X_CHANNEL, SAP_NAME = temp.X_SM_NM, ROLE = "SM" })

                                              //Added by vikas 
                                              .Union
                                              (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.AGENT_CODE != null
                                               select new { SHC_SAPCODE = temp.AGENT_CODE, X_CHANNEL = temp.SUBCHANNEL, SAP_NAME = temp.AGENT_NAME, ROLE = "AGENT" })
                                    //Code end here
                                    on a.leads_sapcode equals c.SHC_SAPCODE
                                    //Added by vikas singh
                                    where a.leads_sapcode == SAPCODE
                                    select new CustomModel.ViewModelLeads
                                    {
                                        cm = b,
                                        l = a,
                                        X_CHANNEL = c.X_CHANNEL
                                        ///Role = c.ROLE
                                    }).OrderByDescending(xx => xx.l.leads_date).ToList();
                //.Distinct().ToList();


            }
            else
            {
                ViewBag.lstLeads = (from a in db.Leads
                                    where a.leads_date >= dt1 && a.leads_date <= dt2
                                    join b in db.campaign_master on a.leads_creativeid equals b.campaign_master_id
                                    join c in (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.X_BM_EMP_CD != null
                                               select new { SHC_SAPCODE = temp.X_BM_EMP_CD, temp.X_CHANNEL, SAP_NAME = temp.X_BM_NM, ROLE = "BM" })
                                              .Union
                                              (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.X_SM_EMP_CD != null
                                               select new { SHC_SAPCODE = temp.X_SM_EMP_CD, temp.X_CHANNEL, SAP_NAME = temp.X_SM_NM, ROLE = "SM" })
                                              //Added by vikas 
                                              .Union
                                              (from temp in db.NEW_TEMP_HIERARCHY
                                               where temp.AGENT_CODE != null
                                               select new { SHC_SAPCODE = temp.AGENT_CODE, X_CHANNEL = temp.SUBCHANNEL, SAP_NAME = temp.AGENT_NAME, ROLE = "AGENT" })
                                    //Code end here

                                    on a.leads_sapcode equals c.SHC_SAPCODE
                                    //  Added by vikas singh
                                    where a.leads_sapcode == SAPCODE
                                    select new CustomModel.ViewModelLeads
                                    {
                                        cm = b,
                                        l = a,
                                        X_CHANNEL = c.X_CHANNEL
                                        ///Role = c.ROLE
                                    }).OrderByDescending(xx => xx.l.leads_date).ToList();
                //.Distinct().ToList();
            }




            return View();
        }
        //====================================//


        public ActionResult campaignMult()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // 🟢 Channel Dropdown — Unique Channel Names
            ViewBag.lstchannels = db.channels
                .Where(x => x.channel_delflag == null)
                .GroupBy(x => x.channel_name)
                .Select(g => g.FirstOrDefault())
                .OrderBy(x => x.channel_name)
                .ToList();

            ViewBag.lstcategory = db.campaign_category
                .Where(x => x.campaign_category_delflag == null && x.Campaign_Category_Status == "0")
                .OrderBy(x => x.campaign_category_name)
                .ToList();

            ViewBag.lstcampaign = db.campaigns
                .Where(x => x.campaign_delflag == null)
                .OrderBy(x => x.campaign_name.ToLower())
                .ToList();

            ViewBag.lstsubcampaign = db.subcampaigns
                .Where(x => x.subcampaign_delflag == null)
                .OrderBy(x => x.subcampaign_name)
                .ToList();

            List<SelectListItem> language = new List<SelectListItem>()
        {
            new SelectListItem { Text = "English", Value = "English" },
            new SelectListItem { Text = "Hindi", Value = "Hindi" },
            new SelectListItem { Text = "Marathi", Value = "Marathi" },
            new SelectListItem { Text = "Tamil", Value = "Tamil" }
        };
            ViewBag.lstlanguage = language;

            List<SelectListItem> type = new List<SelectListItem>()
        {
            new SelectListItem { Text = "Blogs", Value = "Blogs" },
            new SelectListItem { Text = "Creative", Value = "Creative" },
            new SelectListItem { Text = "Video", Value = "Video" }
        };
            ViewBag.lsttype = type;

            // Blank default for filters
            ViewBag.channel = "";
            ViewBag.category = "";
            ViewBag.campaign = "";
            ViewBag.subcampaign = "";
            ViewBag.language = "";
            ViewBag.type = "";

            // Campaign list
            var qry1 = db.campaign_master.OrderByDescending(x => x.campaign_master_id).ToList();

            // Status update
            foreach (var item in qry1)
            {
                if (item.camapaign_master_status == "Paused")
                    continue;

                if (item.campaign_master_start_date > DateTime.Now)
                    item.camapaign_master_status = "Upcoming";
                else if (item.campaign_master_start_date <= DateTime.Now &&
                         item.campaign_master_end_date >= DateTime.Now)
                    item.camapaign_master_status = "Active";
                else if (item.campaign_master_end_date < DateTime.Now)
                    item.camapaign_master_status = "Stopped";
            }
            db.SaveChanges();

            ViewBag.lstAllCampaigns = qry1;

            return View();
        }



        [HttpPost]
        public ActionResult campaignMult(FormCollection form)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            List<SelectListItem> language = new List<SelectListItem>()
        {
            new SelectListItem { Text = "English", Value = "English" },
            new SelectListItem { Text = "Hindi", Value = "Hindi" },
            new SelectListItem { Text = "Marathi", Value = "Marathi" },
            new SelectListItem { Text = "Tamil", Value = "Tamil" }
        };
            ViewBag.lstlanguage = language;

            List<SelectListItem> type = new List<SelectListItem>()
        {
            new SelectListItem { Text = "Blogs", Value = "Blogs" },
            new SelectListItem { Text = "Creative", Value = "Creative" },
            new SelectListItem { Text = "Video", Value = "Video" }
        };
            ViewBag.lsttype = type;

            // Get filter values
            string channelVal = form["channel"];
            string categoryVal = form["category"];
            string campaignVal = form["campaign"];
            string subcampaignVal = form["subcampaign"];
            string languageVal = form["language"];
            string typeVal = form["campaigntype"];

            var predicate = PredicateBuilder.True<campaign_master>();

            if (!string.IsNullOrEmpty(channelVal))
            {
                decimal channel = Convert.ToDecimal(channelVal);
                predicate = predicate.And(i => i.channel_id == channel);
            }

            if (!string.IsNullOrEmpty(categoryVal))
            {
                decimal category = Convert.ToDecimal(categoryVal);
                predicate = predicate.And(i => i.campaign_category_id == category);
            }

            if (!string.IsNullOrEmpty(campaignVal))
            {
                decimal campaign = Convert.ToDecimal(campaignVal);
                predicate = predicate.And(i => i.campaign_id == campaign);
            }

            if (!string.IsNullOrEmpty(subcampaignVal))
            {
                decimal subcampaign = Convert.ToDecimal(subcampaignVal);
                predicate = predicate.And(i => i.subcampaign_id == subcampaign);
            }

            if (!string.IsNullOrEmpty(languageVal))
            {
                predicate = predicate.And(i => i.campaign_master_lang == languageVal);
            }

            if (!string.IsNullOrEmpty(typeVal))
            {
                predicate = predicate.And(i => i.campaign_master_type == typeVal);
            }

            // 🟢 Dropdowns reload
            ViewBag.lstchannels = db.channels
                .Where(x => x.channel_delflag == null)
                .GroupBy(x => x.channel_name)
                .Select(g => g.FirstOrDefault())
                .OrderBy(x => x.channel_name)
                .ToList();

            ViewBag.lstcategory = db.campaign_category
                .Where(x => x.campaign_category_delflag == null && x.Campaign_Category_Status == "0")
                .OrderBy(x => x.campaign_category_name)
                .ToList();

            ViewBag.lstcampaign = db.campaigns
                .Where(x => x.campaign_delflag == null)
                .OrderBy(x => x.campaign_name.ToLower())
                .ToList();

            ViewBag.lstsubcampaign = db.subcampaigns
                .Where(x => x.subcampaign_delflag == null)
                .OrderBy(x => x.subcampaign_name)
                .ToList();

            // 🟢 Query data based on filter
            var qry1 = db.campaign_master
                .Where(predicate)
                .OrderByDescending(x => x.campaign_master_id)
                .ToList();

            // Update status logic
            foreach (var item in qry1)
            {
                if (item.camapaign_master_status == "Paused")
                    continue;
                if (item.campaign_master_start_date > DateTime.Now)
                    item.camapaign_master_status = "Upcoming";
                else if (item.campaign_master_start_date <= DateTime.Now &&
                         item.campaign_master_end_date >= DateTime.Now)
                    item.camapaign_master_status = "Active";
                else if (item.campaign_master_end_date < DateTime.Now)
                    item.camapaign_master_status = "Stopped";
            }

            db.SaveChanges();

            ViewBag.lstAllCampaigns = qry1;

            // 🟢 Reset dropdowns and textbox after filter
            ViewBag.channel = "";
            ViewBag.category = "";
            ViewBag.campaign = "";
            ViewBag.subcampaign = "";
            ViewBag.language = "";
            ViewBag.type = "";

            return View();
        }
        //==================================================================================
        public ActionResult campaign(decimal? id)  // <-- added id param for "View Similar" 
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Dropdown fill
            ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
            ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null && x.Campaign_Category_Status == "0").OrderBy(x => x.campaign_category_name).ToList();
            ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).OrderBy(x => x.campaign_name).ToList();
            ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).OrderBy(x => x.subcampaign_name).ToList();

            // Language dropdown
            List<SelectListItem> language = new List<SelectListItem>()
    {
        new SelectListItem { Text = "English", Value = "English" },
        new SelectListItem { Text = "Hindi", Value = "Hindi" },
        new SelectListItem { Text = "Marathi", Value = "Marathi" },
        new SelectListItem { Text = "Tamil", Value = "Tamil" }
    };
            ViewBag.lstlanguage = language;

            // Type dropdown
            List<SelectListItem> type = new List<SelectListItem>()
    {
        new SelectListItem { Text = "Creative", Value = "Creative" },
        new SelectListItem { Text = "Video", Value = "Video" },
        new SelectListItem { Text = "Blogs", Value = "Blogs" }
    };
            ViewBag.lsttype = type;

            ViewBag.channel = "";
            ViewBag.category = "";
            ViewBag.campaign = "";
            ViewBag.subcampaign = "";
            ViewBag.language = "";
            ViewBag.type = "";

            List<campaign_master> qry1;
            if (id != null && id > 0)
            {
                // 🔹 Get clicked campaign info
                var selectedCampaign = db.campaign_master.FirstOrDefault(x => x.campaign_master_id == id);
                if (selectedCampaign != null)
                {
                    string caption = selectedCampaign.campaign_master_creative_caption;
                    string campaignName = selectedCampaign.CampaignName;

                    // 🔹 Find all records having the same Caption & CampaignName as clicked one
                    var groupedCampaigns = db.campaign_master
                        .Where(x => x.campaign_master_creative_caption == caption &&
                                    x.CampaignName == campaignName)
                        .GroupBy(x => new { x.CampaignName, x.campaign_master_creative_caption })
                        .Select(g => new
                        {
                            CampaignName = g.Key.CampaignName,
                            CreativeCaption = g.Key.campaign_master_creative_caption,
                            Count = g.Count()
                        })
                        .ToList();

                    // ✅ Calculate total campaign count
                    ViewBag.TotalCampaignCount = groupedCampaigns.Sum(x => x.Count);

                    // 🧠 Now ViewBag.TotalCampaignCount has a value
                    if (ViewBag.TotalCampaignCount > 1)
                    {
                        // ✅ When more than one campaign exists
                        qry1 = db.campaign_master
                            .Where(x => x.campaign_master_creative_caption == caption
                                     && x.CampaignName == campaignName)
                            .OrderByDescending(x => x.campaign_master_id)
                            .ToList();
                    }
                    else
                    {
                        // ⚙️ Otherwise, show only by ID
                        qry1 = db.campaign_master
                            .Where(x => x.campaign_master_id == id)
                            .OrderByDescending(x => x.campaign_master_id)
                            .ToList();
                    }
                }
                else
                {
                    // 🔹 Default list (when id not found)
                    qry1 = db.campaign_master
                        .Where(x => id == null || x.campaign_master_id == id)
                        .OrderByDescending(x => x.campaign_master_id)
                        .ToList();
                }
            }
            else
            {
                // 🔹 Default list (when id is null)
                qry1 = db.campaign_master
                    .OrderByDescending(x => x.campaign_master_id)
                    .ToList();
            }
            // 🔹 Status calculate by dates
            foreach (var item in qry1)
            {
                if (item.camapaign_master_status == "Paused")
                    continue;

                if (item.campaign_master_start_date > DateTime.Now)
                {
                    item.camapaign_master_status = "Upcoming";
                }
                else if (item.campaign_master_start_date <= DateTime.Now
                         && item.campaign_master_end_date >= DateTime.Now)
                {
                    item.camapaign_master_status = "Active";
                }
                else if (item.campaign_master_end_date < DateTime.Now)
                {
                    item.camapaign_master_status = "Stopped";
                }
            }
            db.SaveChanges();

            ViewBag.lstAllCampaigns = qry1;

            return View();
        }

        [HttpPost]
        public ActionResult campaign(FormCollection form)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Dropdown fill
            List<SelectListItem> language = new List<SelectListItem>()
    {
        new SelectListItem { Text = "English", Value = "English" },
        new SelectListItem { Text = "Hindi", Value = "Hindi" },
        new SelectListItem { Text = "Marathi", Value = "Marathi" },
        new SelectListItem { Text = "Tamil", Value = "Tamil" }
    };
            ViewBag.lstlanguage = language;

            List<SelectListItem> type = new List<SelectListItem>()
    {
        new SelectListItem { Text = "Creative", Value = "Creative" },
        new SelectListItem { Text = "Video", Value = "Video" },
        new SelectListItem { Text = "Blogs", Value = "Blogs" }
    };
            ViewBag.lsttype = type;

            ViewBag.channel = form["channel"] ?? string.Empty;
            ViewBag.category = form["category"] ?? string.Empty;
            ViewBag.campaign = form["campaign"] ?? string.Empty;
            ViewBag.subcampaign = form["subcampaign"] ?? string.Empty;
            ViewBag.language = form["language"] ?? string.Empty;
            ViewBag.type = form["campaigntype"] ?? string.Empty;

            var predicate = PredicateBuilder.True<campaign_master>();

            if (!string.IsNullOrEmpty(form["channel"]))
            {
                decimal channel = Convert.ToDecimal(form["channel"]);
                predicate = predicate.And(i => i.channel_id == channel);
            }

            if (!string.IsNullOrEmpty(form["category"]))
            {
                decimal category = Convert.ToDecimal(form["category"]);
                predicate = predicate.And(i => i.campaign_category_id == category);
            }

            if (!string.IsNullOrEmpty(form["campaign"]))
            {
                decimal campaign = Convert.ToDecimal(form["campaign"]);
                predicate = predicate.And(i => i.campaign_id == campaign);
            }

            if (!string.IsNullOrEmpty(form["subcampaign"]))
            {
                decimal subcampaign = Convert.ToDecimal(form["subcampaign"]);
                predicate = predicate.And(i => i.subcampaign_id == subcampaign);
            }

            if (!string.IsNullOrEmpty(form["language"]))
            {
                string lang = form["language"];
                predicate = predicate.And(i => i.campaign_master_lang == lang);
            }

            if (!string.IsNullOrEmpty(form["campaigntype"]))
            {
                string ctype = form["campaigntype"];
                predicate = predicate.And(i => i.campaign_master_type == ctype);
            }

            ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
            ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).OrderBy(x => x.campaign_category_name).ToList();
            ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).OrderBy(x => x.campaign_name).ToList();
            ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).OrderBy(x => x.subcampaign_name).ToList();

            var qry1 = db.campaign_master.Where(predicate).OrderByDescending(x => x.campaign_master_id).ToList();


            // Status calculate by dates
            foreach (var item in qry1)
            {
                if (item.camapaign_master_status == "Paused")
                    continue;

                if (item.campaign_master_start_date > DateTime.Now)
                {
                    item.camapaign_master_status = "Upcoming";
                }
                else if (item.campaign_master_start_date <= DateTime.Now
                         && item.campaign_master_end_date >= DateTime.Now)
                {
                    item.camapaign_master_status = "Active";
                }
                else if (item.campaign_master_end_date < DateTime.Now)
                {
                    item.camapaign_master_status = "Stopped";
                }
            }
            db.SaveChanges();

            ViewBag.lstAllCampaigns = qry1;

            return View();
        }

        //==========================================================

        //    public ActionResult campaign()  
        //    {
        //        if (Session["userid"] == null)
        //        {
        //            return RedirectToAction("Index");
        //        }

        //        // Dropdown fill
        //        ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
        //        ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).OrderBy(x => x.campaign_category_name).ToList();
        //        ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).OrderBy(x => x.campaign_name).ToList();
        //        ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).OrderBy(x => x.subcampaign_name).ToList();

        //        List<SelectListItem> language = new List<SelectListItem>()
        //{
        //    new SelectListItem { Text = "English", Value = "English" },
        //    new SelectListItem { Text = "Hindi", Value = "Hindi" },
        //    new SelectListItem { Text = "Marathi", Value = "Marathi" },
        //    new SelectListItem { Text = "Tamil", Value = "Tamil" }
        //};
        //        ViewBag.lstlanguage = language;

        //        List<SelectListItem> type = new List<SelectListItem>()
        //{
        //    new SelectListItem { Text = "Creative", Value = "Creative" },
        //    new SelectListItem { Text = "Video", Value = "Video" },
        //    new SelectListItem { Text = "Blogs", Value = "Blogs" }
        //};
        //        ViewBag.lsttype = type;

        //        ViewBag.channel = "";
        //        ViewBag.category = "";
        //        ViewBag.campaign = "";
        //        ViewBag.subcampaign = "";
        //        ViewBag.language = "";
        //        ViewBag.type = "";

        //        // Campaign list
        //        var qry1 = db.campaign_master.OrderByDescending(x => x.campaign_master_id).ToList();

        //        // Status calculate by dates and update in database
        //        foreach (var item in qry1)
        //        {
        //            if (item.camapaign_master_status == "Paused")
        //                continue;

        //            if (item.campaign_master_start_date > DateTime.Now)
        //            {
        //                item.camapaign_master_status = "Upcoming";
        //            }
        //            else if (item.campaign_master_start_date <= DateTime.Now
        //                     && item.campaign_master_end_date >= DateTime.Now)
        //            {
        //                item.camapaign_master_status = "Active";
        //            }
        //            else if (item.campaign_master_end_date < DateTime.Now)
        //            {
        //                item.camapaign_master_status = "Stopped";
        //            }
        //        }
        //        db.SaveChanges(); // Persist status updates to the database

        //        ViewBag.lstAllCampaigns = qry1;

        //        return View();
        //    }


        //    [HttpPost]
        //    public ActionResult campaign(FormCollection form)
        //    {
        //        if (Session["userid"] == null)
        //        {
        //            return RedirectToAction("Index");
        //        }

        //        List<SelectListItem> language = new List<SelectListItem>()
        //    {
        //        new SelectListItem { Text = "English", Value = "English" },
        //        new SelectListItem { Text = "Hindi", Value = "Hindi" },
        //        new SelectListItem { Text = "Marathi", Value = "Marathi" },
        //        new SelectListItem { Text = "Tamil", Value = "Tamil" }
        //    };
        //        ViewBag.lstlanguage = language;

        //        List<SelectListItem> type = new List<SelectListItem>()
        //    {
        //        new SelectListItem { Text = "Creative", Value = "Creative" },
        //        new SelectListItem { Text = "Video", Value = "Video" },
        //        new SelectListItem { Text = "Blogs", Value = "Blogs" }
        //    };
        //        ViewBag.lsttype = type;

        //        ViewBag.channel = form["channel"] ?? string.Empty;
        //        ViewBag.category = form["category"] ?? string.Empty;
        //        ViewBag.campaign = form["campaign"] ?? string.Empty;
        //        ViewBag.subcampaign = form["subcampaign"] ?? string.Empty;
        //        ViewBag.language = form["language"] ?? string.Empty;
        //        ViewBag.type = form["campaigntype"] ?? string.Empty;

        //        var predicate = PredicateBuilder.True<campaign_master>();

        //        if (!string.IsNullOrEmpty(form["channel"]))
        //        {
        //            decimal channel = Convert.ToDecimal(form["channel"]);
        //            predicate = predicate.And(i => i.channel_id == channel);
        //        }

        //        if (!string.IsNullOrEmpty(form["category"]))
        //        {
        //            decimal category = Convert.ToDecimal(form["category"]);
        //            predicate = predicate.And(i => i.campaign_category_id == category);
        //        }

        //        if (!string.IsNullOrEmpty(form["campaign"]))
        //        {
        //            decimal campaign = Convert.ToDecimal(form["campaign"]);
        //            predicate = predicate.And(i => i.campaign_id == campaign);
        //        }

        //        if (!string.IsNullOrEmpty(form["subcampaign"]))
        //        {
        //            decimal subcampaign = Convert.ToDecimal(form["subcampaign"]);
        //            predicate = predicate.And(i => i.subcampaign_id == subcampaign);
        //        }

        //        if (!string.IsNullOrEmpty(form["language"]))
        //        {
        //            string lang = form["language"];
        //            predicate = predicate.And(i => i.campaign_master_lang == lang);
        //        }

        //        if (!string.IsNullOrEmpty(form["campaigntype"]))
        //        {
        //            string ctype = form["campaigntype"];
        //            predicate = predicate.And(i => i.campaign_master_type == ctype);
        //        }

        //        ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
        //        ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).OrderBy(x => x.campaign_category_name).ToList();
        //        ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).OrderBy(x => x.campaign_name).ToList();
        //        ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).OrderBy(x => x.subcampaign_name).ToList();

        //        var qry1 = db.campaign_master.Where(predicate).OrderByDescending(x => x.campaign_master_id).ToList();

        //        // Status calculate by dates and update in database
        //        foreach (var item in qry1)
        //        {
        //            if (item.camapaign_master_status == "Paused")
        //                continue;

        //            if (item.campaign_master_start_date > DateTime.Now)
        //            {
        //                item.camapaign_master_status = "Upcoming";
        //            }
        //            else if (item.campaign_master_start_date <= DateTime.Now
        //                     && item.campaign_master_end_date >= DateTime.Now)
        //            {
        //                item.camapaign_master_status = "Active";
        //            }
        //            else if (item.campaign_master_end_date < DateTime.Now)
        //            {
        //                item.camapaign_master_status = "Stopped";
        //            }
        //        }
        //        db.SaveChanges(); // Persist status updates to the database

        //        ViewBag.lstAllCampaigns = qry1;

        //        return View();
        //    }

        //===========================




        [HttpPost]
        public ActionResult UpdateStatus(string campaign_master_id, string campaign_master_status)
        {
            if (string.IsNullOrWhiteSpace(campaign_master_id) || string.IsNullOrWhiteSpace(campaign_master_status))
                return new HttpStatusCodeResult(400, "Invalid request");

            try
            {
                var idList = campaign_master_id.Split(',')
                    .Select(id => decimal.TryParse(id.Trim(), out var parsedId) ? parsedId : (decimal?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (!idList.Any())
                    return new HttpStatusCodeResult(400, "No valid campaign master IDs provided.");

                var campaigns = db.campaign_master
                    .Where(x => idList.Contains(x.campaign_master_id))
                    .ToList();

                if (!campaigns.Any())
                    return new HttpStatusCodeResult(404, "Campaigns not found.");

                int skippedBecauseActiveToStopped = 0;
                int skippedBecauseStoppedToActiveOrPaused = 0;
                int skippedBecauseInvalidTransition = 0;

                foreach (var campaign in campaigns)
                {
                    string currentStatus = campaign.campaign_master_start_date > DateTime.Now
                        ? "Upcoming"
                        : campaign.camapaign_master_status;

                    // Rule 1: Active campaigns cannot be changed to Stopped
                    if (currentStatus == "Active" && campaign_master_status == "Stopped")
                    {
                        skippedBecauseActiveToStopped++;
                        continue;
                    }

                    // Rule 2: Stopped campaigns cannot be changed to Active or Paused
                    if (currentStatus == "Stopped" && (campaign_master_status == "Active" || campaign_master_status == "Paused"))
                    {
                        skippedBecauseStoppedToActiveOrPaused++;
                        continue;
                    }

                    // Rule 3: Paused campaigns cannot be changed to Stopped
                    if (currentStatus == "Paused" && campaign_master_status == "Stopped")
                    {
                        skippedBecauseStoppedToActiveOrPaused++;
                        continue;
                    }

                    // Rule 4: Only Active can change to Paused
                    if (campaign_master_status == "Paused" && currentStatus != "Active")
                    {
                        skippedBecauseInvalidTransition++;
                        continue;
                    }

                    // Rule 5: Only Paused can change to Active
                    if (campaign_master_status == "Active" && currentStatus != "Paused")
                    {
                        skippedBecauseInvalidTransition++;
                        continue;
                    }

                    // Rule 6: Upcoming campaigns cannot be changed to Active, Paused, or Stopped
                    if (currentStatus == "Upcoming" && (campaign_master_status == "Active" || campaign_master_status == "Paused" || campaign_master_status == "Stopped"))
                    {
                        skippedBecauseStoppedToActiveOrPaused++;
                        continue;
                    }

                    // Update status only if the campaign is not Upcoming
                    if (currentStatus != "Upcoming")
                    {
                        campaign.camapaign_master_status = campaign_master_status;
                    }
                }

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    skippedBecauseActiveToStopped,
                    skippedBecauseStoppedToActiveOrPaused,
                    skippedBecauseInvalidTransition
                });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, $"Error occurred: {ex.Message}");
            }
        }


        //====================================================================
        public ActionResult campaigndetails(decimal CampaignMasterId)
        {
            //if (Session["userid"] == null)
            //{
            //    return RedirectToAction("Index");
            //}
            //else
            //{
            //ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
            //ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
            //ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
            //ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

            ViewBag.lstAllCampaigns = (from a in db.campaign_master
                                       join b in db.channels on a.channel_id equals b.channel_id
                                       join c in db.campaign_category on a.campaign_category_id equals c.campaign_category_id
                                       join d in db.campaigns on a.campaign_id equals d.campaign_id
                                       join e in db.subcampaigns on a.subcampaign_id equals e.subcampaign_id
                                       where a.campaign_master_id == CampaignMasterId
                                       select new CustomModel.ViewModelCampaignCategory
                                       {
                                           cm = a,
                                           ch = b,
                                           cc = c,
                                           cam = d,
                                           sc = e
                                       }).FirstOrDefault();
            return PartialView("_PartialCampaignDetails", ViewBag.lstAllCampaigns);
        }



        public ActionResult campaigndetailsadmin(decimal CampaignMasterId)
        {

            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {

                //ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
                //ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
                //ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
                //ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();
                ViewBag.lstAllCampaigns = (from a in db.campaign_master
                                               //join b in db.channels on a.channel_id equals b.channel_id
                                               //join c in db.campaign_category on a.campaign_category_id equals c.campaign_category_id
                                               //join d in db.campaigns on a.campaign_id equals d.campaign_id
                                           join e in db.subcampaigns on a.subcampaign_id equals e.subcampaign_id
                                           where a.campaign_master_id == CampaignMasterId
                                           select new CustomModel.ViewModelCampaignCategory
                                           {
                                               cm = a,
                                               //ch = b,
                                               //cc = c,
                                               //cam = d,
                                               sc = e
                                           }).FirstOrDefault();

                return View();
            }

        }



        public ActionResult SessionTimeOut()
        {
            ViewBag.Message = "Session Time Out. Please login again.";
            return View();
        }


        public ActionResult campaigndetailsuser(decimal CampaignMasterId)
        {
            try
            {
                //// Optional: Check if user session exists
                //if (Session["SAPCODE"] == null)
                //{
                //    return RedirectToAction("SessionTimeOut", "Marketing");
                //}


                // Load other dropdown data if needed
                SetData();

                // Use LEFT JOIN to ensure subcampaign can be null
                var campaignDetails = (from a in db.campaign_master
                                       join e in db.subcampaigns on a.subcampaign_id equals e.subcampaign_id into se
                                       from e in se.DefaultIfEmpty()
                                       where a.campaign_master_id == CampaignMasterId
                                       select new CustomModel.ViewModelCampaignCategory
                                       {
                                           cm = a,
                                           sc = e
                                       }).FirstOrDefault();

                if (campaignDetails == null)
                {
                    TempData["ErrorMessage"] = "Campaign not found.";
                    return RedirectToAction("campaign"); // Redirect to campaign list if not found
                }

                ViewBag.lstAllCampaigns = campaignDetails;

                return View();
            }
            catch (Exception ex)
            {
                // Log the exception
                // Example: Logger.LogError(ex.Message);

                TempData["ErrorMessage"] = "An error occurred while loading campaign details.";
                return RedirectToAction("campaign"); // Redirect to campaign list on error
            }
        }







        //public ActionResult campaignfbshare(decimal CampaignMasterId,string SAPCODE, string CREATIVEID, string PLATEFORM)
        public ActionResult campaignfbshare(string PARAMS)
        {


            string SAPCODE = string.Empty;
            string PLATEFORM = string.Empty;
            string CREATIVEID = string.Empty;
            decimal CampaignMasterId = 0;
            var valueBytes = Convert.FromBase64String(PARAMS);
            string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

            string[] separate_params = str_REFKEY.Split('&');


            foreach (var item in separate_params)
            {
                if (item.Contains("SAPCODE"))
                {
                    SAPCODE = (item.Split('='))[1];
                    ViewBag.SAPCODE = SAPCODE;
                }
                if (item.Contains("PLATEFORM"))
                {
                    PLATEFORM = (item.Split('='))[1];
                    ViewBag.PLATEFORM = PLATEFORM;
                }
                if (item.Contains("CREATIVEID"))
                {
                    CREATIVEID = (item.Split('='))[1];
                    ViewBag.CREATIVEID = CREATIVEID;
                }
                if (item.Contains("CampaignMasterId"))
                {
                    CampaignMasterId = Convert.ToDecimal((item.Split('='))[1]);
                    ViewBag.CampaignMasterId = CampaignMasterId;
                }
            }

            ViewBag.lstAllCampaigns = (from a in db.campaign_master

                                       where a.campaign_master_id == CampaignMasterId
                                       select new CustomModel.ViewModelCampaignCategory
                                       {
                                           cm = a

                                       }).FirstOrDefault();
            //if (TempData["alert"].ToString() == null || TempData["alert"].ToString() == "Hide" )
            //{
            //    ViewBag.alert = "Hide";
            //}
            //else
            //{
            //    ViewBag.alert = "Show";
            //}

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> campaignfbshare(Lead data, string SAPCODE, string CREATIVEID, string PLATEFORM, string PARAMS)
        //public ActionResult campaignfbshare(string PARAMS, Lead data)
        {
            logger.Info("Inside campaignfbshare");
            logger.Info("data is " + JsonConvert.SerializeObject(data));
            logger.Info("Sapcode is " + SAPCODE);
            logger.Info("CREATIVEID is " + CREATIVEID);
            logger.Info("PLATEFORM is " + PLATEFORM);
            logger.Info("PARAMS is " + PARAMS);

            //string SAPCODE = string.Empty;
            //string PLATEFORM = string.Empty;
            //string CREATIVEID = string.Empty;
            //decimal CampaignMasterId = 0;
            //var valueBytes = Convert.FromBase64String(PARAMS);
            //string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

            //string[] separate_params = str_REFKEY.Split('&');

            //foreach (var item in separate_params)
            //{
            //    if (item.Contains("SAPCODE"))
            //    {
            //        SAPCODE = (item.Split('='))[1];
            //        ViewBag.SAPCODE = SAPCODE;
            //    }
            //    if (item.Contains("PLATEFORM"))
            //    {
            //        PLATEFORM = (item.Split('='))[1];
            //        ViewBag.PLATEFORM = PLATEFORM;
            //    }
            //    if (item.Contains("CREATIVEID"))
            //    {
            //        CREATIVEID = (item.Split('='))[1];
            //        ViewBag.CREATIVEID = CREATIVEID;
            //    }
            //    if (item.Contains("CampaignMasterId"))
            //    {
            //        CampaignMasterId = Convert.ToDecimal((item.Split('='))[1]);
            //        ViewBag.CampaignMasterId = CampaignMasterId;
            //    }
            //}
            string x_channel = "";
            Decimal id = Convert.ToDecimal(CREATIVEID);
            logger.Info("Id is " + id);
            ViewBag.lstAllCampaigns = (from a in db.campaign_master
                                       where a.campaign_master_id == id
                                       select new CustomModel.ViewModelCampaignCategory
                                       {
                                           cm = a

                                       }).FirstOrDefault();
            var existingLead = db.Leads.FirstOrDefault(l => l.leads_mobile == data.leads_mobile && l.leads_creativeid == id);

            if (existingLead != null)
            {
                logger.Info("Lead with the same mobile number and creative ID already exists.");
                ModelState.AddModelError("leads_mobile", "Lead with the same mobile number and creative ID already exists.");
                return View("campaignfbshare");
            }

            if (!string.IsNullOrEmpty(data.leads_mobile.ToString()))
            {
                string mobileRegex = @"^([0-9]{10})$";
                Regex re = new Regex(mobileRegex);
                if (!re.IsMatch(data.leads_mobile.ToString()))
                {
                    logger.Info("Enter a valid Mobile Number");
                    ModelState.AddModelError("leads_mobile", "Enter a valid Mobile Number");
                    return View("campaignfbshare");
                }
            }
            else
            {
                logger.Info("Enter Your Mobile Number");
                ModelState.AddModelError("leads_mobile", "Enter Your Mobile Number");
                return View("campaignfbshare");
            }


            if (ModelState.IsValid)
            {
                logger.Info("Model is valid");
                data.leads_date = System.DateTime.UtcNow.AddMinutes(330);
                data.leads_sapcode = SAPCODE;
                data.leads_creativeid = Convert.ToDecimal(CREATIVEID);
                data.leads_plateform = PLATEFORM;

                db.Leads.Add(data);
                //db.SaveChanges();
                if (db.SaveChanges() > 0)
                {
                    logger.Info("Lead Added");
                    decimal lastInsertedId = data.leads_id;
                    var qry = (from a in db.Leads
                               join b in db.campaign_master on a.leads_creativeid equals b.campaign_master_id
                               join c in db.campaigns on b.campaign_id equals c.campaign_id
                               where a.leads_id == lastInsertedId
                               select new { a, b, c }).FirstOrDefault();

                    string leadtype = string.Empty;
                    if (qry.c.campaign_category_id == 2)
                    {
                        leadtype = "Recruitment";
                    }
                    else
                    {
                        leadtype = "New Business";
                    }
                    TempData["alert"] = "Show";


                    try
                    {
                        bool defaultApi = true;

                        var channel = db.NEW_TEMP_HIERARCHY.FirstOrDefault(x => x.X_SM_EMP_CD == SAPCODE || x.X_BM_EMP_CD == SAPCODE || x.X_ZM_EMP_CD == SAPCODE || x.X_RM_EMP_EMP_CD == SAPCODE);

                        if (channel != null)
                            x_channel = channel.X_CHANNEL;

                        string url = "https://saservices.reliancenipponlife.com/eopsservices/wsLeadActivityPlanning.svc/SaveLeadInformation";
                        string requestBody = "{\"ASM_FLS_Code\":\"\",\"Aadhaar\":\"\",\"AddedByBM_YN\":\"\",\"Added_By\":\"" + qry.a.leads_sapcode + "\",\"Address\":\"\",\"Address_1\":\"\",\"Address_2\":\"\",\"Address_3\":\"\",\"Advisor_Code\":\"\",\"AgeGroup\":\"\",\"Alternate_Number\":\"\",\"AnnualIncome\":\"\",\"AppointmentDate\":\"\",\"AppointmentTime\":\"\",\"BankName\":\"\",\"BranchCode\":\"\",\"BranchName\":\"\",\"CCECode\":\"\",\"CRMLeadType\":\"\",\"CallType\":\"\",\"Campaign\":\"" + qry.c.campaign_name + "\",\"City\":\"\",\"Commute_Time\":\"\",\"CustomerBaseBBC\":\"\",\"DOB\":\"\",\"DOB_Changed\":\"\",\"Dependents\":\"\",\"Device_Id\":\"\",\"Educational_Background\":\"\",\"Email_ID\":\"\",\"FLS_Sapcode\":\"\",\"From_Address\":\"\",\"From_Latitude\":\"\",\"From_Longitude\":\"\",\"Gender\":\"\",\"Income\":\"\",\"InsLoginType\":\"\",\"Is_Updated\":\"\",\"Landline\":\"\",\"Latitude\":\"\",\"LeadInfo_Remarks\":\"\",\"Lead_From_Contact_List\":\"\",\"Lead_Source\":\"\",\"Lead_Status\":\"\",\"Lead_Sub_Source\":\"\",\"Lead_Sub_Type\":\"\",\"Lead_Type\":\"" + leadtype + "\",\"LifeStage\":\"\",\"Longitude\":\"\",\"Marital_Status\":\"\",\"Mobile\":\"" + qry.a.leads_mobile.ToString() + "\",\"Name\":\"" + qry.a.leads_name + "\",\"OTP\":\"\",\"OTPSentYN\":\"\",\"Occupation\":\"\",\"Occupation_Remarks\":\"\",\"Pin_Code\":\"\",\"Policy_Number\":\"\",\"RNLICCustomer_YN\":\"\",\"Reference_LeadID\":\"\",\"Reference_YN\":\"\",\"Referred_By\":\"\",\"+qry.a.leads_sapcode+\":\"\",\"Source_From\":\"SMC Invest/SMC Sales\",\"State\":\"\",\"Sub_Activity_Options\":\"\",\"Sync_Txn_Id\":\"\",\"Sync_by\":\"" + SAPCODE + "\",\"User_Role\":\"\",\"Variance_Lat_Long\":\"\",\"Verification_Type\":\"\",\"Verticals_SPR\":\"\",\"WithoutWelcomeCodeYN\":\"\"}";

                        logger.Info("X_channel is " + x_channel);

                        if (x_channel == "DB" || x_channel == "DP" || x_channel == "DL" || x_channel == "PC")
                        {

                            defaultApi = false;
                            url = "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";
                            requestBody = "{\"LeadDetails\":[{\"Attribute\":\"SearchBy\",\"Value\":\"Phone\"},{\"Attribute\":\"__UseUserDefinedGuid__\",\"Value\":\"true\"},{\"Attribute\":\"Source\",\"Value\":\"DIGIMYIN\"},{\"Attribute\":\"Phone\",\"Value\":\"" + qry.a.leads_mobile.ToString() + "\"}],\"Opportunity\":{\"OpportunityEventCode\":12000,\"OpportunityNote\":\"Opportunity\",\"OverwriteFields\":true,\"DoNotPostDuplicateActivity\":false,\"DoNotChangeOwner\":true,\"Fields\":[{\"SchemaName\":\"mx_Custom_2\",\"Value\":\"New Lead\"},{\"SchemaName\":\"Status\",\"Value\":\"Open\"},{\"SchemaName\":\"mx_Custom_11\",\"Value\":\"" + qry.c.campaign_name + "\"},{\"SchemaName\":\"mx_Custom_1\",\"Value\":\"" + qry.a.leads_name + "\"},{\"SchemaName\":\"mx_Custom_35\",\"Value\":\"\"},{\"SchemaName\":\"mx_Custom_13\",\"Value\":\"1990-01-01\"},{\"SchemaName\":\"mx_Custom_12\",\"Value\":\"New Business\"},{\"SchemaName\":\"mx_Custom_43\",\"Value\":\"indusind Nippon Life Super Suraksha\"},{\"SchemaName\":\"mx_Custom_44\",\"Value\":\"Health Plan\"},{\"SchemaName\":\"mx_Custom_45\",\"Value\":\"188\"},{\"SchemaName\":\"mx_Custom_15\",\"Value\":\"Up To 3 Lacs\"},{\"SchemaName\":\"mx_Custom_54\",\"Value\":\"Individual\"},{\"SchemaName\":\"mx_Custom_27\",\"Value\":\"DIGIMYIN\"},{\"SchemaName\":\"mx_Custom_46\",\"Value\":\"\",\"Fields\":[{\"SchemaName\":\"mx_CustomObject_1\",\"Value\":\"API\"},{\"SchemaName\":\"mx_Custom_36.mx_CustomObject_1\",\"Value\":\"" + SAPCODE + "\"},{\"SchemaName\":\"mx_CustomObject_2\",\"Value\":\"DM\"}]}]}}";
                            //  requestBody = "{\"LeadDetails\":[{\"Attribute\":\"SearchBy\",\"Value\":\"Phone\"},{\"Attribute\":\"__UseUserDefinedGuid__\",\"Value\":\"true\"},{\"Attribute\":\"Source\",\"Value\":\"DIGIMYIN\"},{\"Attribute\":\"Phone\",\"Value\":\"9875478985\"}],\"Opportunity\":{\"OpportunityEventCode\":12000,\"OpportunityNote\":\"Opportunity\",\"OverwriteFields\":true,\"DoNotPostDuplicateActivity\":false,\"DoNotChangeOwner\":true,\"Fields\":[{\"SchemaName\":\"mx_Custom_2\",\"Value\":\"New Lead\"},{\"SchemaName\":\"Status\",\"Value\":\"Open\"},{\"SchemaName\":\"mx_Custom_11\",\"Value\":\"Product Campaign\"},{\"SchemaName\":\"mx_Custom_1\",\"Value\":\"vikas\"},{\"SchemaName\":\"mx_Custom_35\",\"Value\":\"\"},{\"SchemaName\":\"mx_Custom_13\",\"Value\":\"1990-01-01\"},{\"SchemaName\":\"mx_Custom_12\",\"Value\":\"New Business\"},{\"SchemaName\":\"mx_Custom_43\",\"Value\":\"indusind Nippon Life Super Suraksha\"},{\"SchemaName\":\"mx_Custom_44\",\"Value\":\"Health Plan\"},{\"SchemaName\":\"mx_Custom_45\",\"Value\":\"188\"},{\"SchemaName\":\"mx_Custom_15\",\"Value\":\"Up To 3 Lacs\"},{\"SchemaName\":\"mx_Custom_54\",\"Value\":\"Individual\"},{\"SchemaName\":\"mx_Custom_27\",\"Value\":\"DIGIMYIN\"},{\"SchemaName\":\"mx_Custom_46\",\"Value\":\"\",\"Fields\":[{\"SchemaName\":\"mx_CustomObject_1\",\"Value\":\"API\"},{\"SchemeName\":\"mx_Custom_36.mx_CustomObject_1\",\"Value\":\"70657623\"},{\"SchemaName\":\"mx_CustomObject_2\",\"Value\":\"DM\"}]}]}}";

                            //  httpWReq.Headers["Origin"] = "https://karma.indusindnipponlife.com";
                            // httpWReq.Headers["x-client-id"] = "22a0b4e5-940f-40a4-984d-6af1ac8e8c9e";
                        }

                        logger.Info("Default Api is " + defaultApi);
                        logger.Info("Default URL is " + url);
                        logger.Info("Request body Api is " + requestBody);


                        //  string url = "https://karma.indusindnipponlife.com/v1.0/nlms/push-leads";
                        //  string requestBody = "{\"userId\":\"" + qry.a.leads_sapcode + "\",\"sl\":{\"loginType\":\"\",\"leadFrom\":\"\",\"leadType\":\"" + leadtype + "\",\"leadSubType\":\"New Prospect\",\"advisorId\":\"\",\"gender\":\"Male\",\"maritialStatus\":\"Married\",\"name\":\"" + qry.a.leads_name + "\",\"dateOfBrith\":\"220924800000\",\"occupation\":\"Business\",\"incomeBand\":\"Up to 3 Lacs\",\"educationalGroup\":\"Below 10th Standard\",\"phoneNo\":\"" + qry.a.leads_mobile.ToString() + "\",\"alternatePhoneNo\":\"\",\"landline\":\"\",\"address\":\"\",\"state\":\"Maharastra\",\"city\":\"Mumbai\",\"postalcode\":\"\",\"emailId\":\"\",\"campaign\":\"" + qry.c.campaign_name + "\",\"deviceId\":\"\",\"leadSource\":\"\",\"leadSubSource\":\"\",\"longitute\":\"\",\"latitude\":\"\",\"ageBand\":\"\",\"pan\":\"\",\"verticals\":\"\",\"prospectType\":\"\",\"lifeStage\":\"\",\"source\":\"\",\"whatsappNo\":\"\",\"isExistingCustomer\":\"\",\"channel\":\"\",\"isWithWelcomeCode\":\"\",\"spId\":\"\",\"asmId\":\"\",\"spName\":\"\",\"asmName\":\"\",\"branchCode\":\"\",\"branchName\":\"\",\"callType\":\"\",\"vertical\":\"ISG\"}}";

                        // string requestBody = "{\"userId\":\"" + qry.a.leads_sapcode + "\",\"loginType\":\"Individual\",\"leadType\":\"" + leadtype + "\",\"leadProfileType\":\"\",\"leadSubType\":\"New Prospect\",\"leadSource\":\"Customer Portal\",\"leadSubSource\":\"\",\"gender\":\"Male\",\"maritialStatus\":\"Married\",\"name\":\"" + qry.a.leads_name + "\",\"isExistingCustomer\":\"Y\",\"policyNo\":\"\",\"dateOfBrith\":\"28-AUG-1969\",\"occupation\":\"\",\"incomeBand\":\"_3_lacs_to_6_lacs\",\"lifeStage\":\"Married\",\"educationalGroup\":\"\",\"phoneNo\":\"" + qry.a.leads_mobile.ToString() + "\",\"landline\":\"\",\"alternatePhoneNo\":\"\",\"whatsappNo\":\"\",\"address\":\"203-204/2A, Brindaban II,\",\"state\":\"Maharastra\",\"city\":\"Mumbai\",\"postalcode\":\"400093\",\"emailId\":\"\",\"campaign\":\"Customer Portal\",\"ageBand\":\"\",\"vertical\":\"\",\"callType\":\"\",\"spId\":\"\",\"spName\":\"\",\"teleCallerName\":\"\",\"teleCallerId\":\"\",\"asmId\":\"\",\"asmName\":\"\",\"advisorName\":\"Ashu Gupta test\",\"advisorId\":\"70000099\",\"pan\":\"AUEPS2835F\",\"linkedLead\":[]}";

                        // Create the request
                        if (defaultApi)
                        {
                            HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(url);
                            httpWReq.Method = "POST";
                            httpWReq.ContentType = "application/json";

                            //Added by vikas
                            //if(!defaultApi)
                            //{
                            //    httpWReq.Headers["Origin"] = "https://karma.reliancenipponlife.com";
                            //    httpWReq.Headers["x-client-id"] = "22a0b4e5-940f-40a4-984d-6af1ac8e8c9e";
                            //}
                            //Code end here

                            // Add cookies if needed
                            ////httpWReq.Headers.Add("Cookie", "ARRAffinity=14ddb9344dde9c1ee40459559b99c5eba0e53ac9bc666c127a1811352f9ea86e; ASP.NET_SessionId=03zod5441rhqgrilvwjhjc2f");

                            // Convert the request body to a byte array
                            byte[] api_data = Encoding.UTF8.GetBytes(requestBody);

                            // Set the content length
                            httpWReq.ContentLength = api_data.Length;

                            // Get the request stream and write the data
                            using (Stream stream = httpWReq.GetRequestStream())
                            {
                                stream.Write(api_data, 0, api_data.Length);
                            }
                            // Get the response
                            using (HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse())
                            {
                                // Read the response data
                                using (Stream responseStream = response.GetResponseStream())
                                {
                                    using (StreamReader reader = new StreamReader(responseStream))
                                    {
                                        string responseText = reader.ReadToEnd();
                                        // Console.WriteLine("Response: " + responseText);
                                        if (!string.IsNullOrEmpty(responseText))
                                        {
                                            // Parse the API response to extract the Lead_Id
                                            var apiResponse = JsonConvert.DeserializeObject<ApiResponose>(responseText);

                                            // Retrieve the last inserted ID from the Leads table

                                            string leadIdToUpdate = apiResponse.Lead_Id.ToString();
                                            // Update ap_leads_id in the Leads table
                                            Lead leadToUpdate = db.Leads.FirstOrDefault(l => l.leads_id == lastInsertedId);
                                            if (leadToUpdate != null)
                                            {
                                                logger.Info("Lead API Id is " + leadIdToUpdate);
                                                leadToUpdate.api_leads_id = leadIdToUpdate; // Update ap_leads_id with the Lead_Id from the API response
                                                db.SaveChanges();
                                            }
                                        }

                                    }

                                }
                            }
                        }
                        else
                        {
                            var client = new HttpClient();
                            var request = new HttpRequestMessage(HttpMethod.Post, "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27");
                            var content = new StringContent("{\"LeadDetails\":[{\"Attribute\":\"SearchBy\",\"Value\":\"Phone\"},{\"Attribute\":\"__UseUserDefinedGuid__\",\"Value\":\"true\"},{\"Attribute\":\"Source\",\"Value\":\"DIGIMYIN\"},{\"Attribute\":\"Phone\",\"Value\":\"9875478985\"}],\"Opportunity\":{\"OpportunityEventCode\":12000,\"OpportunityNote\":\"Opportunity\",\"OverwriteFields\":true,\"DoNotPostDuplicateActivity\":false,\"DoNotChangeOwner\":true,\"Fields\":[{\"SchemaName\":\"mx_Custom_2\",\"Value\":\"New Lead\"},{\"SchemaName\":\"Status\",\"Value\":\"Open\"},{\"SchemaName\":\"mx_Custom_11\",\"Value\":\"Product Campaign\"},{\"SchemaName\":\"mx_Custom_1\",\"Value\":\"vikas\"},{\"SchemaName\":\"mx_Custom_35\",\"Value\":\"\"},{\"SchemaName\":\"mx_Custom_13\",\"Value\":\"1990-01-01\"},{\"SchemaName\":\"mx_Custom_12\",\"Value\":\"New Business\"},{\"SchemaName\":\"mx_Custom_43\",\"Value\":\"indusind Nippon Life Super Suraksha\"},{\"SchemaName\":\"mx_Custom_44\",\"Value\":\"Health Plan\"},{\"SchemaName\":\"mx_Custom_45\",\"Value\":\"188\"},{\"SchemaName\":\"mx_Custom_15\",\"Value\":\"Up To 3 Lacs\"},{\"SchemaName\":\"mx_Custom_54\",\"Value\":\"Individual\"},{\"SchemaName\":\"mx_Custom_27\",\"Value\":\"DIGIMYIN\"},{\"SchemaName\":\"mx_Custom_46\",\"Value\":\"\",\"Fields\":[{\"SchemaName\":\"mx_CustomObject_1\",\"Value\":\"API\"},{\"SchemaName\":\"mx_Custom_36.mx_CustomObject_1\",\"Value\":\"70657623\"},{\"SchemaName\":\"mx_CustomObject_2\",\"Value\":\"DM\"}]}]}}", null, "application/json");
                            logger.Info(content);
                            request.Content = content;
                            var response = await client.SendAsync(request);
                            logger.Info(response);
                            response.EnsureSuccessStatusCode();
                            var result = await response.Content.ReadAsStringAsync();
                            var apiResponse = JsonConvert.DeserializeObject<ApiResponose>(result);

                            logger.Info("update leads set status=" + apiResponse.Status + ",exception_type='" + apiResponse.ExceptionType + "',exception_message='" + apiResponse.ExceptionMessage + "',request_id='" + apiResponse.RequestId + "' where leads_id=" + lastInsertedId);
                            db.Database.ExecuteSqlCommand("update leads set status=" + apiResponse.Status + ",exception_type='" + apiResponse.ExceptionType + "',exception_message='" + apiResponse.ExceptionMessage + "',request_id='" + apiResponse.RequestId + "' where leads_id=" + lastInsertedId);
                        }

                    }
                    catch (WebException ex)
                    {
                        Response.Headers.Add("Error", ex.Message);
                        logger.Error(ex);
                    }
                    ///SendData(data.leads_id);

                    return RedirectToAction("campaignfbshare", new { @PARAMS = PARAMS });
                }

            }

            TempData["alert"] = "Hide";
            return View("campaignfbshare", new { @PARAMS = PARAMS });




        }

        //Added new method by vikas singh


        //public async Task<string> TestAPI()
        //{
        //    using (var client = new HttpClient())
        //    {
        //        var request = new HttpRequestMessage(
        //            HttpMethod.Post,
        //            "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27"
        //        );

        //        var jsonBody = "{ YOUR CLEAN JSON BODY }";

        //        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"); 

        //        var response = await client.SendAsync(request);
        //        var result = await response.Content.ReadAsStringAsync();

        //        try
        //        {
        //            var apiResult = JsonConvert.DeserializeObject<LeadSquaredResponse>(result);
        //            return apiResult?.RequestId; // 🔥 always return RequestId
        //        }
        //        catch
        //        {
        //            return "PARSE_FAILED";
        //        }
        //    }
        //}



        //public async Task<string> TestAPI()
        //{
        //    var r = "inside api";
        //    var client = new HttpClient();
        //    client.DefaultRequestHeaders.ConnectionClose = true;
        //    var request = new HttpRequestMessage(HttpMethod.Post, "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27");
        //    var content = new StringContent("{\"LeadDetails\":[{\"Attribute\":\"SearchBy\",\"Value\":\"Phone\"},{\"Attribute\":\"_UseUserDefinedGuid_\",\"Value\":\"true\"},{\"Attribute\":\"Source\",\"Value\":\"DIGIMYIN\"},{\"Attribute\":\"Phone\",\"Value\":\"9875478985\"}],\"Opportunity\":{\"OpportunityEventCode\":12000,\"OpportunityNote\":\"Opportunity\",\"OverwriteFields\":true,\"DoNotPostDuplicateActivity\":false,\"DoNotChangeOwner\":true,\"Fields\":[{\"SchemaName\":\"mx_Custom_27\",\"Value\":\"New Lead\"},{\"SchemaName\":\"Status\",\"Value\":\"Open\"},{\"SchemaName\":\"mx_Custom_11\",\"Value\":\"Product Campaign\"},{\"SchemaName\":\"mx_Custom_1\",\"Value\":\"vikas\"},{\"SchemaName\":\"mx_Custom_35\",\"Value\":\"\"},{\"SchemaName\":\"mx_Custom_13\",\"Value\":\"1990-01-01\"},{\"SchemaName\":\"mx_Custom_12\",\"Value\":\"New Business\"},{\"SchemaName\":\"mx_Custom_43\",\"Value\":\"indusind Nippon Life Super Suraksha\"},{\"SchemaName\":\"mx_Custom_44\",\"Value\":\"Health Plan\"},{\"SchemaName\":\"mx_Custom_45\",\"Value\":\"188\"},{\"SchemaName\":\"mx_Custom_15\",\"Value\":\"Up To 3 Lacs\"},{\"SchemaName\":\"mx_Custom_54\",\"Value\":\"Individual\"},{\"SchemaName\":\"mx_Custom_27\",\"Value\":\"DIGIMYIN\"},{\"SchemaName\":\"mx_Custom_46\",\"Value\":\"\",\"Fields\":[{\"SchemaName\":\"mx_CustomObject_1\",\"Value\":\"API\"},{\"SchemaName\":\"mx_Custom_36.mx_CustomObject_1\",\"Value\":\"70657623\"},{\"SchemaName\":\"mx_CustomObject_2\",\"Value\":\"DM\"}]}]}}", null, "application/json");
        //    request.Content = content;
        //    try
        //    {
        //        r = "before send";
        //        var response = await client.SendAsync(request);
        //        r = "after send";
        //        response.EnsureSuccessStatusCode();
        //        r = await response.Content.ReadAsStringAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        r = ex.Message;
        //        r = ex.StackTrace;
        //    }
        //    return r;
        //    //Console.WriteLine(await response.Content.ReadAsStringAsync());
        //}

        //===================correct code =================================================

        private static readonly HttpClient _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };


        //    public async Task<string> TestAPI()
        //    {
        //        var url =
        //            "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture" +
        //            "?accessKey=u$re5470b019cbada9931f9d41d27261f60" +
        //            "&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";

        //        var jsonBody = @"
        //{
        //  ""LeadDetails"": [
        //    { ""Attribute"": ""SearchBy"", ""Value"": ""Phone"" },
        //    { ""Attribute"": ""Source"", ""Value"": ""DIGIMYIN"" },
        //    { ""Attribute"": ""Phone"", ""Value"": ""9875478985"" }
        //  ],
        //  ""Opportunity"": {
        //    ""OpportunityEventCode"": 12000,
        //    ""OpportunityNote"": ""Opportunity"",
        //    ""OverwriteFields"": true,
        //    ""DoNotPostDuplicateActivity"": false,
        //    ""DoNotChangeOwner"": true,
        //    ""Fields"": [
        //      { ""SchemaName"": ""mx_Custom_27"", ""Value"": ""New Lead"" },
        //      { ""SchemaName"": ""Status"", ""Value"": ""Open"" }
        //    ]
        //  }
        //}";

        //        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        //        var response = await _client.PostAsync(url, content);
        //        var responseJson = await response.Content.ReadAsStringAsync();

        //        var apiResult = JsonConvert.DeserializeObject<LeadSquaredResponse>(responseJson);

        //        // ✅ Safe return (RequestId / RequestID / fallback)
        //        return apiResult?.RequestId
        //            ?? apiResult?.RequestId
        //            ?? "";
        //    }



      //===================================================


        public async Task<(LeadSquaredResponse Response, string RawJson)> TestAPI(string name,string mobile,string sapcode,string SapName)
        {
            var url = "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture" +
                      "?accessKey=u$re5470b019cbada9931f9d41d27261f60" +
                      "&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";

            var jsonBody = $@"
{{
  ""LeadDetails"": [
    {{ ""Attribute"": ""SearchBy"", ""Value"": ""Phone"" }},
    {{ ""Attribute"": ""__UseUserDefinedGuid__"", ""Value"": ""true"" }},
    {{ ""Attribute"": ""Source"", ""Value"": ""DigiMyin"" }},
    {{ ""Attribute"": ""FirstName"", ""Value"": ""{name}"" }},
    {{ ""Attribute"": ""Phone"", ""Value"": ""{mobile}"" }}
  ],
  ""Opportunity"": {{
    ""OpportunityEventCode"": 12000,
    ""OpportunityNote"": ""Opportunity"",
    ""OverwriteFields"": true,
    ""DoNotPostDuplicateActivity"": false,
    ""DoNotChangeOwner"": true,
    ""Fields"": [
      {{ ""SchemaName"": ""mx_Custom_2"", ""Value"": ""New Lead"" }},
      {{ ""SchemaName"": ""Status"", ""Value"": ""Open"" }},
      {{ ""SchemaName"": ""mx_Custom_11"", ""Value"": ""Digimyin"" }},
      {{ ""SchemaName"": ""mx_Custom_1"", ""Value"": ""{name}"" }},
      {{ ""SchemaName"": ""mx_Custom_35"", ""Value"": ""Mukhedkar Complex, Shivaji Putala, Vazirabad, Nanded, Maharashtra, India 431601"" }},
      {{
        ""SchemaName"": ""mx_Custom_36"",
        ""Value"": """",
        ""Fields"": [
          {{ ""SchemaName"": ""mx_CustomObject_1"", ""Value"": ""{sapcode}"" }},
          {{ ""SchemaName"": ""mx_CustomObject_2"", ""Value"": ""{SapName}"" }},
          {{ ""SchemaName"": ""mx_CustomObject_3"", ""Value"": ""777777"" }}
        ]
      }},
      {{ ""SchemaName"": ""mx_Custom_32"", ""Value"": ""431601"" }},
      {{ ""SchemaName"": ""mx_Custom_13"", ""Value"": ""1971-12-11"" }},
      {{ ""SchemaName"": ""mx_Custom_12"", ""Value"": ""New Business"" }},
      {{ ""SchemaName"": ""mx_Custom_43"", ""Value"": "" Indusind Nippon Life Super Suraksha"" }},
      {{ ""SchemaName"": ""mx_Custom_44"", ""Value"": ""Health Plan"" }},
      {{ ""SchemaName"": ""mx_Custom_45"", ""Value"": ""188"" }},
      {{ ""SchemaName"": ""mx_Custom_15"", ""Value"": ""20-50 Lacs"" }},
      {{ ""SchemaName"": ""mx_Custom_27"", ""Value"": ""API"" }},
      {{ ""SchemaName"": ""mx_Custom_54"", ""Value"": ""Individual"" }},
      {{
        ""SchemaName"": ""mx_Custom_46"",
        ""Value"": """",
        ""Fields"": [
          {{ ""SchemaName"": ""mx_CustomObject_1"", ""Value"": ""API"" }},
          {{ ""SchemaName"": ""mx_CustomObject_2"", ""Value"": ""DM"" }}
        ] 
      }}
    ]
  }}
}}";

            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(url, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var obj = JsonConvert.DeserializeObject<LeadSquaredResponse>(responseJson);

            return (obj, responseJson);
        }

        private string CleanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Only Alphabets + Space allowed
            return Regex.Replace(name, @"[^a-zA-Z\s]", "").Trim();
        }


        public async Task<(LeadSquaredResponse Response, string RawJson)> TestAPI_SE(
            string name,
            string mobile,
            string sapcode,
            string SapName,string categoryName)
        {
            var url = "https://sa3dev.reliancenipponlife.com/v1.0/nlms/push-leads";

            // 🔥 Clean Data (API strict validation karta hai)
            string cleanedName = Regex.Replace(name ?? "", @"[^a-zA-Z\s]", "").Trim();
            string cleanMobile = Regex.Replace(mobile ?? "", @"\D", "");

                            var jsonBody = $@"
                {{
                    ""userId"": ""9050022"",
                    ""name"": ""Sunil Sarsande"",
                    ""phoneNo"": ""9890455817"",
                    ""apiSource"": ""digimyin"",
                    ""loginType"": ""Individual"",
                    ""leadType"": ""{categoryName}""
                }}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            // 🔥 Required Headers
            request.Headers.Add("x-client-id", "22a0b4e5-940f-40a4-984d-6af1ac8e8c9e");
            request.Headers.TryAddWithoutValidation("x-token", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJkYXRhIjp7InVzZXJJZCI6IjExMTExMSIsImRldmljZUlkIjoiNDkyNTBkMDUzM2JjZTkxMSJ9LCJpYXQiOjE3MjA0NjEyMzUsImV4cCI6MTcyMDQ5MDAzNX0.gdImEjRsw-XR-BaYckwiE1zRjhuhLB1RmnDPnRJMyFIbDENP-udS56M27N5GpaRSP9ffpFH_fKEtUEFawEvE3ckvGth5fKEXZW0eIcbMLGfKPJ4DS3U_zoeqXG_4bvaGjaUCFCvZxS2rf5P5e_Yg3_OJzuhq4IvuruUEfHLwnAJYP-s0qi0RmPGyUH1C0rGcjlCW8dP6V0_DyHDYeivuA6-vkjJGaYPvP2kbM3zQP9vxDg9hfayv5y7Cp5oFUbrvylCD6k9rTClMlPpyUn2MLFfvVSHxKgFVCvI844RLAaUG78hFI4Sf-gJVUiCiO3T4JHjNqBExFqZI4zNdF1uMmw");   // 👈 Add this
            request.Headers.Add("origin", "https://sa3dev.reliancenipponlife.com");

            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            LeadSquaredResponse apiResponse = new LeadSquaredResponse();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    dynamic obj = JsonConvert.DeserializeObject(responseJson);

                    apiResponse.Status = 1;
                    apiResponse.RequestId = obj?.id ?? obj?.leadId ?? Guid.NewGuid().ToString();
                }
                catch
                {
                    apiResponse.Status = 1;
                    apiResponse.RequestId = Guid.NewGuid().ToString();
                }
            }
            else
            {
                apiResponse.Status = 0;
                apiResponse.RequestId = null;
                apiResponse.ExceptionMessage = responseJson;
            }

            return (apiResponse, responseJson);
        }







        //==============================================================================






        //        public async Task<(LeadSquaredResponse Response, string RawJson)> TestAPI()
        //        {
        //            var url = "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture" +
        //                      "?accessKey=u$re5470b019cbada9931f9d41d27261f60" +
        //                      "&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";

        //            var jsonBody = @"
        //{
        //  ""LeadDetails"": [
        //    { ""Attribute"": ""SearchBy"", ""Value"": ""Phone"" },
        //    { ""Attribute"": ""__UseUserDefinedGuid__"", ""Value"": ""true"" },
        //    { ""Attribute"": ""Source"", ""Value"": ""DigiMyin"" },
        //    { ""Attribute"": ""Phone"", ""Value"": ""7738223341"" }
        //  ],
        //  ""Opportunity"": {
        //    ""OpportunityEventCode"": 12000,
        //    ""OpportunityNote"": ""Opportunity"",
        //    ""OverwriteFields"": true,
        //    ""DoNotPostDuplicateActivity"": false,
        //    ""DoNotChangeOwner"": true,
        //    ""Fields"": [
        //      { ""SchemaName"": ""mx_Custom_2"", ""Value"": ""New Lead"" },
        //      { ""SchemaName"": ""Status"", ""Value"": ""Open"" },
        //      { ""SchemaName"": ""mx_Custom_11"", ""Value"": ""Navyug"" },
        //      { ""SchemaName"": ""mx_Custom_1"", ""Value"": ""DM Test"" },
        //      { ""SchemaName"": ""mx_Custom_35"", ""Value"": ""Mukhedkar Complex, Shivaji Putala, Vazirabad, Nanded, Maharashtra, India 431601"" },
        //      {
        //        ""SchemaName"": ""mx_Custom_36"",
        //        ""Value"": """",
        //        ""Fields"": [
        //          { ""SchemaName"": ""mx_CustomObject_1"", ""Value"": ""70642064"" },
        //          { ""SchemaName"": ""mx_CustomObject_2"", ""Value"": ""Kamlesh"" },
        //          { ""SchemaName"": ""mx_CustomObject_3"", ""Value"": ""777777"" }
        //        ]
        //      },
        //      { ""SchemaName"": ""mx_Custom_32"", ""Value"": ""431601"" },
        //      { ""SchemaName"": ""mx_Custom_13"", ""Value"": ""1971-12-11"" },
        //      { ""SchemaName"": ""mx_Custom_12"", ""Value"": ""New Business"" },
        //      { ""SchemaName"": ""mx_Custom_43"", ""Value"": ""Reliance Nippon Life Super Suraksha"" },
        //      { ""SchemaName"": ""mx_Custom_44"", ""Value"": ""Health Plan"" },
        //      { ""SchemaName"": ""mx_Custom_45"", ""Value"": ""188"" },
        //      { ""SchemaName"": ""mx_Custom_15"", ""Value"": ""20-50 Lacs"" },
        //      { ""SchemaName"": ""mx_Custom_27"", ""Value"": ""Navyug leads"" },
        //      { ""SchemaName"": ""mx_Custom_54"", ""Value"": ""Individual"" },
        //      {
        //        ""SchemaName"": ""mx_Custom_46"",
        //        ""Value"": """",
        //        ""Fields"": [
        //          { ""SchemaName"": ""mx_CustomObject_1"", ""Value"": ""API"" },
        //          { ""SchemaName"": ""mx_CustomObject_2"", ""Value"": ""DM"" }
        //        ]
        //      }
        //    ]
        //  }
        //}";

        //            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        //            var response = await _client.PostAsync(url, content);
        //            var responseJson = await response.Content.ReadAsStringAsync();

        //            var obj = JsonConvert.DeserializeObject<LeadSquaredResponse>(responseJson);

        //            return (obj, responseJson);
        //        }

        //=======================================================correct cht=================
        //  public async Task<(LeadSquaredResponse Response, string RawJson)> TestAPI()
        //        {
        //            var url = "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture" +
        //                      "?accessKey=u$re5470b019cbada9931f9d41d27261f60" +
        //                      "&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";

        //            var jsonBody = @"
        //{
        //  ""LeadDetails"": [
        //    { ""Attribute"": ""SearchBy"", ""Value"": ""Phone"" },
        //    { ""Attribute"": ""_UseUserDefinedGuid_"", ""Value"": ""true"" },
        //    { ""Attribute"": ""Source"", ""Value"": ""DIGIMYIN"" },
        //    { ""Attribute"": ""Phone"", ""Value"": ""7738223341"" }
        //  ],
        //  ""Opportunity"": {
        //    ""OpportunityEventCode"": 12000,
        //    ""OpportunityNote"": ""Opportunity"",
        //    ""OverwriteFields"": true,
        //    ""DoNotPostDuplicateActivity"": false,
        //    ""DoNotChangeOwner"": true,
        //    ""Fields"": [
        //      { ""SchemaName"": ""mx_Custom_2"", ""Value"": ""New Lead"" },
        //      { ""SchemaName"": ""Status"", ""Value"": ""Open"" },
        //      { ""SchemaName"": ""mx_Custom_11"", ""Value"": ""Navyug"" },
        //      { ""SchemaName"": ""mx_Custom_1"", ""Value"": ""DM Test"" },
        //      { ""SchemaName"": ""mx_Custom_35"", ""Value"": ""Mukhedkar Complex, Shivaji Putala, Vazirabad, Nanded, Maharashtra, India 431601"" },
        //      { ""SchemaName"": ""mx_Custom_36.mx_CustomObject_1"", ""Value"": ""70301675"" },
        //      { ""SchemaName"": ""mx_Custom_32"", ""Value"": ""431601"" },
        //      { ""SchemaName"": ""mx_Custom_13"", ""Value"": ""1971-12-11"" },
        //      { ""SchemaName"": ""mx_Custom_12"", ""Value"": ""New Business"" },
        //      { ""SchemaName"": ""mx_Custom_43"", ""Value"": ""Reliance Nippon Life Super Suraksha"" },
        //      { ""SchemaName"": ""mx_Custom_44"", ""Value"": ""Health Plan"" },
        //      { ""SchemaName"": ""mx_Custom_45"", ""Value"": ""188"" },
        //      { ""SchemaName"": ""mx_Custom_15"", ""Value"": ""20-50 Lacs"" },
        //      { ""SchemaName"": ""mx_Custom_27"", ""Value"": ""Navyug leads"" },
        //      { ""SchemaName"": ""mx_Custom_54"", ""Value"": ""Individual/Renewal/Business/CDA"" },
        //      {
        //        ""SchemaName"": ""mx_Custom_46"",
        //        ""Value"": """",
        //        ""Fields"": [
        //          { ""SchemaName"": ""mx_CustomObject_1"", ""Value"": ""API"" },
        //          { ""SchemaName"": ""mx_CustomObject_2"", ""Value"": ""DM"" }
        //        ]
        //      }
        //    ]
        //  }
        //}";

        //            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        //            var response = await _client.PostAsync(url, content);
        //            var responseJson = await response.Content.ReadAsStringAsync();

        //            var obj = JsonConvert.DeserializeObject<LeadSquaredResponse>(responseJson);

        //            return (obj, responseJson);
        //        }
        //===========================correct amir===============================================================
        //        public async Task<(LeadSquaredResponse Response, string RawJson)> TestAPI()
        //        {
        //            //var url =
        //            //    "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture" +
        //            //    "?accessKey=u$re5470b019cbada9931f9d41d27261f60" +
        //            //    "&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";

        //            var url = "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";

        //            //            var jsonBody = @"
        //            //{
        //            //  ""LeadDetails"": [
        //            //    { ""Attribute"": ""SearchBy"", ""Value"": ""Phone"" },
        //            //    { ""Attribute"": ""Source"", ""Value"": ""DIGIMYIN"" },
        //            //    { ""Attribute"": ""Phone"", ""Value"": ""9875478985"" }
        //            //  ],
        //            //  ""Opportunity"": {
        //            //    ""OpportunityEventCode"": 12000,
        //            //    ""OpportunityNote"": ""Opportunity"",
        //            //    ""OverwriteFields"": true,
        //            //    ""DoNotPostDuplicateActivity"": false,
        //            //    ""DoNotChangeOwner"": true,
        //            //    ""Fields"": [
        //            //      { ""SchemaName"": ""mx_Custom_27"", ""Value"": ""New Lead"" },
        //            //      { ""SchemaName"": ""Status"", ""Value"": ""Open"" }
        //            //    ]
        //            //  }
        //            //}";
        //            var jsonBody = @"
        //{
        //    ""LeadDetails"": [
        //        {
        //            ""Attribute"": ""SearchBy"",
        //            ""Value"": ""Phone""
        //        },
        //        {
        //            ""Attribute"": ""__UseUserDefinedGuid__"",
        //            ""Value"": ""DIGIMYIN""
        //        },
        //        {
        //            ""Attribute"": ""Source"", 
        //            ""Value"": ""9875478985""
        //        },
        //        {
        //            ""Attribute"": ""Phone"",
        //            ""Value"": ""7738223341""
        //        }

        //    ],
        //    ""Opportunity"": {
        //        ""OpportunityEventCode"": 12000,
        //        ""OpportunityNote"": ""Opportunity"",
        //        ""OverwriteFields"": true,
        //        ""DoNotPostDuplicateActivity"": false,
        //        ""DoNotChangeOwner"": true, //These fields to be kept static
        //        ""Fields"": [
        //            {
        //                ""SchemaName"": ""mx_Custom_2"",
        //                ""Value"": ""New Lead""
        //            },
        //            {
        //                ""SchemaName"": ""Status"",
        //                ""Value"": ""Open""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_11"",
        //                ""Value"": ""Navyug""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_1"",
        //                ""Value"": ""DM Test""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_35"",
        //                ""Value"": ""Mukhedkar Complex, Shivaji Putala, Vazirabad,
        //Nanded, Maharashtra, India 431601""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_36.mx_CustomObject_1"",
        //                ""Value"": ""70301675""
        //            },


        //     {
        //                ""SchemaName"": ""mx_Custom_32"",
        //                ""Value"": ""431601""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_13"",
        //                ""Value"": ""1971-12-11""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_12"", 
        //                ""Value"": ""New Business""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_43"", 
        //                ""Value"": ""Reliance Nippon Life Super Suraksha""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_44"", 
        //                ""Value"": ""Health Plan""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_45"",
        //                ""Value"": ""188"" //Plan No
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_15"", 
        //                ""Value"": ""20-50 Lacs""
        //            },
        //            {
        //                ""SchemaName"": ""mx_Custom_27"", 
        //                ""Value"": ""Navyug leads""
        //            },
        //     {
        //                ""SchemaName"": ""mx_Custom_54"",
        //                ""Value"": ""Individual/Renewal/Business/CDA""
        //            },

        //            {
        //                ""SchemaName"": ""mx_Custom_46"",
        //                ""Value"": """",
        //                ""Fields"": [
        //                    {
        //                        ""SchemaName"": ""mx_CustomObject_1"",
        //                        ""Value"": ""API"" //To be static
        //                    },
        //                    {
        //                        ""SchemaName"": ""mx_CustomObject_2"",
        //                        ""Value"": ""DM"" //To be static
        //                    }
        //                ]
        //            }
        //        ]
        //    }
        //}";


        //            var content = new StringContent(jsonBody, Encoding.UTF8, "application /json");
        //            var response = await _client.PostAsync(url, content);

        //            var responseJson = await response.Content.ReadAsStringAsync();

        //            var obj = JsonConvert.DeserializeObject<LeadSquaredResponse>(responseJson);

        //            return (obj, responseJson);
        //        }





        public async Task<string> TestAPI1()
        {
            var r = "inside api => ";
            var client = new HttpClient();
            client.DefaultRequestHeaders.ConnectionClose = true;
            client.Timeout = TimeSpan.FromMinutes(5);
            var url = "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27");
            var content = new StringContent("{\"LeadDetails\":[{\"Attribute\":\"SearchBy\",\"Value\":\"Phone\"},{\"Attribute\":\"_UseUserDefinedGuid_\",\"Value\":\"true\"},{\"Attribute\":\"Source\",\"Value\":\"DIGIMYIN\"},{\"Attribute\":\"Phone\",\"Value\":\"9875478985\"}],\"Opportunity\":{\"OpportunityEventCode\":12000,\"OpportunityNote\":\"Opportunity\",\"OverwriteFields\":true,\"DoNotPostDuplicateActivity\":false,\"DoNotChangeOwner\":true,\"Fields\":[{\"SchemaName\":\"mx_Custom_2\",\"Value\":\"New Lead\"},{\"SchemaName\":\"Status\",\"Value\":\"Open\"},{\"SchemaName\":\"mx_Custom_11\",\"Value\":\"Product Campaign\"},{\"SchemaName\":\"mx_Custom_1\",\"Value\":\"vikas\"},{\"SchemaName\":\"mx_Custom_35\",\"Value\":\"\"},{\"SchemaName\":\"mx_Custom_13\",\"Value\":\"1990-01-01\"},{\"SchemaName\":\"mx_Custom_12\",\"Value\":\"New Business\"},{\"SchemaName\":\"mx_Custom_43\",\"Value\":\"indusind Nippon Life Super Suraksha\"},{\"SchemaName\":\"mx_Custom_44\",\"Value\":\"Health Plan\"},{\"SchemaName\":\"mx_Custom_45\",\"Value\":\"188\"},{\"SchemaName\":\"mx_Custom_15\",\"Value\":\"Up To 3 Lacs\"},{\"SchemaName\":\"mx_Custom_54\",\"Value\":\"Individual\"},{\"SchemaName\":\"mx_Custom_27\",\"Value\":\"DIGIMYIN\"},{\"SchemaName\":\"mx_Custom_46\",\"Value\":\"\",\"Fields\":[{\"SchemaName\":\"mx_CustomObject_1\",\"Value\":\"API\"},{\"SchemaName\":\"mx_Custom_36.mx_CustomObject_1\",\"Value\":\"70657623\"},{\"SchemaName\":\"mx_CustomObject_2\",\"Value\":\"DM\"}]}]}}", null, "application/json");
            request.Content = content;
            try
            {
                r += "before send => ";
                var response = await client.PostAsync(url, content);
                r += "after send => ";
                response.EnsureSuccessStatusCode();
                r += await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                r += ex.Message + " => ";
                r += ex.StackTrace;
            }
            return r;
            //Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
        public async Task<string> TestAPI2()
        {
            var r = "inside api => ";
            var client = new HttpClient();
            client.DefaultRequestHeaders.ConnectionClose = true;
            client.Timeout = TimeSpan.FromMinutes(5);
            client.BaseAddress = new Uri("https://api-in21.leadsquared.com/v2/OpportunityManagement.svc");
            var url = "/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27");
            var content = new StringContent("{\"LeadDetails\":[{\"Attribute\":\"SearchBy\",\"Value\":\"Phone\"},{\"Attribute\":\"_UseUserDefinedGuid_\",\"Value\":\"true\"},{\"Attribute\":\"Source\",\"Value\":\"DIGIMYIN\"},{\"Attribute\":\"Phone\",\"Value\":\"9875478985\"}],\"Opportunity\":{\"OpportunityEventCode\":12000,\"OpportunityNote\":\"Opportunity\",\"OverwriteFields\":true,\"DoNotPostDuplicateActivity\":false,\"DoNotChangeOwner\":true,\"Fields\":[{\"SchemaName\":\"mx_Custom_2\",\"Value\":\"New Lead\"},{\"SchemaName\":\"Status\",\"Value\":\"Open\"},{\"SchemaName\":\"mx_Custom_11\",\"Value\":\"Product Campaign\"},{\"SchemaName\":\"mx_Custom_1\",\"Value\":\"vikas\"},{\"SchemaName\":\"mx_Custom_35\",\"Value\":\"\"},{\"SchemaName\":\"mx_Custom_13\",\"Value\":\"1990-01-01\"},{\"SchemaName\":\"mx_Custom_12\",\"Value\":\"New Business\"},{\"SchemaName\":\"mx_Custom_43\",\"Value\":\"indusind Nippon Life Super Suraksha\"},{\"SchemaName\":\"mx_Custom_44\",\"Value\":\"Health Plan\"},{\"SchemaName\":\"mx_Custom_45\",\"Value\":\"188\"},{\"SchemaName\":\"mx_Custom_15\",\"Value\":\"Up To 3 Lacs\"},{\"SchemaName\":\"mx_Custom_54\",\"Value\":\"Individual\"},{\"SchemaName\":\"mx_Custom_27\",\"Value\":\"DIGIMYIN\"},{\"SchemaName\":\"mx_Custom_46\",\"Value\":\"\",\"Fields\":[{\"SchemaName\":\"mx_CustomObject_1\",\"Value\":\"API\"},{\"SchemaName\":\"mx_Custom_36.mx_CustomObject_1\",\"Value\":\"70657623\"},{\"SchemaName\":\"mx_CustomObject_2\",\"Value\":\"DM\"}]}]}}", null, "application/json");
            request.Content = content;
            try
            {
                r += "before send => ";
                var response = await client.PostAsync(url, content);
                r += "after send => ";
                response.EnsureSuccessStatusCode();
                r += await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                r += ex.Message + " => ";
                r += ex.StackTrace;
            }
            return r;
            //Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        //Code end here





        // public ActionResult SendData(decimal? lead_id, string x_channel="")
        public ActionResult SendData(decimal? lead_id)
        {
            // string url = "https://saservices.reliancenipponlife.com/eopsservices/wsLeadActivityPlanning.svc/SaveLeadInformation";
            //string requestBody = "{\"ASM_FLS_Code\":\"\",\"Aadhaar\":\"\",\"AddedByBM_YN\":\"\",\"Added_By\":\"258743\",\"Address\":\"\",\"Address_1\":\"\",\"Address_2\":\"\",\"Address_3\":\"\",\"Advisor_Code\":\"\",\"AgeGroup\":\"\",\"Alternate_Number\":\"\",\"AnnualIncome\":\"\",\"AppointmentDate\":\"\",\"AppointmentTime\":\"\",\"BankName\":\"\",\"BranchCode\":\"\",\"BranchName\":\"\",\"CCECode\":\"\",\"CRMLeadType\":\"\",\"CallType\":\"\",\"Campaign\":\"TEST 19 JAN\",\"City\":\"\",\"Commute_Time\":\"\",\"CustomerBaseBBC\":\"\",\"DOB\":\"\",\"DOB_Changed\":\"\",\"Dependents\":\"\",\"Device_Id\":\"\",\"Educational_Background\":\"\",\"Email_ID\":\"\",\"FLS_Sapcode\":\"\",\"From_Address\":\"\",\"From_Latitude\":\"\",\"From_Longitude\":\"\",\"Gender\":\"\",\"Income\":\"\",\"InsLoginType\":\"\",\"Is_Updated\":\"\",\"Landline\":\"\",\"Latitude\":\"\",\"LeadInfo_Remarks\":\"\",\"Lead_From_Contact_List\":\"\",\"Lead_Source\":\"\",\"Lead_Status\":\"\",\"Lead_Sub_Source\":\"\",\"Lead_Sub_Type\":\"\",\"Lead_Type\":\"Recruitment\",\"LifeStage\":\"\",\"Longitude\":\"\",\"Marital_Status\":\"\",\"Mobile\":\"8080905083\",\"Name\":\"ASHUTOSH\",\"OTP\":\"\",\"OTPSentYN\":\"\",\"Occupation\":\"\",\"Occupation_Remarks\":\"\",\"Pin_Code\":\"\",\"Policy_Number\":\"\",\"RNLICCustomer_YN\":\"\",\"Reference_LeadID\":\"\",\"Reference_YN\":\"\",\"Referred_By\":\"\",\"SP_Code\":\"\",\"Source_From\":\"SMC Invest/SMC Sales\",\"State\":\"\",\"Sub_Activity_Options\":\"\",\"Sync_Txn_Id\":\"\",\"Sync_by\":\"123456\",\"User_Role\":\"\",\"Variance_Lat_Long\":\"\",\"Verification_Type\":\"\",\"Verticals_SPR\":\"\",\"WithoutWelcomeCodeYN\":\"\"}";

            string url = "https://karma.reliancenipponlife.com/v1.0/nlms/push-leads";
            //string url = "https://karma.indusindnipponlife.com/v1.0/nlms/push-leads";
            string requestBody = "{\"userId\":\"\",\"sl\":{\"loginType\":\"\",\"leadFrom\":\"\",\"leadType\":\"\",\"leadSubType\":\"\",\"advisorId\":\"\",\"gender\":\"\",\"maritialStatus\":\"\",\"name\":\"\",\"dateOfBrith\":\"\",\"occupation\":\"\",\"incomeBand\":\"\",\"educationalGroup\":\"\",\"phoneNo\":\"\",\"alternatePhoneNo\":\"\",\"landline\":\"\",\"address\":\" \",\"state\":\"\",\"city\":\"\",\"postalcode\":\"\",\"emailId\":\"\",\"campaign\":\"\",\"deviceId\":\"\",\"leadSource\":\"\",\"leadSubSource\":\"\",\"longitute\":\"\",\"latitude\":\"\",\"ageBand\":\"\",\"pan\":\"\",\"verticals\":\"\",\"prospectType\":\"\",\"lifeStage\":\"\",\"source\":\"\",\"whatsappNo\":\"\",\"isExistingCustomer\":\"\",\"channel\":\"\",\"isWithWelcomeCode\",\"spId\":\"\",\"asmId\":\"\",\"spName\":\"\",\"asmName\":\"\",\"branchCode\":\"\",\"branchName\":\"\",\"callType\":\"\",\"vertical\":\"ISG\"}}";

            // string requestBody = "{\"userId\":\"70000099\",\"loginType\":\"Individual\",\"leadType\":\"New Business\",\"leadProfileType\":\"\",\"leadSubType\":\"New Prospect\",\"leadSource\":\"Customer Portal\",\"leadSubSource\":\"\",\"gender\":\"Male\",\"maritialStatus\":\"Married\",\"name\":\"Ravish Pandey\",\"isExistingCustomer\":\"Y\",\"policyNo\":\"\",\"dateOfBrith\":\"28-AUG-1969\",\"occupation\":\"\",\"incomeBand\":\"_3_lacs_to_6_lacs\",\"lifeStage\":\"Married\",\"educationalGroup\":\"\",\"phoneNo\":\"9503740233\",\"landline\":\"\",\"alternatePhoneNo\":\"\",\"whatsappNo\":\"\",\"address\":\"203-204/2A, Brindaban II,\",\"state\":\"\",\"city\":\"\",\"postalcode\":\"400093\",\"emailId\":\"\",\"campaign\":\"Customer Portal\",\"ageBand\":\"\",\"vertical\":\"\",\"callType\":\"\",\"spId\":\"\",\"spName\":\"\",\"teleCallerName\":\"\",\"teleCallerId\":\"\",\"asmId\":\"\",\"asmName\":\"\",\"advisorName\":\"Ashu Gupta test\",\"advisorId\":\"70000099\",\"pan\":\"AUEPS2835F\",\"linkedLead\":[]}";

            //  string requestBody = "{\"userId\":\"" + qry.a.leads_sapcode + "\",\"loginType\":\"Individual\",\"leadType\":\"" + leadtype + "\",\"leadProfileType\":\"\",\"leadSubType\":\"New Prospect\",\"leadSource\":\"Customer Portal\",\"leadSubSource\":\"\",\"gender\":\"Male\",\"maritialStatus\":\"Married\",\"name\":\"" + qry.a.leads_name + "\",\"isExistingCustomer\":\"Y\",\"policyNo\":\"\",\"dateOfBrith\":\"28-AUG-1969\",\"occupation\":\"\",\"incomeBand\":\"_3_lacs_to_6_lacs\",\"lifeStage\":\"Married\",\"educationalGroup\":\"\",\"phoneNo\":\"" + qry.a.leads_mobile.ToString() + "\",\"landline\":\"\",\"alternatePhoneNo\":\"\",\"whatsappNo\":\"\",\"address\":\"203-204/2A, Brindaban II,\",\"state\":\"Maharastra\",\"city\":\"Mumbai\",\"postalcode\":\"400093\",\"emailId\":\"\",\"campaign\":\"Customer Portal\",\"ageBand\":\"\",\"vertical\":\"\",\"callType\":\"\",\"spId\":\"\",\"spName\":\"\",\"teleCallerName\":\"\",\"teleCallerId\":\"\",\"asmId\":\"\",\"asmName\":\"\",\"advisorName\":\"Ashu Gupta test\",\"advisorId\":\"70000099\",\"pan\":\"AUEPS2835F\",\"linkedLead\":[]}";

            //if(x_channel == "DA")
            //{
            //     url = "http://lifelineuat.reliancelife.com/RassistServices/wsLeadActivityPlanning.svc/SaveLeadInformation";

            //   requestBody = "{\"ASM_FLS_Code\":\"\",\"Aadhaar\":\"\",\"AddedByBM_YN\":\"\",\"Added_By\":\"258743\",\"Address\":\"\",\"Address_1\":\"\",\"Address_2\":\"\",\"Address_3\":\"\",\"Advisor_Code\":\"\",\"AgeGroup\":\"\",\"Alternate_Number\":\"\",\"AnnualIncome\":\"\",\"AppointmentDate\":\"\",\"AppointmentTime\":\"\",\"BankName\":\"\",\"BranchCode\":\"\",\"BranchName\":\"\",\"CCECode\":\"\",\"CRMLeadType\":\"\",\"CallType\":\"\",\"Campaign\":\"TEST 19 JAN\",\"City\":\"\",\"Commute_Time\":\"\",\"CustomerBaseBBC\":\"\",\"DOB\":\"\",\"DOB_Changed\":\"\",\"Dependents\":\"\",\"Device_Id\":\"\",\"Educational_Background\":\"\",\"Email_ID\":\"\",\"FLS_Sapcode\":\"\",\"From_Address\":\"\",\"From_Latitude\":\"\",\"From_Longitude\":\"\",\"Gender\":\"\",\"Income\":\"\",\"InsLoginType\":\"\",\"Is_Updated\":\"\",\"Landline\":\"\",\"Latitude\":\"\",\"LeadInfo_Remarks\":\"\",\"Lead_From_Contact_List\":\"\",\"Lead_Source\":\"\",\"Lead_Status\":\"\",\"Lead_Sub_Source\":\"\",\"Lead_Sub_Type\":\"\",\"Lead_Type\":\"Recruitment\",\"LifeStage\":\"\",\"Longitude\":\"\",\"Marital_Status\":\"\",\"Mobile\":\"8080905083\",\"Name\":\"ASHUTOSH\",\"OTP\":\"\",\"OTPSentYN\":\"\",\"Occupation\":\"\",\"Occupation_Remarks\":\"\",\"Pin_Code\":\"\",\"Policy_Number\":\"\",\"RNLICCustomer_YN\":\"\",\"Reference_LeadID\":\"\",\"Reference_YN\":\"\",\"Referred_By\":\"\",\"SP_Code\":\"\",\"Source_From\":\"SMC Invest/SMC Sales\",\"State\":\"\",\"Sub_Activity_Options\":\"\",\"Sync_Txn_Id\":\"\",\"Sync_by\":\"123456\",\"User_Role\":\"\",\"Variance_Lat_Long\":\"\",\"Verification_Type\":\"\",\"Verticals_SPR\":\"\",\"WithoutWelcomeCodeYN\":\"\"}";

            //}

            // Create the request
            HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(url);
            httpWReq.Method = "POST";
            httpWReq.ContentType = "application/json";

            // Add cookies if needed
            ////httpWReq.Headers.Add("Cookie", "ARRAffinity=14ddb9344dde9c1ee40459559b99c5eba0e53ac9bc666c127a1811352f9ea86e; ASP.NET_SessionId=03zod5441rhqgrilvwjhjc2f");

            // Convert the request body to a byte array
            byte[] data = Encoding.UTF8.GetBytes(requestBody);

            // Set the content length
            httpWReq.ContentLength = data.Length;

            // Get the request stream and write the data
            using (Stream stream = httpWReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                // Get the response
                using (HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse())
                {
                    // Read the response data
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            string responseText = reader.ReadToEnd();
                            Console.WriteLine("Response: " + responseText);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                // Handle exceptions
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    Console.WriteLine("Error: " + errorResponse.StatusCode);
                }
                else
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            return View();

        }

        //==================







        public ActionResult ForgotPassOtp()
        {
            return View();
        }



        [HttpPost]
        public JsonResult SendForgotOtp(string username)
        {
            var user = db.authentications
                         .FirstOrDefault(x => x.au_username == username &&
                                              (x.Role == "Admin" || x.Role == "SubAdmin" || x.Role == "MasterAdmin"));

            if (user == null)
            {
                return Json(new { success = false, message = "Invalid Username or Role ❌" });
            }

            // Generate OTP
            Random rnd = new Random();
            int otp = rnd.Next(100000, 999999);

            Session["OTP"] = otp.ToString();
            Session["Username"] = username;
            Session["IsVerified"] = null;

            string message = $"Dear {username}, your OTP for login into ARDM Activity Tracker is {otp}. This OTP is valid for only 2mins. T&C Apply. -RNLIC";

            try
            {
                if (username.Contains("@")) // Send Email
                {
                    var emailJson = new
                    {
                        subject = "OTP Verification",
                        body = message,
                        filename = "",
                        file = "NA",
                        emailid = username,
                        firstname = "User",
                        lastname = "",
                        policyno = "NA",
                        source = "ARDM",
                        tag = "otp"
                    };

                    using (var client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.ContentType] = "text/plain";
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(emailJson);
                        client.UploadString("https://extapi.indusindnipponlife.com/EmailService/send", "POST", json);
                    }
                }
                else // Send SMS
                {
                    string encodedText = System.Web.HttpUtility.UrlEncode(message);
                    string url = $"https://bulkpush.mytoday.com/BulkSms/SingleMsgApi?" +
                                 $"feedid=376053&username=9900000002&password=pwmtg&To={username}" +
                                 $"&Text={encodedText}&time=202409051156&templateid=1107173192276056259&senderid=RNLICT";

                    using (var client = new WebClient())
                    {
                        client.DownloadString(url);
                    }
                }

                return Json(new { success = true, message = "OTP sent successfully ✅" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to send OTP ❌" });
            }
        }

        [HttpPost]
        public JsonResult VerifyOTPAjax(string username, string otp)
        {
            string sessionOtp = Session["OTP"]?.ToString();
            string sessionUsername = Session["Username"]?.ToString();

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(sessionUsername))
            {
                return Json(new { success = false, message = "Session expired. Please try again ❌" });
            }

            if (username != sessionUsername)
            {
                return Json(new { success = false, message = "Username mismatch ❌" });
            }

            if (otp != sessionOtp)
            {
                return Json(new { success = false, message = "Invalid OTP ❌" });
            }

            Session["IsVerified"] = true;

            return Json(new
            {
                success = true,
                message = "OTP verified successfully ✅",
                redirectUrl = Url.Action("ForgotPass", "Marketing")
            });
        }
        //==================================Forgotpass============= kamlesh===========
        public ActionResult ForgotPass()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ForgotPasswordAjax(string username, string newPassword, string confirmPassword)
        {
            try
            {
                if (Session["IsVerified"] == null || (bool)Session["IsVerified"] == false)
                {
                    return Json(new { success = false, message = "OTP not verified." });
                }

                var user = db.authentications.FirstOrDefault(x => x.au_username == username);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "Passwords do not match." });
                }

                user.au_password = newPassword;
                db.SaveChanges();

                Session["OTP"] = null;
                Session["Username"] = null;
                Session["IsVerified"] = null;

                return Json(new { success = true, message = "Password updated successfully ✅" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        //==================================
        [HttpPost]
        public JsonResult AjaxMethod(string type, int value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type) || value <= 0)
                {
                    return Json(new List<SelectListItem>());
                }

                List<SelectListItem> items = new List<SelectListItem>();

                switch (type)
                {
                    case "cm_campaign_category_id":
                        var campaigns = db.campaigns
                            .Where(x => x.campaign_delflag == null && x.campaign_category_id == value)
                            .Select(x => new SelectListItem
                            {
                                Text = x.campaign_name,
                                Value = x.campaign_id.ToString()
                            })
                            .ToList();
                        items.AddRange(campaigns);
                        break;

                    case "cm_campaign_id":
                        var subCampaigns = db.subcampaigns
                            .Where(x => x.subcampaign_delflag == null && x.campaign_id == value)
                            .Select(x => new SelectListItem
                            {
                                Text = x.subcampaign_name,
                                Value = x.subcampaign_id.ToString()
                            })
                            .ToList();
                        items.AddRange(subCampaigns);
                        break;

                    default:
                        return Json(new List<SelectListItem>());
                }

                return Json(items);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return Json(new List<SelectListItem>());
            }
        }


        public ActionResult Newcampaign()
        {
            try
            {
                if (Session["userid"] == null)
                {
                    return RedirectToAction("Index");
                }

                // ✅ Channels
                ViewBag.lstchannels = db.channels
                    .Where(x => x.channel_delflag == null)
                    .OrderBy(x => x.channel_name)
                    .ToList();

                // ✅ Campaign Categories (sirf active)
                ViewBag.lstcategory = db.campaign_category
                    .Where(x => x.campaign_category_delflag == null
                             && x.Campaign_Category_Status == "0")
                    .OrderBy(x => x.campaign_category_name)
                    .ToList();

                // ✅ Campaigns
                var campaign = db.campaigns
                    .Where(a => a.campaign_delflag == null)
                    .OrderBy(a => a.campaign_name)
                    .ToList();

                List<SelectListItem> camp = new List<SelectListItem>();
                if (campaign != null && campaign.Count > 0)
                {
                    foreach (var item in campaign)
                    {
                        camp.Add(new SelectListItem
                        {
                            Text = item.campaign_name,
                            Value = item.campaign_id.ToString()
                        });
                    }
                }
                ViewBag.lstcampaign = camp;

                // ✅ Sub Campaigns
                ViewBag.lstsubcampaign = db.subcampaigns
                    .Where(x => x.subcampaign_delflag == null)
                    .OrderBy(x => x.subcampaign_name)
                    .ToList();

                // ✅ Landing Pages
                ViewBag.lstlandingpage = db.landingpages
                    .Where(x => x.landingpage_delflag == null)
                    .ToList();

                // ✅ Agar Publish ke baad clear karna hai, to ek flag check karenge
                if (TempData["PublishSuccess"] != null && (bool)TempData["PublishSuccess"] == true)
                {
                    ViewBag.lstchannels = new List<SelectListItem>();
                    ViewBag.lstcampaign = new List<SelectListItem>();
                    ViewBag.lstsubcampaign = new List<SelectListItem>();
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Something went wrong: " + ex.Message;
                //return View();
                return View(new campaign());

            }
        }


        [HttpPost]
        public ActionResult AddCampaign(CustomModel.ModelNewCampaign data, string instagram, string facebook, string twitter, string whatsapp, string linkedin, string input_name, string input_mobile, string input_email)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Initialize dropdown data
            ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
            ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
            ViewBag.lstcampaign = db.campaigns.Where(a => a.campaign_delflag == null)
                .Select(item => new SelectListItem
                {
                    Text = item.campaign_name,
                    Value = item.campaign_id.ToString()
                }).ToList();
            ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();
            ViewBag.lstlandingpage = db.landingpages.Where(x => x.landingpage_delflag == null).ToList();

            var supportedTypes = new[] { ".jpg", ".png", ".jpeg" };

            // Subcampaign: Filter by submitted campaign
            var subcampaigns = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();
            if (data.cm.campaign_id > 0)
            {
                subcampaigns = subcampaigns.Where(x => x.campaign_id == data.cm.campaign_id).ToList();
            }
            ViewBag.lstsubcampaign = subcampaigns;

            // Image validation
            if (data.images == null || data.images[0] == null)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Image is mandatory" });
                }
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "Image is mandatory";
                return View("Newcampaign");
            }

            foreach (HttpPostedFileBase file in data.images)
            {
                decimal size = Math.Round((decimal)file.ContentLength / 1024, 2);
                if (!supportedTypes.Contains(Path.GetExtension(file.FileName).ToLower()))
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Only jpeg, png, or jpg files are allowed." });
                    }
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "Only jpeg, png, or jpg files are allowed.";
                    return View("Newcampaign");
                }
                if (size > 500)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "All files must not exceed 500 KB." });
                    }
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All files must not exceed 500 KB.";
                    return View("Newcampaign");
                }
            }

            // Required field validation
            // NEW: Add Social Media Share and Tags validation
            bool isAnySocialMediaSelected = (instagram == "true" || facebook == "true" || twitter == "true" || whatsapp == "true" || linkedin == "true");
            if (data.SelectedChannels == null || !data.SelectedChannels.Any() ||
                data.cm.campaign_category_id == 0 || data.cm.campaign_id == 0 || data.cm.subcampaign_id == 0 ||
                string.IsNullOrEmpty(data.cm.campaign_master_type) || string.IsNullOrEmpty(data.cm.campaign_master_lang) ||
                string.IsNullOrEmpty(data.cm.campaign_master_description) || string.IsNullOrEmpty(data.cm.campaign_master_creative_caption) ||
                string.IsNullOrEmpty(data.cm.campaign_master_tags) || string.IsNullOrEmpty(data.cm.campaign_master_landing_page) ||
                data.cm.campaign_master_start_date == null || data.cm.campaign_master_end_date == null ||
                !isAnySocialMediaSelected)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "All fields are mandatory, including at least one social media platform and tags." });
                }
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = "All fields are mandatory, including at least one social media platform and tags.";
                return View("Newcampaign");
            }

            if (ModelState.IsValid)
            {
                // Save images
                string imagefiles = string.Empty;
                int i = 1;
                foreach (HttpPostedFileBase file in data.images)
                {
                    string filename = Path.GetFileName(file.FileName);
                    string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                    string path = Path.Combine(Server.MapPath("~/Content/images/campaign"), _filename);
                    file.SaveAs(path);

                    imagefiles = i == 1 ? _filename : imagefiles + "," + _filename;
                    i++;
                }

                // Loop over selected channels
                foreach (var channelId in data.SelectedChannels)
                {
                    var channel = db.channels.FirstOrDefault(x => x.channel_id == channelId && x.channel_delflag == null);
                    if (channel == null)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = false, message = $"Channel with ID {channelId} not found." });
                        }
                        TempData["AlertType"] = "danger";
                        TempData["AlertMessage"] = $"Channel with ID {channelId} not found.";
                        return View("Newcampaign");
                    }

                    // Get campaign name
                    var selectedCampaign = db.campaigns.FirstOrDefault(c => c.campaign_id == data.cm.campaign_id);
                    string campaignName = selectedCampaign != null ? selectedCampaign.campaign_name : "";

                    campaign_master cm = new campaign_master
                    {
                        channel_id = channelId,
                        ChannelName = channel.channel_name,
                        channel_code = channel.subchannel_name,
                        campaign_category_id = data.cm.campaign_category_id,
                        campaign_id = data.cm.campaign_id,
                        CampaignName = campaignName,
                        subcampaign_id = data.cm.subcampaign_id,
                        campaign_master_type = data.cm.campaign_master_type,
                        campaign_master_lang = data.cm.campaign_master_lang,
                        campaign_master_description = data.cm.campaign_master_description,
                        campaign_master_creative_caption = data.cm.campaign_master_creative_caption,
                        campaign_master_tags = data.cm.campaign_master_tags,
                        campaign_master_landing_page = data.cm.campaign_master_landing_page,
                        campaign_master_start_date = data.cm.campaign_master_start_date,
                        campaign_master_end_date = data.cm.campaign_master_end_date,
                        campaign_master_facebook = facebook == "true",
                        campaign_master_whatsapp = whatsapp == "true",
                        campaign_master_twitter = twitter == "true",
                        campaign_master_instagram = instagram == "true",
                        campaign_master_linkedin = linkedin == "true",
                        input_name = input_name == "true",
                        input_mobile = input_mobile == "true",
                        input_email = input_email == "true",
                        camapaign_master_status = "Active",
                        campaign_master_images = imagefiles,
                        user_id = Convert.ToDecimal(Session["userid"]),
                        campaign_master_date = DateTime.UtcNow.AddMinutes(330),
                        CentralBlast = Request["CentralBlast"]
                    };

                    db.campaign_master.Add(cm);
                }

                try
                {
                    //    if (db.SaveChanges() > 0)
                    //    {
                    //        // ---- Get logged-in user channel & SAPCODE ----
                    //        string userChannel = Convert.ToString(Session["X_CHANNEL"]);
                    //        string userSAP = Convert.ToString(Session["SAPCODE"]);

                    //        // ---- Check if this campaign belongs to user's channel ----
                    //        bool isForThisUser = data.SelectedChannels
                    //                              .Any(ch => db.channels
                    //                                           .Where(x => x.channel_id == ch)
                    //                                           .Select(x => x.channel_name)
                    //                                           .FirstOrDefault() == userChannel);

                    //        // ---- You can also match SAPCODE if needed ----
                    //        // bool isSAPMatch = (campaignMaster.sapcode == userSAP);

                    //        // ---- Now decide message ----
                    //        string finalMessage = isForThisUser
                    //                                ? "Your Channel has received a new campaign."
                    //                                : "New Campaign created successfully and published.";

                    //        Session["NotificationCount"] = (Session["NotificationCount"] == null)
                    //                                        ? 1
                    //                                        : (int)Session["NotificationCount"] + 1;

                    //        if (Request.IsAjaxRequest())
                    //        {
                    //            return Json(new { success = true, message = finalMessage });
                    //        }
                    //        else
                    //        {
                    //            TempData["AlertType"] = "success";
                    //            TempData["AlertMessage"] = finalMessage;
                    //            return RedirectToAction("Dashboard", "Marketing");
                    //        }
                    //    }
                    //}
                    //================================================================
                    if (db.SaveChanges() > 0)
                    {
                        Session["NotificationCount"] = (Session["NotificationCount"] == null) ? 1 : (int)Session["NotificationCount"] + 1;
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = true, message = "New Campaign created successfully and published." });
                        }
                        else
                        {
                            TempData["AlertType"] = "success";
                            TempData["AlertMessage"] = "New Campaign successfully created and published.";
                            return RedirectToAction("Dashboard", "Marketing");
                        }
                    }

                    else
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = false, message = "Error: No changes were saved to the database." });
                        }
                        TempData["AlertType"] = "danger";
                        TempData["AlertMessage"] = "Error: No changes were saved to the database.";
                        return View("Newcampaign");
                    }
                }



                catch (Exception ex)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = $"Error: {ex.Message}" });
                    }
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = $"Error: {ex.Message}";
                    return View("Newcampaign");
                }
            }

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = false, message = "Error: Validation failed." });
            }
            TempData["AlertType"] = "danger";
            TempData["AlertMessage"] = "Error: Validation failed.";
            return View("Newcampaign");
        }


        //   [HttpPost]
        //   public ActionResult AddCampaign(
        //CustomModel.ModelNewCampaign data,
        //string instagram,
        //string facebook,
        //string twitter,
        //string whatsapp,
        //string linkedin,
        //string input_name,
        //string input_mobile,
        //string input_email)
        //   {
        //       if (Session["userid"] == null)
        //       {
        //           return RedirectToAction("Index");
        //       }

        //       try
        //       {
        //           // =========================
        //           // 1️⃣ VALIDATION
        //           // =========================
        //           if (!ModelState.IsValid)
        //           {
        //               return View("Newcampaign");
        //           }

        //           if (data.images == null || data.images[0] == null)
        //           {
        //               return View("Newcampaign");
        //           }

        //           var supportedTypes = new[] { ".jpg", ".png", ".jpeg" };
        //           foreach (var file in data.images)
        //           {
        //               if (!supportedTypes.Contains(Path.GetExtension(file.FileName).ToLower()))
        //                   return View("Newcampaign");
        //           }

        //           // =========================
        //           // 2️⃣ SAVE IMAGES
        //           // =========================
        //           string imagefiles = "";
        //           int i = 1;

        //           foreach (var file in data.images)
        //           {
        //               string filename = DateTime.Now.ToString("yymmssfff") + Path.GetFileName(file.FileName);
        //               file.SaveAs(Server.MapPath("~/Content/images/campaign/" + filename));
        //               imagefiles = (i == 1) ? filename : imagefiles + "," + filename;
        //               i++;
        //           }

        //           // =========================
        //           // 3️⃣ SAVE CAMPAIGN MASTER
        //           // =========================
        //           foreach (var channelId in data.SelectedChannels)
        //           {
        //               var channel = db.channels.FirstOrDefault(x => x.channel_id == channelId);
        //               if (channel == null) continue;

        //               var selectedCampaign = db.campaigns
        //                   .FirstOrDefault(c => c.campaign_id == data.cm.campaign_id);

        //               db.campaign_master.Add(new campaign_master
        //               {
        //                   channel_id = channelId,
        //                   ChannelName = channel.channel_name,

        //                   // 🔥 IMPORTANT: Channel_code must match X_CHANNEL
        //                   channel_code = channel.subchannel_name, // AG / DB

        //                   campaign_id = data.cm.campaign_id,
        //                   CampaignName = selectedCampaign?.campaign_name,
        //                   campaign_category_id = data.cm.campaign_category_id,
        //                   subcampaign_id = data.cm.subcampaign_id,
        //                   campaign_master_type = data.cm.campaign_master_type,
        //                   campaign_master_lang = data.cm.campaign_master_lang,
        //                   campaign_master_description = data.cm.campaign_master_description,
        //                   campaign_master_creative_caption = data.cm.campaign_master_creative_caption,
        //                   campaign_master_tags = data.cm.campaign_master_tags,
        //                   campaign_master_landing_page = data.cm.campaign_master_landing_page,
        //                   campaign_master_start_date = data.cm.campaign_master_start_date,
        //                   campaign_master_end_date = data.cm.campaign_master_end_date,
        //                   campaign_master_images = imagefiles,
        //                   camapaign_master_status = "Active",
        //                   user_id = Convert.ToDecimal(Session["userid"]),
        //                   campaign_master_date = DateTime.UtcNow.AddMinutes(330)
        //               });
        //           }

        //           // =========================
        //           // 4️⃣ SAVE + NOTIFICATIONS
        //           // =========================
        //           if (db.SaveChanges() > 0)
        //           {
        //               // Last inserted campaigns
        //               var latestCampaigns = db.campaign_master
        //                   .OrderByDescending(x => x.campaign_master_id)
        //                   .Take(data.SelectedChannels.Count)
        //                   .ToList();

        //               foreach (var campaign in latestCampaigns)
        //               {
        //                   // 🔔 Sirf us channel ke users
        //                   var matchedUsers = db.NEW_TEMP_HIERARCHY
        //                       .Where(u => u.X_CHANNEL == campaign.channel_code && u.AGENT_CODE != null)
        //                       .Select(u => u.AGENT_CODE)
        //                       .Distinct()
        //                       .ToList();

        //                   foreach (var sap in matchedUsers)
        //                   {
        //                       // ❌ Duplicate avoid
        //                       bool exists = db.UserNotifications.Any(n =>
        //                           n.SAPCode == sap &&
        //                           n.Channel_code == campaign.channel_code &&
        //                           n.Message.Contains(campaign.CampaignName)
        //                       );

        //                       if (!exists)
        //                       {
        //                           db.UserNotifications.Add(new UserNotification
        //                           {
        //                               SAPCode = sap,
        //                               Channel_code = campaign.channel_code,
        //                               Message = "New Campaign Created: " + campaign.CampaignName,
        //                               IsRead = false,
        //                               CreatedDate = DateTime.UtcNow.AddMinutes(330)
        //                           });
        //                       }
        //                   }
        //               }

        //               db.SaveChanges();

        //               return RedirectToAction("Dashboard", "Marketing");
        //           }

        //           return View("Newcampaign");
        //       }
        //       catch (Exception ex)
        //       {
        //           TempData["AlertType"] = "danger";
        //           TempData["AlertMessage"] = ex.Message;
        //           return View("Newcampaign");
        //       }
        //   }



        [HttpPost]
        public ActionResult ResetNotification()
        {
            string sap = Convert.ToString(Session["SAPCODE"]);

            var notifs = db.UserNotifications
                .Where(x => x.SAPCode == sap && x.IsRead == false)
                .ToList();

            foreach (var n in notifs)
            {
                n.IsRead = true;
            }

            db.SaveChanges();

            Session["NotificationCount"] = 0;
            return Json(new { success = true });
        }


        //[HttpPost]
        //public ActionResult ResetNotification()
        //{
        //    Session["NotificationCount"] = 0;
        //    return Json(new { success = true });
        //}

        //end  here



        /***********End : new campaign*************/

        //================EditcampaignCentralBlast============

        public ActionResult EditcampaignCentralBlast(decimal id)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
                ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).OrderBy(x => x.campaign_category_name).ToList();

                var campaign = (from a in db.campaigns
                                where a.campaign_delflag == null
                                select a).ToList();
                List<SelectListItem> camp = new List<SelectListItem>();
                if (campaign != null)
                {
                    foreach (var item in campaign)
                    {
                        camp.Add(new SelectListItem { Text = item.campaign_name, Value = item.campaign_id.ToString() });
                    }
                }

                ViewBag.lstcampaign = camp;
                ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();
                ViewBag.lstlandingpage = db.landingpages.Where(x => x.landingpage_delflag == null).ToList();

                List<SelectListItem> status = new List<SelectListItem>()
        {
            new SelectListItem{Text="Active", Value="Active"},
            new SelectListItem{Text="Paused", Value="Paused"},
            new SelectListItem{Text="Upcoming", Value="Upcoming"}
        };

                ViewBag.lstStatus = status;

                // CentralBlast के साथ data load करें
                var campaignMaster = db.campaign_master.FirstOrDefault(x => x.campaign_master_id == id);

                if (campaignMaster == null)
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "Campaign not found.";
                    return RedirectToAction("CentralBlast");
                }

                // अगर CentralBlast null है तो default value set करें
                if (string.IsNullOrEmpty(campaignMaster.CentralBlast))
                {
                    campaignMaster.CentralBlast = "No";
                }

                CustomModel.ModelNewCampaign cam = new CustomModel.ModelNewCampaign
                {
                    cm = campaignMaster
                };

                return View(cam);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditcampaignCentralBlast(decimal id, CustomModel.ModelNewCampaign data,
string instagram, string facebook, string twitter, string whatsapp, string linkedin,
string input_name, string input_mobile, string input_email)
        {
            if (Session["userid"] == null)
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }

            try
            {
                // ViewBag data load करें (same as before)
                ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
                ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).OrderBy(x => x.campaign_category_name).ToList();

                var campaign = (from a in db.campaigns where a.campaign_delflag == null select a).ToList();
                List<SelectListItem> camp = new List<SelectListItem>();
                if (campaign != null)
                {
                    foreach (var item in campaign)
                    {
                        camp.Add(new SelectListItem { Text = item.campaign_name, Value = item.campaign_id.ToString() });
                    }
                }

                ViewBag.lstcampaign = camp;
                ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();
                ViewBag.lstlandingpage = db.landingpages.Where(x => x.landingpage_delflag == null).ToList();

                List<SelectListItem> status = new List<SelectListItem>()
        {
            new SelectListItem{Text="Active", Value="Active"},
            new SelectListItem{Text="Paused", Value="Paused"},
            new SelectListItem{Text="Upcoming", Value="Upcoming"}
        };
                ViewBag.lstStatus = status;

                // Existing record को database से लें
                var existingCampaign = db.campaign_master.FirstOrDefault(x => x.campaign_master_id == id);
                if (existingCampaign == null)
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "Campaign not found.";
                    return RedirectToAction("CentralBlast");
                }

                // Image validation (same as before)
                if (data.images != null && data.images[0] != null)
                {
                    var supportedTypes = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                    decimal size;

                    foreach (HttpPostedFileBase file in data.images)
                    {
                        size = Math.Round(((decimal)(file).ContentLength / (decimal)1024), 2);
                        if (!supportedTypes.Contains(Path.GetExtension(file.FileName).ToLower()))
                        {
                            TempData["AlertType"] = "danger";
                            TempData["AlertMessage"] = "Only jpg, jpeg or png should be selected";
                            return View(new CustomModel.ModelNewCampaign { cm = existingCampaign });
                        }
                        else if (size > 500)
                        {
                            TempData["AlertType"] = "danger";
                            TempData["AlertMessage"] = "All File must not exceed 500 KB";
                            return View(new CustomModel.ModelNewCampaign { cm = existingCampaign });
                        }
                    }
                }

                // सभी validations (same as before)
                bool isValid = true;

                if (string.IsNullOrEmpty(data.cm.channel_id.ToString())) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_category_id.ToString())) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_id.ToString())) isValid = false;
                if (string.IsNullOrEmpty(data.cm.subcampaign_id.ToString())) isValid = false;

                if (string.IsNullOrEmpty(data.cm.CentralBlast))
                {
                    isValid = false;
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "CentralBlast is mandatory";
                    return View(new CustomModel.ModelNewCampaign { cm = existingCampaign });
                }

                if (string.IsNullOrEmpty(data.cm.campaign_master_type)) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_master_lang)) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_master_description)) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_master_creative_caption)) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_master_tags)) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_master_landing_page)) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_master_start_date.ToString())) isValid = false;
                if (string.IsNullOrEmpty(data.cm.campaign_master_end_date.ToString())) isValid = false;
                if (string.IsNullOrEmpty(data.cm.camapaign_master_status)) isValid = false;

                if (!isValid)
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(new CustomModel.ModelNewCampaign { cm = existingCampaign });
                }

                // Image upload processing (same as before)
                int i = 1;
                string imagefiles = string.Empty;
                if (data.images != null && data.images[0] != null)
                {
                    foreach (HttpPostedFileBase file in data.images)
                    {
                        try
                        {
                            string filename = Path.GetFileName(file.FileName);
                            string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                            string path = Path.Combine(Server.MapPath("~/Content/images/campaign/"), _filename);
                            file.SaveAs(path);
                            if (i == 1)
                            {
                                imagefiles = _filename;
                            }
                            else
                            {
                                imagefiles = imagefiles + "," + _filename;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Image upload error: {ex.Message}");
                        }
                        i++;
                    }
                    if (!string.IsNullOrEmpty(imagefiles))
                    {
                        existingCampaign.campaign_master_images = imagefiles;
                    }
                }

                // Checkbox processing (same as before)
                existingCampaign.campaign_master_facebook = !string.IsNullOrEmpty(facebook) && (facebook == "true" || facebook == "true,true");
                existingCampaign.campaign_master_whatsapp = !string.IsNullOrEmpty(whatsapp) && (whatsapp == "true" || whatsapp == "true,true");
                existingCampaign.campaign_master_twitter = !string.IsNullOrEmpty(twitter) && (twitter == "true" || twitter == "true,true");
                existingCampaign.campaign_master_instagram = !string.IsNullOrEmpty(instagram) && (instagram == "true" || instagram == "true,true");
                existingCampaign.campaign_master_linkedin = !string.IsNullOrEmpty(linkedin) && (linkedin == "true" || linkedin == "true,true");

                existingCampaign.input_name = !string.IsNullOrEmpty(input_name) && (input_name == "true" || input_name == "true,true");
                existingCampaign.input_mobile = !string.IsNullOrEmpty(input_mobile) && (input_mobile == "true" || input_mobile == "true,true");
                existingCampaign.input_email = !string.IsNullOrEmpty(input_email) && (input_email == "true" || input_email == "true,true");

                // सभी fields को update करें
                existingCampaign.channel_id = data.cm.channel_id;
                existingCampaign.campaign_category_id = data.cm.campaign_category_id;
                existingCampaign.campaign_id = data.cm.campaign_id;
                existingCampaign.subcampaign_id = data.cm.subcampaign_id;

                // CENTRALBLAST UPDATE
                existingCampaign.CentralBlast = data.cm.CentralBlast;

                existingCampaign.campaign_master_type = data.cm.campaign_master_type;
                existingCampaign.campaign_master_lang = data.cm.campaign_master_lang;
                existingCampaign.campaign_master_description = data.cm.campaign_master_description;
                existingCampaign.campaign_master_creative_caption = data.cm.campaign_master_creative_caption;
                existingCampaign.campaign_master_tags = data.cm.campaign_master_tags;
                existingCampaign.campaign_master_landing_page = data.cm.campaign_master_landing_page;
                existingCampaign.campaign_master_start_date = data.cm.campaign_master_start_date;
                existingCampaign.campaign_master_end_date = data.cm.campaign_master_end_date;
                existingCampaign.camapaign_master_status = data.cm.camapaign_master_status;

                // Save changes
                int result = db.SaveChanges();

                if (result > 0)
                {
                    // Verify save in database
                    var savedCampaign = db.campaign_master.FirstOrDefault(x => x.campaign_master_id == id);

                    // SweetAlert के लिए JavaScript message return करें
                    //TempData["AlertType"] = "success";
                    //TempData["AlertMessage"] = $"Campaign updated successfully! CentralBlast: {data.cm.CentralBlast}";

                    // Success के लिए JavaScript code add करें
                    ViewBag.SuccessMessage = $"Campaign updated successfully! CentralBlast set to: {data.cm.CentralBlast}";
                    ViewBag.IsSuccess = true;

                    return RedirectToAction("CentralBlast");
                }
                else
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "Error!! Record not updated. No changes detected.";
                    return View(new CustomModel.ModelNewCampaign { cm = existingCampaign });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = $"Error: {ex.Message}";
                return View(new CustomModel.ModelNewCampaign { cm = db.campaign_master.FirstOrDefault(x => x.campaign_master_id == id) });
            }
        }

        /******** START - EDIT CAMPAIGN ***********/
        public ActionResult Editcampaign(decimal id)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
                ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null && x.Campaign_Category_Status == "0").OrderBy(x => x.campaign_category_name).ToList();
                var campaign = (from a in db.campaigns
                                where a.campaign_delflag == null
                                select a).ToList();
                List<SelectListItem> camp = new List<SelectListItem>();
                if (campaign != null)
                {
                    foreach (var item in campaign)
                    {
                        camp.Add(new SelectListItem { Text = item.campaign_name, Value = item.campaign_id.ToString() });
                    }
                }

                ViewBag.lstcampaign = camp;
                ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();
                ViewBag.lstlandingpage = db.landingpages.Where(x => x.landingpage_delflag == null).ToList();


                List<SelectListItem> status = new List<SelectListItem>()
                {
                    new SelectListItem{Text="Active", Value="Active"},
                    new SelectListItem{Text="Paused", Value="Paused"},
                    new SelectListItem{Text="Upcoming", Value="Upcoming"}
                };

                ViewBag.lstStatus = status;


                CustomModel.ModelNewCampaign cam = (from a in db.campaign_master
                                                    where a.campaign_master_id == id
                                                    select new CustomModel.ModelNewCampaign
                                                    {
                                                        cm = a
                                                    }).FirstOrDefault();


                return View(cam);
            }

        }


        [HttpPost]
        public ActionResult Editcampaign(decimal id, CustomModel.ModelNewCampaign data, string instagram, string facebook, string twitter, string whatsapp, string linkedin, string input_name, string input_mobile, string input_email)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                List<SelectListItem> status = new List<SelectListItem>()
                {
                    new SelectListItem{Text="Active", Value="Active"},
                    new SelectListItem{Text="Pause", Value="Pause"}
                };

                ViewBag.lstStatus = status;
                ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
                ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null && x.Campaign_Category_Status == "0").OrderBy(x => x.campaign_category_name).ToList();
                var campaign = (from a in db.campaigns
                                where a.campaign_delflag == null
                                select a).ToList();
                List<SelectListItem> camp = new List<SelectListItem>();
                if (campaign != null)
                {
                    foreach (var item in campaign)
                    {
                        camp.Add(new SelectListItem { Text = item.campaign_name, Value = item.campaign_id.ToString() });
                    }
                }

                ViewBag.lstcampaign = camp;
                ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();
                ViewBag.lstlandingpage = db.landingpages.Where(x => x.landingpage_delflag == null).ToList();


                CustomModel.ModelNewCampaign cam = (from a in db.campaign_master
                                                    where a.campaign_master_id == id
                                                    select new CustomModel.ModelNewCampaign
                                                    {
                                                        cm = a
                                                    }).FirstOrDefault();



                if (data.images[0] != null)
                {
                    var supportedTypes = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                    decimal size;

                    if (data.images != null)
                    {
                        foreach (HttpPostedFileBase file in data.images)
                        {
                            size = Math.Round(((decimal)(file).ContentLength / (decimal)1024), 2);
                            if (!supportedTypes.Contains(Path.GetExtension(file.FileName).ToLower()))
                            {
                                TempData["AlertType"] = "danger";
                                TempData["AlertMessage"] = "Only jpg, jpeg or png should be selected";
                                ModelState.AddModelError("image", "Only jpg, jpeg or png should be selected. ");
                            }
                            else if (size > 500)
                            {
                                TempData["AlertType"] = "danger";
                                TempData["AlertMessage"] = "All File must not exceed 500 KB";
                                ModelState.AddModelError("image", "All File must not exceed 500 KB.");
                            }
                        }

                    }
                    else
                    {
                        TempData["AlertType"] = "danger";
                        TempData["AlertMessage"] = "All Fields are mandatory";
                        ModelState.AddModelError("images", "Required");
                    }

                }


                if (string.IsNullOrEmpty(data.cm.channel_id.ToString()))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }

                if (string.IsNullOrEmpty(data.cm.campaign_category_id.ToString()))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }

                if (string.IsNullOrEmpty(data.cm.campaign_id.ToString()))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.subcampaign_id.ToString()))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.campaign_master_type))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.campaign_master_lang))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.campaign_master_description))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }

                if (string.IsNullOrEmpty(data.cm.campaign_master_creative_caption))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.campaign_master_tags))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.campaign_master_landing_page))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.campaign_master_start_date.ToString()))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.campaign_master_end_date.ToString()))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }
                if (string.IsNullOrEmpty(data.cm.camapaign_master_status))
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "All Fields are mandatory";
                    return View(cam);
                }

                if (ModelState.IsValid)
                {
                    int i = 1;
                    string imagefiles = string.Empty;
                    if (data.images[0] != null)
                    {
                        foreach (HttpPostedFileBase file in data.images)
                        {
                            try
                            {
                                string filename = Path.GetFileName(file.FileName);
                                string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                                string path = Path.Combine(Server.MapPath("~/Content/images/campaign/"), _filename);
                                file.SaveAs(path);
                                if (i == 1)
                                {
                                    imagefiles = _filename;
                                }
                                else
                                {
                                    imagefiles = imagefiles + "," + _filename;
                                }

                            }
                            catch (Exception ex)
                            {

                            }
                            i++;
                        }

                        cam.cm.campaign_master_images = imagefiles;
                    }

                    if (facebook == "true")
                    {
                        cam.cm.campaign_master_facebook = true;
                    }
                    else
                    {
                        cam.cm.campaign_master_facebook = false;
                    }

                    if (whatsapp == "true")
                    {
                        cam.cm.campaign_master_whatsapp = true;
                    }
                    else
                    {
                        cam.cm.campaign_master_whatsapp = false;
                    }

                    if (twitter == "true")
                    {
                        cam.cm.campaign_master_twitter = true;
                    }
                    else
                    {
                        cam.cm.campaign_master_twitter = false;
                    }

                    if (instagram == "true")
                    {
                        cam.cm.campaign_master_instagram = true;
                    }
                    else
                    {
                        cam.cm.campaign_master_instagram = false;
                    }
                    if (linkedin == "true")
                    {
                        cam.cm.campaign_master_linkedin = true;
                    }
                    else
                    {
                        cam.cm.campaign_master_linkedin = false;
                    }
                    if (input_name == "true")
                    {
                        data.cm.input_name = true;
                    }
                    else
                    {
                        data.cm.input_name = false;
                    }

                    if (input_mobile == "true")
                    {
                        data.cm.input_mobile = true;
                    }
                    else
                    {
                        data.cm.input_mobile = false;
                    }

                    if (input_email == "true")
                    {
                        data.cm.input_email = true;
                    }
                    else
                    {
                        data.cm.input_email = false;
                    }
                    cam.cm.channel_id = data.cm.channel_id;
                    cam.cm.campaign_category_id = data.cm.campaign_category_id;
                    cam.cm.campaign_id = data.cm.campaign_id;
                    cam.cm.subcampaign_id = data.cm.subcampaign_id;
                    cam.cm.campaign_master_type = data.cm.campaign_master_type;
                    cam.cm.campaign_master_lang = data.cm.campaign_master_lang;
                    cam.cm.campaign_master_description = data.cm.campaign_master_description;
                    cam.cm.campaign_master_creative_caption = data.cm.campaign_master_creative_caption;
                    cam.cm.campaign_master_tags = data.cm.campaign_master_tags;
                    cam.cm.campaign_master_landing_page = data.cm.campaign_master_landing_page;
                    cam.cm.campaign_master_start_date = data.cm.campaign_master_start_date;
                    cam.cm.campaign_master_end_date = data.cm.campaign_master_end_date;
                    cam.cm.camapaign_master_status = data.cm.camapaign_master_status;


                    if (db.SaveChanges() > 0)
                    {
                        return RedirectToAction("campaign", new { id = id });

                        //return RedirectToAction("campaign");
                        //return RedirectToAction("Editcampaign", new { id = campaignId });

                    }
                    else
                    {
                        TempData["AlertType"] = "danger";
                        TempData["AlertMessage"] = "Error!! Record not updated.";

                        return View(cam);
                    }

                }
                else
                {
                    TempData["AlertType"] = "danger";
                    TempData["AlertMessage"] = "Error!! Record not updated.";

                    return View(cam);

                }


            }

        }


        /*************END - EDIT CAMPAIGN *************/
        //    public ActionResult campaignview()
        //    {
        //        SetData();

        //        List<SelectListItem> status = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Start", Value="Start"},
        //    new SelectListItem{Text="Pause", Value="Pause"}
        //};
        //        ViewBag.lstStatus = status;

        //        ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();

        //        // 🔹 Pehle category list lao
        //        var categories = db.campaign_category
        //                           .Where(x => x.campaign_category_delflag == null)
        //                           .ToList();

        //        // 🔹 Check karo ki category ke andar campaign_master ya campaign linked hai ya nahi
        //        foreach (var cat in categories)
        //        {
        //            bool hasData = db.campaign_master.Any(c => c.campaign_category_id == cat.campaign_category_id);

        //            if (!hasData)
        //            {
        //                // Agar data nahi hai to status = 1 set kardo
        //                cat.Campaign_Category_Status = "1";
        //            }
        //            else
        //            {
        //                // Agar data hai to status = 0 set kardo
        //                cat.Campaign_Category_Status = "0";
        //            }
        //        }

        //        db.SaveChanges();

        //        // 🔹 Ab sirf wahi categories dikhao jinki status = 0 hai
        //        ViewBag.lstcategory = categories
        //                              .Where(x => x.Campaign_Category_Status == "0")
        //                              .ToList();

        //        ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
        //        ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

        //        List<SelectListItem> language = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="English", Value="English"},
        //    new SelectListItem{Text="Hindi", Value="Hindi"},
        //    new SelectListItem{Text="Marathi", Value="Marathi"},
        //    new SelectListItem{Text="Tamil", Value="Tamil"}
        //};
        //        ViewBag.lstlanguage = language;

        //        List<SelectListItem> type = new List<SelectListItem>()
        //{
        //    new SelectListItem{Text="Creative", Value="Creative"},
        //    new SelectListItem{Text="Video", Value="Video"},
        //    new SelectListItem{Text="Blogs", Value="Blogs"}
        //};
        //        ViewBag.lsttype = type;

        //        ViewBag.channel = "";
        //        ViewBag.category = "";
        //        ViewBag.campaign = "";
        //        ViewBag.subcampaign = "";
        //        ViewBag.language = "";
        //        ViewBag.type = "";

        //        DateTime dt = System.DateTime.UtcNow.AddMinutes(330);

        //        var qry1 = db.campaign_master
        //                     .Where(x => x.campaign_master_start_date > dt ||
        //                                 (x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt))
        //                     .OrderByDescending(x => x.campaign_master_id)
        //                     .ToList();

        //        ViewBag.lstAllCampaigns = qry1;

        //        return View();
        //    }



        //    [HttpPost]
        //    public ActionResult campaignview(FormCollection form)
        //    {
        //        //Session["SAPCODE"] = SAPCODE;
        //        //Session["TYPE"] = TYPE;
        //        //Session["LOGIN"] = LOGIN;

        //        SetData();

        //        List<SelectListItem> status = new List<SelectListItem>()
        //            {
        //                new SelectListItem{Text="Start", Value="Start"},
        //                new SelectListItem{Text="Pause", Value="Pause"}
        //            };

        //        ViewBag.lstStatus = status;
        //        List<SelectListItem> language = new List<SelectListItem>()
        //            {
        //                new SelectListItem{Text="English", Value="English"},
        //                new SelectListItem{Text="Hindi", Value="Hindi"},
        //                new SelectListItem{Text="Marathi", Value="Marathi"},
        //                new SelectListItem{Text="Tamil", Value="Tamil"}
        //            };

        //        ViewBag.lstlanguage = language;


        //        List<SelectListItem> type = new List<SelectListItem>()
        //            {
        //                new SelectListItem{Text="Creative", Value="Creative"},
        //                                        new SelectListItem{Text="Video", Value="Video"},
        //                                         new SelectListItem{Text="Blogs", Value="Blogs"}
        //            };

        //        ViewBag.lsttype = type;

        //        ViewBag.channel = form["channel"] ?? string.Empty;
        //        ViewBag.category = form["category"] ?? string.Empty;
        //        ViewBag.campaign = form["campaign"] ?? string.Empty;
        //        ViewBag.subcampaign = form["subcampaign"] ?? string.Empty;
        //        ViewBag.language = form["language"] ?? string.Empty;
        //        ViewBag.type = form["campaigntype"] ?? string.Empty;
        //        ViewBag.status = form["campaignstatus"] ?? string.Empty;


        //        var predicate = PredicateBuilder.True<campaign_master>();
        //        if (!string.IsNullOrEmpty(form["channel"]))
        //        {
        //            decimal channel = Convert.ToDecimal(form["channel"]);
        //            predicate = predicate.And(i => i.channel_id == channel);
        //        }

        //        if (!string.IsNullOrEmpty(form["category"]))
        //        {
        //            decimal category = Convert.ToDecimal(form["category"]);
        //            predicate = predicate.And(i => i.campaign_category_id == category);
        //        }

        //        if (!string.IsNullOrEmpty(form["campaign"]))
        //        {
        //            decimal campaign = Convert.ToDecimal(form["campaign"]);
        //            predicate = predicate.And(i => i.campaign_id == campaign);
        //        }

        //        if (!string.IsNullOrEmpty(form["subcampaign"]))
        //        {
        //            decimal subcampaign = Convert.ToDecimal(form["subcampaign"]);
        //            predicate = predicate.And(i => i.subcampaign_id == subcampaign);
        //        }

        //        if (!string.IsNullOrEmpty(form["language"]))
        //        {
        //            //decimal category = Convert.ToDecimal(form["la"]);
        //            string lang = form["language"];
        //            predicate = predicate.And(i => i.campaign_master_lang == lang);
        //        }

        //        if (!string.IsNullOrEmpty(form["campaigntype"]))
        //        {
        //            string ctype = form["campaigntype"];
        //            predicate = predicate.And(i => i.campaign_master_type == ctype);
        //        }
        //        if (!string.IsNullOrEmpty(form["campaignstatus"]))
        //        {
        //            string cstatus = form["campaignstatus"];
        //            predicate = predicate.And(i => i.camapaign_master_status == cstatus);
        //        }
        //        ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
        //        ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
        //        ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
        //        ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

        //        DateTime dt = System.DateTime.UtcNow.AddMinutes(330);
        //        predicate = predicate.And(i => i.camapaign_master_status == "Pause" || (i.campaign_master_start_date <= dt && i.campaign_master_end_date >= dt) || i.campaign_master_start_date > dt);

        //        var qry1 = db.campaign_master.Where(predicate).ToList();
        //        //var qry = (from a in db.campaign_master
        //        //                           join b in db.channels on a.channel_id equals b.channel_id
        //        //                           join c in db.campaign_category on a.campaign_category_id equals c.campaign_category_id
        //        //                           join d in db.campaigns on a.campaign_id equals d.campaign_id
        //        //                           join e in db.subcampaigns on a.subcampaign_id equals e.subcampaign_id

        //        //                           select new CustomModel.ViewModelCampaignCategory
        //        //                           {
        //        //                               cm = a,
        //        //                               ch = b,
        //        //                               cc = c,
        //        //                               cam = d,
        //        //                               sc = e
        //        //                           }).ToList();

        //        ViewBag.lstAllCampaigns = qry1;
        //        return View();


        //    }

        public ActionResult EditCategory(decimal CategoryId)
        {

            campaign_category model = new campaign_category();

            if (CategoryId > 0)
            {

                campaign_category cat = db.campaign_category.SingleOrDefault(x => x.campaign_category_id == CategoryId && x.campaign_category_delflag == null);
                model.campaign_category_id = cat.campaign_category_id;

                model.campaign_category_name = cat.campaign_category_name;

            }
            return PartialView("_PartialCategoryEdit", model);
        }


        [HttpPost]
        public ActionResult EditCategory(campaign_category data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                try
                {
                    if (data.campaign_category_id > 0)
                    {
                        //update
                        campaign_category cat = db.campaign_category.SingleOrDefault(x => x.campaign_category_id == data.campaign_category_id && x.campaign_category_delflag == null);

                        cat.campaign_category_name = data.campaign_category_name;
                        db.SaveChanges();


                    }
                    //return View("CampaignCategoryList");
                    return RedirectToAction("CampaignCategoryList");
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }


        // kamlesh edit here 
        [HttpPost]
        public ActionResult DeleteCategory(decimal CategoryId)
        {
            try
            {
                // Record fetch karo jiska delflag null ho
                var cat = db.campaign_category
                    .SingleOrDefault(x => x.campaign_category_id == CategoryId && x.campaign_category_delflag == null);

                if (cat != null)
                {
                    db.campaign_category.Remove(cat); // Permanently delete
                    db.SaveChanges();

                    return Json(new { success = true, message = "Category deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Category not found or already deleted." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred: " + ex.Message });
            }
        }
        // end here 


        public ActionResult EditCampaignMaster(decimal CampaignId)
        {
            ViewBag.lstCategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
            campaign model = new campaign();

            if (CampaignId > 0)
            {

                campaign cam = db.campaigns.SingleOrDefault(x => x.campaign_id == CampaignId && x.campaign_delflag == null);
                model.campaign_category_id = cam.campaign_category_id;
                model.campaign_id = cam.campaign_id;
                model.campaign_name = cam.campaign_name;

            }
            return PartialView("_PartialCampaignMasterEdit", model);
        }

        [HttpPost]
        public ActionResult EditCampaignMaster(campaign data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                try
                {
                    ViewBag.lstCategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
                    if (data.campaign_id > 0 && data.campaign_category_id > 0)
                    {
                        //update
                        campaign cat = db.campaigns.SingleOrDefault(x => x.campaign_id == data.campaign_id && x.campaign_delflag == null);
                        cat.campaign_category_id = data.campaign_category_id;
                        cat.campaign_name = data.campaign_name;
                        db.SaveChanges();


                    }
                    //return View(data);
                    return RedirectToAction("CampaignList");
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }









        public ActionResult EditSubCampaign(decimal SubCampaignId)
        {
            ViewBag.lstCampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
            subcampaign model = new subcampaign();

            if (SubCampaignId > 0)
            {

                subcampaign cam = db.subcampaigns.SingleOrDefault(x => x.subcampaign_id == SubCampaignId && x.subcampaign_delflag == null);
                model.subcampaign_id = cam.subcampaign_id;
                model.campaign_id = cam.campaign_id;
                model.subcampaign_name = cam.subcampaign_name;

            }
            return PartialView("_PartialSubCampaignEdit", model);
        }

        [HttpPost]
        public ActionResult EditSubCampaign(subcampaign data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                try
                {
                    ViewBag.lstCampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
                    if (data.subcampaign_id > 0 && data.campaign_id > 0)
                    {
                        //update
                        subcampaign cat = db.subcampaigns.SingleOrDefault(x => x.subcampaign_id == data.subcampaign_id && x.subcampaign_delflag == null);
                        cat.campaign_id = data.campaign_id;
                        cat.subcampaign_name = data.subcampaign_name;
                        db.SaveChanges();


                    }
                    //return View(data);
                    return RedirectToAction("SubCampaignList");
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }



        //=====================================
        //[HttpPost]
        //public JsonResult DeleteSubCampaign(int SubCampaignId)
        //{
        //    if (Session["userid"] == null)
        //    {
        //        return Json(new { success = false, message = "Session expired." });
        //    }

        //    try
        //    {
        //        // Find the subcampaign by ID
        //        var sub = db.subcampaigns.SingleOrDefault(x => x.subcampaign_id == SubCampaignId);

        //        if (sub != null)
        //        {
        //            db.subcampaigns.Remove(sub); // 🔥 Permanently delete record from [dbo].[subcampaign]
        //            db.SaveChanges();
        //            return Json(new { success = true });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "SubCampaign not found." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}
        //[HttpPost]
        //public JsonResult DeleteSubCampaign(int SubCampaignId)
        //{
        //    if (Session["userid"] == null)
        //    {
        //        return Json(new { success = false, message = "Session expired." });
        //    }

        //    try
        //    {
        //        // Find the subcampaign by ID
        //        var sub = db.subcampaigns.SingleOrDefault(x => x.subcampaign_id == SubCampaignId);

        //        if (sub == null)
        //        {
        //            return Json(new { success = false, message = "SubCampaign not found." });
        //        }

        //        // 🔥 Permanently delete the record
        //        db.subcampaigns.Remove(sub);
        //        db.SaveChanges();

        //        return Json(new { success = true, message = "Deleted successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Error details (FK issue, constraints etc.)
        //        return Json(new { success = false, message = "Error: " + ex.Message });
        //    }
        //}



        [HttpPost]
        public JsonResult DeleteSubCampaign(int SubCampaignId)
        {
            if (Session["userid"] == null)
            {
                return Json(new { success = false, message = "Session expired." });
            }

            try
            {
                var sub = db.subcampaigns.SingleOrDefault(x => x.subcampaign_id == SubCampaignId);

                if (sub == null)
                {
                    return Json(new { success = false, message = "SubCampaign not found." });
                }

                // Soft delete
                sub.subcampaign_delflag = "true";

                db.SaveChanges();

                return Json(new { success = true, message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        //=============================================




        public ActionResult upcomingcampaignsuser()
        {
            //Session["SAPCODE"] = SAPCODE;
            SetData();

            List<SelectListItem> status = new List<SelectListItem>()
                {
                    new SelectListItem{Text="Start", Value="Start"},
                    new SelectListItem{Text="Pause", Value="Pause"}
                };

            ViewBag.lstStatus = status;

            ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
            ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
            ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
            ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

            List<SelectListItem> language = new List<SelectListItem>()
                {
                    new SelectListItem{Text="English", Value="English"},
                    new SelectListItem{Text="Hindi", Value="Hindi"},
                    new SelectListItem{Text="Marathi", Value="Marathi"},
                    new SelectListItem{Text="Tamil", Value="Tamil"}
                };

            ViewBag.lstlanguage = language;


            List<SelectListItem> type = new List<SelectListItem>()
                {
                    new SelectListItem{Text="Creative", Value="Creative"},
                                            new SelectListItem{Text="Video", Value="Video"},
                                             new SelectListItem{Text="Blogs", Value="Blogs"}
                };

            ViewBag.lsttype = type;


            ViewBag.channel = "";
            ViewBag.category = "";
            ViewBag.campaign = "";
            ViewBag.subcampaign = "";
            ViewBag.language = "";
            ViewBag.type = "";

            DateTime curdate = System.DateTime.UtcNow.AddMinutes(330);
            ViewBag.lstAllCampaigns = db.campaign_master.Where(x => x.campaign_master_start_date > curdate).ToList();
            return View();
        }


        public ActionResult upcomingcampaigns()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {


                ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).ToList();
                ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).ToList();
                ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).ToList();
                ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).ToList();

                ViewBag.lstAllCampaigns = (from a in db.campaign_master
                                           join b in db.channels on a.channel_id equals b.channel_id
                                           join c in db.campaign_category on a.campaign_category_id equals c.campaign_category_id
                                           join d in db.campaigns on a.campaign_id equals d.campaign_id
                                           join e in db.subcampaigns on a.subcampaign_id equals e.subcampaign_id

                                           select new CustomModel.ViewModelCampaignCategory
                                           {
                                               cm = a,
                                               ch = b,
                                               cc = c,
                                               cam = d,
                                               sc = e
                                           }).ToList();
                return View();
            }

        }


        public ActionResult calendar()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {

                return View();
            }

        }
        //==================CentralBlast===on whataaps message=show===============
        public ActionResult CentralBlast()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Dropdowns
            ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
            //ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null).OrderBy(x => x.campaign_category_name).ToList();
            ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null && x.Campaign_Category_Status == "0").OrderBy(x => x.campaign_category_name).ToList();

            ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).OrderBy(x => x.campaign_name).ToList();
            ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).OrderBy(x => x.subcampaign_name).ToList();

            ViewBag.lstlanguage = new List<SelectListItem>()
    {
        new SelectListItem { Text = "English", Value = "English" },
        new SelectListItem { Text = "Hindi", Value = "Hindi" },
        new SelectListItem { Text = "Marathi", Value = "Marathi" },
        new SelectListItem { Text = "Tamil", Value = "Tamil" }
    };

            ViewBag.lsttype = new List<SelectListItem>()
    {
                 new SelectListItem { Text = "Blogs", Value = "Blogs" },
        new SelectListItem { Text = "Creative", Value = "Creative" },
        new SelectListItem { Text = "Video", Value = "Video" }

    };

            // Empty filter values
            ViewBag.channel = "";
            ViewBag.category = "";
            ViewBag.campaign = "";
            ViewBag.subcampaign = "";
            ViewBag.language = "";
            ViewBag.type = "";
            ViewBag.searchText = "";

            // Load all Active + Paused campaigns
            var qry1 = db.campaign_master
                .Where(x =>
                       (x.camapaign_master_status == "Active" || x.camapaign_master_status == "Paused")
                       && x.CampaignName != null
                       && x.CentralBlast == "Yes")
                .OrderByDescending(x => x.campaign_master_id)
                .ToList();

            // ✅ Automatic DB update logic: Calculate and persist status changes
            int expiredCount = 0;
            foreach (var item in qry1)
            {
                string originalStatus = item.camapaign_master_status;
                string calculatedStatus = originalStatus; // Default to original

                if (originalStatus == "Paused")
                {
                    // For Paused: Check if end date has passed, then set to Stopped
                    if (item.campaign_master_end_date < DateTime.Now)
                    {
                        calculatedStatus = "Stopped";
                    }
                    else
                    {
                        calculatedStatus = "Paused";
                    }
                }
                else
                {
                    // For non-Paused (assumed Active or others)
                    if (item.campaign_master_start_date > DateTime.Now)
                    {
                        calculatedStatus = "Upcoming";
                    }
                    else if (item.campaign_master_start_date <= DateTime.Now && item.campaign_master_end_date >= DateTime.Now)
                    {
                        calculatedStatus = "Active";
                    }
                    else if (item.campaign_master_end_date < DateTime.Now)
                    {
                        calculatedStatus = "Stopped";
                    }
                }

                // If calculated status differs, update DB and count if expired
                if (calculatedStatus != originalStatus)
                {
                    item.camapaign_master_status = calculatedStatus;
                    if (calculatedStatus == "Stopped")
                    {
                        expiredCount++;
                    }
                }

                // For display: Set the status for the view (this is already calculated)
                item.camapaign_master_status = calculatedStatus;
            }

            // Persist changes to DB if any
            if (qry1.Any(x => x.camapaign_master_status != x.camapaign_master_status)) // This check is redundant, but for safety
            {
                db.SaveChanges();
            }

            ViewBag.lstAllCampaigns = qry1;

            // Optional: Set a message if any were expired
            if (expiredCount > 0)
            {
                TempData["SuccessMessage"] = $"{expiredCount} campaigns were automatically stopped due to expiration.";
            }

            return View();
        }

        [HttpPost]
        public ActionResult CentralBlast(FormCollection form)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Reload dropdowns
            ViewBag.lstchannels = db.channels.Where(x => x.channel_delflag == null).OrderBy(x => x.channel_name).ToList();
            ViewBag.lstcategory = db.campaign_category.Where(x => x.campaign_category_delflag == null && x.Campaign_Category_Status == "0").OrderBy(x => x.campaign_category_name).ToList();
            ViewBag.lstcampaign = db.campaigns.Where(x => x.campaign_delflag == null).OrderBy(x => x.campaign_name).ToList();
            ViewBag.lstsubcampaign = db.subcampaigns.Where(x => x.subcampaign_delflag == null).OrderBy(x => x.subcampaign_name).ToList();

            ViewBag.lstlanguage = new List<SelectListItem>()
    {
        new SelectListItem { Text = "English", Value = "English" },
        new SelectListItem { Text = "Hindi", Value = "Hindi" },
        new SelectListItem { Text = "Marathi", Value = "Marathi" },
        new SelectListItem { Text = "Tamil", Value = "Tamil" }
    };

            ViewBag.lsttype = new List<SelectListItem>()
    {
                new SelectListItem { Text = "Blogs", Value = "Blogs" },
        new SelectListItem { Text = "Creative", Value = "Creative" },
        new SelectListItem { Text = "Video", Value = "Video" }

    };

            // Get filter values
            ViewBag.channel = form["channel"] ?? string.Empty;
            ViewBag.category = form["category"] ?? string.Empty;
            ViewBag.campaign = form["campaign"] ?? string.Empty;
            ViewBag.subcampaign = form["subcampaign"] ?? string.Empty;
            ViewBag.language = form["language"] ?? string.Empty;
            ViewBag.type = form["campaigntype"] ?? string.Empty;
            ViewBag.searchText = form["searchText"] ?? string.Empty;

            // ✅ Base query (Active + Paused campaigns)
            var qry1 = db.campaign_master
                .Where(x => (x.camapaign_master_status == "Active" || x.camapaign_master_status == "Paused")
                         && x.CampaignName != null
                         && x.CentralBlast == "Yes")
                .AsQueryable();

            // ✅ Apply filters from form
            if (!string.IsNullOrEmpty(form["channel"]))
            {
                decimal channel = Convert.ToDecimal(form["channel"]);
                qry1 = qry1.Where(i => i.channel_id == channel);
            }

            if (!string.IsNullOrEmpty(form["category"]))
            {
                decimal category = Convert.ToDecimal(form["category"]);
                qry1 = qry1.Where(i => i.campaign_category_id == category);
            }

            if (!string.IsNullOrEmpty(form["campaign"]))
            {
                decimal campaign = Convert.ToDecimal(form["campaign"]);
                qry1 = qry1.Where(i => i.campaign_id == campaign);
            }

            if (!string.IsNullOrEmpty(form["subcampaign"]))
            {
                decimal subcampaign = Convert.ToDecimal(form["subcampaign"]);
                qry1 = qry1.Where(i => i.subcampaign_id == subcampaign);
            }

            if (!string.IsNullOrEmpty(form["language"]))
            {
                string lang = form["language"];
                qry1 = qry1.Where(i => i.campaign_master_lang == lang);
            }

            if (!string.IsNullOrEmpty(form["campaigntype"]))
            {
                string ctype = form["campaigntype"];
                qry1 = qry1.Where(i => i.campaign_master_type == ctype);
            }

            // ✅ Textbox filter
            if (!string.IsNullOrEmpty(form["searchText"]))
            {
                string search = form["searchText"].Trim();
                qry1 = qry1.Where(i => i.CampaignName.Contains(search));
            }

            // Execute query
            var result = qry1.OrderByDescending(x => x.campaign_master_id).ToList();

            // ✅ Automatic DB update logic: Calculate and persist status changes
            int expiredCount = 0;
            foreach (var item in result)
            {
                string originalStatus = item.camapaign_master_status;
                string calculatedStatus = originalStatus; // Default to original

                if (originalStatus == "Paused")
                {
                    // For Paused: Check if end date has passed, then set to Stopped
                    if (item.campaign_master_end_date < DateTime.Now)
                    {
                        calculatedStatus = "Stopped";
                    }
                    else
                    {
                        calculatedStatus = "Paused";
                    }
                }
                else
                {
                    // For non-Paused (assumed Active or others)
                    if (item.campaign_master_start_date > DateTime.Now)
                    {
                        calculatedStatus = "Upcoming";
                    }
                    else if (item.campaign_master_start_date <= DateTime.Now && item.campaign_master_end_date >= DateTime.Now)
                    {
                        calculatedStatus = "Active";
                    }
                    else if (item.campaign_master_end_date < DateTime.Now)
                    {
                        calculatedStatus = "Stopped";
                    }
                }

                // If calculated status differs, update DB and count if expired
                if (calculatedStatus != originalStatus)
                {
                    item.camapaign_master_status = calculatedStatus;
                    if (calculatedStatus == "Stopped")
                    {
                        expiredCount++;
                    }
                }

                // For display: Set the status for the view (this is already calculated)
                item.camapaign_master_status = calculatedStatus;
            }

            // Persist changes to DB if any
            db.SaveChanges();

            ViewBag.lstAllCampaigns = result;

            // Optional: Set a message if any were expired
            if (expiredCount > 0)
            {
                TempData["SuccessMessage"] = $"{expiredCount} campaigns were automatically stopped due to expiration.";
            }
            //======textbox ko blank karne ke liye=================
            ViewBag.channel = "";
            ViewBag.category = "";
            ViewBag.campaign = "";
            ViewBag.subcampaign = "";
            ViewBag.language = "";
            ViewBag.type = "";
            ViewBag.searchText = "";
            //=======================
            return View();
        }

        [HttpPost]
        public JsonResult UpdateSpecificCampaignStatuses(string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids))
                {
                    return Json(new { success = true, expiredCount = 0 });
                }

                var idList = ids.Split(',').Select(decimal.Parse).ToList();

                // Fetch only the specified campaigns that match the criteria
                var campaigns = db.campaign_master
                    .Where(x => idList.Contains(x.campaign_master_id) &&
                                (x.camapaign_master_status == "Active" ||
                                 x.camapaign_master_status == "Paused" ||
                                 x.camapaign_master_status == "Upcoming") && // Include Upcoming campaigns
                                x.CampaignName != null &&
                                x.CentralBlast == "Yes")
                    .ToList();

                int expiredCount = 0;
                foreach (var item in campaigns)
                {
                    string originalStatus = item.camapaign_master_status;
                    string calculatedStatus = originalStatus;

                    // Calculate status for all campaigns, including Upcoming
                    if (item.campaign_master_end_date < DateTime.Now)
                    {
                        calculatedStatus = "Stopped"; // End date passed, set to Stopped
                    }
                    else if (item.campaign_master_start_date > DateTime.Now)
                    {
                        calculatedStatus = "Upcoming"; // Start date in future, keep/set to Upcoming
                    }
                    else if (item.campaign_master_start_date <= DateTime.Now &&
                             item.campaign_master_end_date >= DateTime.Now)
                    {
                        calculatedStatus = "Active"; // Between start and end dates, set to Active
                    }
                    else if (originalStatus == "Paused")
                    {
                        calculatedStatus = "Paused"; // Preserve Paused status unless expired
                    }

                    if (calculatedStatus != originalStatus)
                    {
                        item.camapaign_master_status = calculatedStatus;
                        if (calculatedStatus == "Stopped")
                        {
                            expiredCount++;
                        }
                    }
                }

                db.SaveChanges();

                return Json(new { success = true, expiredCount = expiredCount, message = $"{expiredCount} campaigns updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult DownloadCentralBlastTemplate()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    SheetData sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet()
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "CentralBlast"
                    };
                    sheets.Append(sheet);

                    // Headers
                    string[] headers = { "Sr_Id", "Name", "Contact_Number", "Type_Of_Data", "L1_Code", "L1_Name", "Channel" };
                    Row headerRow = new Row();
                    foreach (var header in headers)
                    {
                        headerRow.AppendChild(new Cell
                        {
                            CellValue = new CellValue(header),
                            DataType = CellValues.String
                        });
                    }
                    sheetData.AppendChild(headerRow);

                    // Sample Rows
                    string[][] sampleRows = new string[][]
                    {
                new string[] { "1", "Kamlesh", "9818889998", "Customer", "125987", "Sanjay", "AG" },
                new string[] { "2", "Vikas", "9818889998", "Ex-Customers", "987562", "Abhishek", "AG" }
                    };

                    foreach (var rowData in sampleRows)
                    {
                        Row row = new Row();
                        foreach (var value in rowData)
                        {
                            row.AppendChild(new Cell
                            {
                                CellValue = new CellValue(value),
                                DataType = CellValues.String
                            });
                        }
                        sheetData.AppendChild(row);
                    }

                    workbookPart.Workbook.Save();
                }

                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "CentralBlast_Template.xlsx");
            }
        }




        //[HttpPost]
        //public async Task<ActionResult> UploadExcel(HttpPostedFileBase excelFile, string CampaignName)
        //{
        //    if (Session["userid"] == null)
        //        return RedirectToAction("Index");

        //    if (excelFile == null || excelFile.ContentLength <= 0)
        //    {
        //        TempData["ErrorMessage"] = "Please upload a valid Excel file.";
        //        return RedirectToAction("CentralBlast");
        //    }

        //    try
        //    {
        //        using (var stream = excelFile.InputStream)
        //        using (SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false))
        //        {
        //            WorkbookPart workbookPart = document.WorkbookPart;
        //            Sheet sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
        //            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
        //            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

        //            foreach (Row row in sheetData.Elements<Row>().Skip(1))   // Skip header
        //            {
        //                string name = GetCellValue(document, row, "B");
        //                string mobile = GetCellValue(document, row, "C");

        //                // --- Normalize mobile ---
        //                mobile = mobile.Replace(" ", "").Replace("-", "").Trim();

        //                if (mobile.Length == 10)
        //                    mobile = "+91" + mobile;
        //                else if (mobile.StartsWith("91"))
        //                    mobile = "+" + mobile;
        //                else if (!mobile.StartsWith("+"))
        //                    mobile = "+91" + mobile;

        //                // Skip invalid number
        //                if (mobile.Length < 10)
        //                    continue;

        //                // SEND WHATSAPP
        //                await SendWhatsAppMessageAsync(mobile, "Campaign", name);

        //                System.Diagnostics.Debug.WriteLine("SENT TO: " + mobile);
        //            }
        //        }

        //        TempData["SuccessMessage"] = "WhatsApp sent to ALL Excel numbers!";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = ex.Message;
        //    }

        //    return RedirectToAction("CentralBlast");
        //}

        //private string GetCellValue(SpreadsheetDocument doc, Row row, string columnName)
        //{
        //    Cell cell = row.Elements<Cell>()
        //                   .FirstOrDefault(c =>
        //                       c.CellReference != null &&
        //                       c.CellReference.Value.StartsWith(columnName));

        //    if (cell == null)
        //        return string.Empty;

        //    // Shared string
        //    if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        //    {
        //        return doc.WorkbookPart.SharedStringTablePart.SharedStringTable
        //                 .ElementAt(int.Parse(cell.CellValue.InnerText)).InnerText;
        //    }

        //    // Inline string
        //    if (cell.DataType != null && cell.DataType.Value == CellValues.InlineString)
        //    {
        //        return cell.InnerText;
        //    }

        //    // Numbers / General
        //    return cell.InnerText ?? string.Empty;
        //}



        //====================================

        [HttpPost]
        public async Task<ActionResult> UploadExcel(HttpPostedFileBase excelFile, string CampaignName, string status)
        {
            ServerLog("===== UploadExcel START =====");

            // 🔒 Session check
            if (Session["userid"] == null)
            {
                ServerLog("Session expired");
                return RedirectToAction("Index");
            }

            // ✅ Campaign check
            if (string.IsNullOrWhiteSpace(CampaignName))
            {
                ServerLog("CampaignName empty");
                TempData["ErrorMessage"] = "Please select a Campaign before uploading.";
                return RedirectToAction("CentralBlast");
            }

            // 📂 File check
            if (excelFile == null || excelFile.ContentLength <= 0)
            {
                ServerLog("Excel file missing");
                TempData["ErrorMessage"] = "Please upload a valid Excel file.";
                return RedirectToAction("CentralBlast");
            }

            try
            {
                ServerLog("File Name: " + excelFile.FileName);
                ServerLog("Campaign: " + CampaignName);

                using (var document = SpreadsheetDocument.Open(excelFile.InputStream, false))
                {
                    WorkbookPart workbookPart = document.WorkbookPart;
                    Sheet sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();

                    if (sheet == null)
                        throw new Exception("No sheet found in Excel");

                    WorksheetPart worksheetPart =
                        (WorksheetPart)workbookPart.GetPartById(sheet.Id);

                    SheetData sheetData =
                        worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

                    int totalRows = sheetData.Elements<Row>().Count();
                    ServerLog("Total rows found (including header): " + totalRows);
                    //string conStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    // string conStr = "Data Source=10.126.143.86,1981;Initial Catalog=Webinar;User ID=reliance_user;Password=pass@123;MultipleActiveResultSets=True;Connection Timeout=10000;";
                    string conStr = "Data Source=10.126.143.86,1981;Initial Catalog=DIGIMYIN;User ID=reliance_user;Password=pass@123;MultipleActiveResultSets=True;Connection Timeout=10000;";


                    //string conStr =
                    //    "Data Source=10.126.143.86,1981;" +
                    //    "Initial Catalog=Webinar;" +
                    //    "User ID=reliance_user;" +
                    //    "Password=pass@123;" +
                    //    "MultipleActiveResultSets=True;" +
                    //    "Connection Timeout=10000;";

                    using (SqlConnection con = new SqlConnection(conStr))
                    {
                        con.Open();
                        ServerLog("DB Connected");

                        int insertCount = 0;

                        // 🔁 Skip header row
                        foreach (Row row in sheetData.Elements<Row>().Skip(1))
                        {
                            try
                            {
                                // 📌 Column Index (0-based)
                                // A=0 B=1 C=2 D=3 E=4 F=5 G=6
                                string name = GetCellValueSafe(row, 1); // B
                                string mobile = GetCellValueSafe(row, 2); // C
                                string typeOfData = GetCellValueSafe(row, 3); // D
                                string l1CodeText = GetCellValueSafe(row, 4); // E
                                string l1Name = GetCellValueSafe(row, 5); // F
                                string channel = GetCellValueSafe(row, 6); // G

                                ServerLog($"Row {row.RowIndex} Raw Mobile: [{mobile}]");

                                // ❌ Skip empty mobile
                                if (string.IsNullOrWhiteSpace(mobile))
                                {
                                    ServerLog($"Row {row.RowIndex} skipped (mobile empty)");
                                    continue;
                                }

                                // 📞 Normalize mobile
                                mobile = mobile.Replace(" ", "").Replace("-", "").Trim();

                                if (mobile.Length == 10)
                                    mobile = "+91" + mobile;
                                else if (mobile.StartsWith("91"))
                                    mobile = "+" + mobile;
                                else if (!mobile.StartsWith("+"))
                                    mobile = "+91" + mobile;

                                int l1Code = 0;
                                int.TryParse(l1CodeText, out l1Code);

                                // 🧾 INSERT CENTRAL_BLAST
                                using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO CENTRAL_BLAST
                            (Name, Contact_Number, Type_Of_Data, L1_Code, L1_Name, Channel, CampaignName)
                            VALUES
                            (@Name, @Contact, @Type, @L1Code, @L1Name, @Channel, @Campaign)", con))
                                {
                                    cmd.Parameters.AddWithValue("@Name", name ?? "");
                                    cmd.Parameters.AddWithValue("@Contact", mobile);
                                    cmd.Parameters.AddWithValue("@Type", typeOfData ?? "");
                                    cmd.Parameters.AddWithValue("@L1Code", l1Code);
                                    cmd.Parameters.AddWithValue("@L1Name", l1Name ?? "");
                                    cmd.Parameters.AddWithValue("@Channel", channel ?? "");
                                    cmd.Parameters.AddWithValue("@Campaign", CampaignName);

                                    cmd.ExecuteNonQuery();
                                }

                                // 🧾 INSERT CampaignResponses
                                using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO CampaignResponses
                            (Mobile, CampaignName, Response, CreatedDate)
                            VALUES
                            (@Mobile, @Campaign, 'Pending', GETDATE())", con))
                                {
                                    cmd.Parameters.AddWithValue("@Mobile", mobile);
                                    cmd.Parameters.AddWithValue("@Campaign", CampaignName);
                                    cmd.ExecuteNonQuery();
                                }

                                insertCount++;
                                ServerLog($"Row {row.RowIndex} inserted | Mobile: {mobile}");

                                // 📲 WhatsApp
                                await SendWhatsAppMessageAsync(mobile, CampaignName);
                            }
                            catch (Exception rowEx)
                            {
                                ServerLog($"Row {row.RowIndex} ERROR: {rowEx}");
                            }
                        }

                        ServerLog("Total rows inserted: " + insertCount);
                    }
                }

                TempData["SuccessMessage"] =
                    "Data saved & WhatsApp messages sent successfully!";
                ServerLog("===== UploadExcel SUCCESS =====");
            }
            catch (Exception ex)
            {
                ServerLog("FATAL ERROR: " + ex);
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("CentralBlast");
        }

        private void ServerLog(string msg)
        {
            try
            {
                string path = Server.MapPath("~/UploadExcel_Log.txt");
                System.IO.File.AppendAllText(
                    path,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " :: " + msg + Environment.NewLine
                );
            }
            catch { }
        }

        //===========================================
        private string GetCellValueSafe(Row row, int columnIndex)
        {
            if (row == null)
                return "";

            // OpenXML rows are sparse → convert to list
            var cells = row.Elements<Cell>().ToList();

            if (cells.Count <= columnIndex)
                return "";

            Cell cell = cells[columnIndex];
            if (cell == null)
                return "";

            // Numeric / normal value
            if (cell.CellValue != null)
                return cell.CellValue.InnerText.Trim();

            // Inline string support
            if (cell.DataType != null &&
                cell.DataType.Value == CellValues.InlineString)
            {
                return cell.InnerText?.Trim() ?? "";
            }

            return "";
        }


        //[HttpPost]
        //public async Task<ActionResult> UploadExcel(HttpPostedFileBase excelFile, string CampaignName, string status)
        //{
        //    ServerLog("===== UploadExcel START =====");

        //    if (Session["userid"] == null)
        //    {
        //        ServerLog("Session expired");
        //        return RedirectToAction("Index");
        //    }

        //    if (string.IsNullOrEmpty(CampaignName))
        //    {
        //        ServerLog("CampaignName empty");
        //        TempData["ErrorMessage"] = "Please select a Campaign before uploading.";
        //        return RedirectToAction("CentralBlast");
        //    }

        //    if (excelFile == null || excelFile.ContentLength <= 0)
        //    {
        //        ServerLog("Excel file missing");
        //        TempData["ErrorMessage"] = "Please upload a valid Excel file.";
        //        return RedirectToAction("CentralBlast");
        //    }

        //    try
        //    {
        //        ServerLog("File Name: " + excelFile.FileName);
        //        ServerLog("Campaign: " + CampaignName);

        //        using (var stream = excelFile.InputStream)
        //        using (SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false))
        //        {
        //            WorkbookPart workbookPart = document.WorkbookPart;
        //            Sheet sheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();

        //            if (sheet == null)
        //            {
        //                ServerLog("Sheet NOT FOUND");
        //                throw new Exception("No sheet found in Excel");
        //            }

        //            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
        //            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

        //            int totalRows = sheetData.Elements<Row>().Count();
        //            ServerLog("Total rows found (including header): " + totalRows);

        //            //string conStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //           string conStr = "Data Source=10.126.143.86,1981;Initial Catalog=Webinar;User ID=reliance_user;Password=pass@123;MultipleActiveResultSets=True;Connection Timeout=10000;";

        //            using (SqlConnection con = new SqlConnection(conStr))
        //            {
        //                con.Open();
        //                ServerLog("DB Connected");

        //                int insertCount = 0;

        //                foreach (Row row in sheetData.Elements<Row>().Skip(1)) // skip header
        //                {
        //                    try
        //                    {
        //                        string name = GetCellValue(document, row, "B");
        //                        string mobile = GetCellValue(document, row, "C");
        //                        string typeOfData = GetCellValue(document, row, "D");
        //                        string l1CodeText = GetCellValue(document, row, "E");
        //                        string l1Name = GetCellValue(document, row, "F");
        //                        string channel = GetCellValue(document, row, "G");

        //                        ServerLog($"Row {row.RowIndex} Raw Mobile: [{mobile}]");

        //                        if (string.IsNullOrWhiteSpace(mobile))
        //                        {
        //                            ServerLog($"Row {row.RowIndex} skipped (mobile empty)");
        //                            continue;
        //                        }

        //                        mobile = mobile.Replace(" ", "").Replace("-", "").Trim();

        //                        if (mobile.Length == 10)
        //                            mobile = "+91" + mobile;
        //                        else if (mobile.StartsWith("91"))
        //                            mobile = "+" + mobile;
        //                        else if (!mobile.StartsWith("+"))
        //                            mobile = "+91" + mobile;

        //                        int l1Code = 0;
        //                        int.TryParse(l1CodeText, out l1Code);

        //                        using (SqlCommand cmd = new SqlCommand(@"
        //                    INSERT INTO CENTRAL_BLAST
        //                    (Name, Contact_Number, Type_Of_Data, L1_Code, L1_Name, Channel, CampaignName)
        //                    VALUES
        //                    (@Name, @Contact_Number, @Type_Of_Data, @L1_Code, @L1_Name, @Channel, @CampaignName)", con))
        //                        {
        //                            cmd.Parameters.AddWithValue("@Name", name ?? "");
        //                            cmd.Parameters.AddWithValue("@Contact_Number", mobile);
        //                            cmd.Parameters.AddWithValue("@Type_Of_Data", typeOfData ?? "");
        //                            cmd.Parameters.AddWithValue("@L1_Code", l1Code);
        //                            cmd.Parameters.AddWithValue("@L1_Name", l1Name ?? "");
        //                            cmd.Parameters.AddWithValue("@Channel", channel ?? "");
        //                            cmd.Parameters.AddWithValue("@CampaignName", CampaignName);

        //                            cmd.ExecuteNonQuery();
        //                        }

        //                        using (SqlCommand cmd = new SqlCommand(@"
        //                    INSERT INTO CampaignResponses
        //                    (Mobile, CampaignName, Response, CreatedDate)
        //                    VALUES
        //                    (@Mobile, @CampaignName, 'Pending', GETDATE())", con))
        //                        {
        //                            cmd.Parameters.AddWithValue("@Mobile", mobile);
        //                            cmd.Parameters.AddWithValue("@CampaignName", CampaignName);
        //                            cmd.ExecuteNonQuery();
        //                        }

        //                        insertCount++;
        //                        ServerLog($"Row {row.RowIndex} inserted successfully | Mobile: {mobile}");

        //                        await SendWhatsAppMessageAsync(mobile, CampaignName);
        //                    }
        //                    catch (Exception rowEx)
        //                    {
        //                        ServerLog($"Row {row.RowIndex} ERROR: {rowEx}");
        //                    }
        //                }

        //                ServerLog("Total rows inserted: " + insertCount);
        //                con.Close();
        //            }
        //        }

        //        TempData["SuccessMessage"] = "Data saved & WhatsApp messages sent to ALL contacts!";
        //        ServerLog("===== UploadExcel SUCCESS =====");
        //    }
        //    catch (Exception ex)
        //    {
        //        ServerLog("FATAL ERROR: " + ex);
        //        TempData["ErrorMessage"] = "Error: " + ex.Message;
        //    }

        //    return RedirectToAction("CentralBlast");
        //}

        //private void ServerLog(string msg)
        //{
        //    try
        //    {
        //        string path = Server.MapPath("~/UploadExcel_Log.txt");
        //        System.IO.File.AppendAllText(
        //            path,
        //            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " :: " + msg + Environment.NewLine
        //        );
        //    }
        //    catch { }
        //}


        //public async Task<ActionResult> UploadExcel(HttpPostedFileBase excelFile, string CampaignName,string status)
        //{
        //    if (Session["userid"] == null)
        //        return RedirectToAction("Index");

        //    if (string.IsNullOrEmpty(CampaignName))
        //    {
        //        TempData["ErrorMessage"] = "Please select a Campaign before uploading.";
        //        return RedirectToAction("CentralBlast");
        //    }

        //    if (excelFile == null || excelFile.ContentLength <= 0)
        //    {
        //        TempData["ErrorMessage"] = "Please upload a valid Excel file.";
        //        return RedirectToAction("CentralBlast");
        //    }

        //    try
        //    {
        //        using (var stream = excelFile.InputStream)
        //        using (SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false))
        //        {
        //            WorkbookPart workbookPart = document.WorkbookPart;
        //            Sheet sheet = workbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
        //            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
        //            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

        //            string conStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //            //string conStr = "Data Source=10.126.143.86,1981;Initial Catalog=Webinar;User ID=reliance_user;Password=pass@123;MultipleActiveResultSets=True;Connection Timeout=10000;";


        //            using (SqlConnection con = new SqlConnection(conStr))
        //            {
        //                con.Open();

        //                foreach (Row row in sheetData.Elements<Row>().Skip(1)) // skip header
        //                {
        //                    // READ ALL CELLS SAFELY
        //                    string name = GetCellValue(document, row, "B");
        //                    string mobile = GetCellValue(document, row, "C");
        //                    string typeOfData = GetCellValue(document, row, "D");
        //                    string l1CodeText = GetCellValue(document, row, "E");
        //                    string l1Name = GetCellValue(document, row, "F");
        //                    string channel = GetCellValue(document, row, "G");

        //                    // --- Normalize Mobile Number ---
        //                    mobile = mobile.Replace(" ", "").Replace("-", "").Trim();

        //                    if (string.IsNullOrWhiteSpace(mobile))
        //                        continue;

        //                    // Ensure country code
        //                    if (mobile.Length == 10)
        //                        mobile = "+91" + mobile;
        //                    else if (mobile.StartsWith("91"))
        //                        mobile = "+" + mobile;
        //                    else if (!mobile.StartsWith("+"))
        //                        mobile = "+91" + mobile;

        //                    // Convert L1 Code
        //                    int l1Code = 0;
        //                    int.TryParse(l1CodeText, out l1Code);

        //                    // Insert into DB
        //                    using (SqlCommand cmd = new SqlCommand(@"
        //                INSERT INTO CENTRAL_BLAST 
        //                (Name, Contact_Number, Type_Of_Data, L1_Code, L1_Name, Channel, CampaignName)
        //                VALUES 
        //                (@Name, @Contact_Number, @Type_Of_Data, @L1_Code, @L1_Name, @Channel, @CampaignName)", con))
        //                    {
        //                        cmd.Parameters.AddWithValue("@Name", name);
        //                        cmd.Parameters.AddWithValue("@Contact_Number", mobile);
        //                        cmd.Parameters.AddWithValue("@Type_Of_Data", typeOfData);
        //                        cmd.Parameters.AddWithValue("@L1_Code", l1Code);
        //                        cmd.Parameters.AddWithValue("@L1_Name", l1Name);
        //                        cmd.Parameters.AddWithValue("@Channel", channel);
        //                        cmd.Parameters.AddWithValue("@CampaignName", CampaignName);

        //                        cmd.ExecuteNonQuery();
        //                    }

        //                    using (SqlCommand cmd = new SqlCommand(@"
        //                        INSERT INTO CampaignResponses
        //                        (Mobile, CampaignName, Response, CreatedDate)
        //                        VALUES 
        //                        (@Mobile, @CampaignName, 'Pending', GETDATE())", con))
        //                    {
        //                        cmd.Parameters.AddWithValue("@Mobile", mobile);
        //                        cmd.Parameters.AddWithValue("@CampaignName", CampaignName);
        //                        cmd.ExecuteNonQuery();
        //                    }


        //                    // SEND WHATSAPP (one-by-one for all rows)
        //                    await SendWhatsAppMessageAsync(mobile, CampaignName);
        //                    //await SendWhatsAppMessageAsync(mobile, CampaignName, status);

        //                }

        //                con.Close();
        //            }
        //        }

        //        TempData["SuccessMessage"] = "Data saved & WhatsApp messages sent to ALL contacts!";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error: " + ex.Message;
        //    }

        //    return RedirectToAction("CentralBlast");
        //}



        //private string GetCellValue(SpreadsheetDocument document, Row row, string columnName)
        //{
        //    if (row == null) return "";

        //    string cellRef = columnName + row.RowIndex;

        //    Cell cell = row.Elements<Cell>()
        //                   .FirstOrDefault(c => c.CellReference != null &&
        //                                        c.CellReference.Value == cellRef);

        //    if (cell == null)
        //        return "";

        //    // If the cell has a value
        //    string value = cell.CellValue?.InnerText;
        //    if (value == null)
        //        return "";

        //    // Shared string
        //    if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        //    {
        //        return document.WorkbookPart.SharedStringTablePart
        //                       .SharedStringTable
        //                       .ElementAt(int.Parse(value))
        //                       .InnerText;
        //    }

        //    // Inline string
        //    if (cell.DataType != null && cell.DataType.Value == CellValues.InlineString)
        //    {
        //        return cell.InnerText;
        //    }

        //    // Numeric / normal value
        //    return value;
        //}

        //==========================================

        //public void SaveTicketToDB(string mobile, string campaign, string status, string ticketId)
        //{
        //    using (rglinixm_relEntities db = new rglinixm_relEntities())
        //    {
        //        var obj = new LeadTicketHistory();
        //        obj.Mobile = mobile;
        //        obj.CampaignName = campaign;
        //        obj.Status = status;
        //        obj.TicketId = ticketId;
        //        obj.CreatedDate = DateTime.Now;

        //        db.LeadTicketHistories.Add(obj);
        //        db.SaveChanges();
        //    }
        //}



        //private async Task SendWhatsAppMessageAsync(string mobileNumber, string CampaignName, string status)
        //{
        //     📌 Pehle LeadSquared me Opportunity Save karo
        //    string ticketId = await SendToLeadSquaredAsync(mobileNumber, CampaignName, status);

        //     📌 TicketId ko Database me Save karo
        //    SaveTicketToDB(mobileNumber, CampaignName, status, ticketId);

        //     📌 WhatsApp Message Sending
        //    var apiUrl = "https://kapi.omni-channel.in/fe/api/v1/iPMessage/One2Many";

        //    var requestBody = new
        //    {
        //        mode = "waba",
        //        wabaPhoneNumber = "+919220363790",
        //        wabaTemplateId = "388940",
        //        campId = "66012",
        //        unicode = false,
        //        shortMessages = new[]
        //        {
        //    new {
        //        recipient = mobileNumber,
        //        corelationId = ticketId,       // <-- TicketId WhatsApp ke sath bhi bhej diya
        //        context = new {
        //            waba_var1 = CampaignName
        //        }
        //    }
        //}
        //    };

        //    string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

        //    using (var client = new HttpClient())
        //    {
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");
        //        client.DefaultRequestHeaders.Add("Authorization", "Basic UmVsaWFuY2VXQUJBOjhVMU5x");

        //        await client.PostAsync(apiUrl, content);
        //    }
        //}


        private async Task SendWhatsAppMessageAsync(string mobileNumber, string CampaignName)
        {
            var apiUrl = "https://kapi.omni-channel.in/fe/api/v1/iPMessage/One2Many";

            var requestBody = new
            {
                mode = "waba",
                wabaPhoneNumber = "+919220363790",
                wabaTemplateId = "388940",   // Updated as per your CURL
                campId = "66012",
                unicode = false,
                shortMessages = new[]
                {
            new {
                recipient = mobileNumber,
                corelationId = "1234667",
                context = new {
                    waba_var1 =CampaignName

                }
            }
        }
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

            using (var client = new HttpClient())
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "Basic UmVsaWFuY2VXQUJBOjhVMU5x");

                try
                {
                    var response = await client.PostAsync(apiUrl, content);
                    string result = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine("WhatsApp Msg Response: " + result);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WhatsApp API Error: {ex.Message}");
                }
            }
        }
        //========================================
        //public async Task<string> SendToLeadSquaredAsync(string phone, string campaignName, string status)
        //{
        //    string url = "https://api-in21.leadsquared.com/v2/OpportunityManagement.svc/Capture?accessKey=u$re5470b019cbada9931f9d41d27261f60&secretKey=5dc21cf82842f7bdccc8c220df4683d8ef2eab27";

        //    var requestBody = new
        //    {
        //        LeadDetails = new[]
        //        {
        //    new { Attribute = "SearchBy", Value = "Phone" },
        //    new { Attribute = "__UseUserDefinedGuid__", Value = "true" },
        //    new { Attribute = "Source", Value = "API" },
        //    new { Attribute = "Phone", Value = phone }
        //},

        //        Opportunity = new
        //        {
        //            OpportunityEventCode = 12000,
        //            OpportunityNote = "Opportunity from Website",
        //            OverwriteFields = true,
        //            DoNotPostDuplicateActivity = false,
        //            DoNotChangeOwner = true,

        //            Fields = new object[]
        //            {
        //        new { SchemaName = "mx_Custom_2", Value = "New Lead" },
        //        new { SchemaName = "Status", Value = status },
        //        new { SchemaName = "mx_Custom_11", Value = "Navyug" },
        //        new { SchemaName = "mx_Custom_1", Value = campaignName },
        //        new { SchemaName = "mx_Custom_35", Value = "" },
        //        new { SchemaName = "mx_Custom_13", Value = "1971-12-11" },
        //        new { SchemaName = "mx_Custom_12", Value = "New Business" },
        //        new { SchemaName = "mx_Custom_45", Value = "188" },
        //        new { SchemaName = "mx_Custom_15", Value = "20-50 Lacs" },
        //        new { SchemaName = "mx_Custom_27", Value = "Navyug leads" }
        //            }
        //        }
        //    };

        //    string json = JsonConvert.SerializeObject(requestBody);

        //    using (var client = new HttpClient())
        //    {
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");
        //        var response = await client.PostAsync(url, content);
        //        string result = await response.Content.ReadAsStringAsync();

        //        System.Diagnostics.Debug.WriteLine("LeadSquared Response: " + result);

        //        dynamic obj = JsonConvert.DeserializeObject(result);

        //        // ⭐ Extract correct TicketId safely
        //        string ticketId =
        //            obj?.RecordId ??
        //            obj?.OpportunityId ??
        //            obj?.ProspectId ??
        //            "";

        //        return ticketId;
        //    }
        //}



        //=============================
        //private async Task SendWhatsAppMessageAsync(string mobileNumber, string source, string name)
        //{
        //    var apiUrl = "https://kapi.omni-channel.in/fe/api/v1/iPMessage/One2Many";

        //    // JSON Body बनाना
        //    var requestBody = new
        //    {
        //        mode = "waba",
        //        wabaPhoneNumber = "+919220363790",
        //        wabaTemplateId = "381660",
        //        campId = "66012",
        //        unicode = false,
        //        shortMessages = new[]
        //        {
        //    new {
        //        recipient = mobileNumber,
        //        corelationId = "1234667",
        //        context = new {
        //            // अगर template variables हैं तो yaha pass करेंगे
        //            // Example: param1 = name, param2 = source
        //        }
        //    }
        //}
        //    };

        //    string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

        //    using (var client = new HttpClient())
        //    {
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        // Add Headers
        //        content.Headers.Clear();
        //        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        //        client.DefaultRequestHeaders.Clear();
        //        client.DefaultRequestHeaders.Add("Authorization", "Basic UmVsaWFuY2VXQUJBOjhVMU5x");

        //        try
        //        {
        //            var response = await client.PostAsync(apiUrl, content);
        //            string result = await response.Content.ReadAsStringAsync();

        //            System.Diagnostics.Debug.WriteLine("WhatsApp Msg Response: " + result);
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"WhatsApp API Error: {ex.Message}");
        //        }
        //    }
        //}
        //=========================

        //private async Task SendWhatsAppMessageAsync(string Contact_Number, string source, string name)
        //{
        //    var url = "https://kapi.omni-channel.in/fe/api/v1/iPMessage/One2Many";

        //    var payload = new
        //    {
        //        mode = "waba",
        //        wabaPhoneNumber = "+919027586766",
        //        wabaTemplateId = "385610",
        //        campId = "66012",
        //        unicode = false,
        //        shortMessages = new[]
        //        {
        //    new {
        //        recipient = Contact_Number,
        //        corelationId = "1234667",
        //        context = new {
        //            waba_var1 = source,  // old 2nd parameter
        //            waba_var2 = name     // old 3rd parameter
        //        }
        //    }
        //}
        //    };

        //    string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Clear();
        //        client.DefaultRequestHeaders.Add("Authorization", "Basic UmVsaWFuY2VXQUJBOjhVMU5x");

        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        try
        //        {
        //            var response = await client.PostAsync(url, content);
        //            string result = await response.Content.ReadAsStringAsync();

        //            System.Diagnostics.Debug.WriteLine("WhatsApp Response: " + result);
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Diagnostics.Debug.WriteLine("WhatsApp Error: " + ex.Message);
        //        }
        //    }
        //}




        //private async Task SendWhatsAppMessageAsync(string mobileNumber, string source, string name)
        //{

        //    string envelope = $@"
        //       <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
        //                  xmlns:tem=""http://tempuri.org/"" 
        //                  xmlns:gup=""http://schemas.datacontract.org/2004/07/GupShupWhatsappService"">
        //         <soapenv:Body>
        //      <tem:GupShupSendMediaNotification>
        //         <tem:RecipentNumber>{mobileNumber}</tem:RecipentNumber>
        //      <tem:Message>Jan25_InstaNACH1</tem:Message> 
        //     <tem:Source>{source}</tem:Source>
        //         <tem:paraList>
        //            <gup:Paramter1>{name}</gup:Paramter1>
        //         </tem:paraList>
        //         <tem:MediaURL>https://customerdocs.reliancenipponlife.com/WHATSAPP/UploadedFiles/160125_RNLIC_InstaNACH Features_01-01.jpg</tem:MediaURL>
        //         <tem:fileName>160125_RNLIC_InstaNACH Features_01-01.jpg</tem:fileName>
        //         <tem:btnActionURL></tem:btnActionURL>
        //         <tem:Seq1>https://youtu.be/q7mSwl8-4A0</tem:Seq1>
        //         <tem:Seq1Type>CTA</tem:Seq1Type>
        //         <tem:Seq2>https://customer.reliancenipponlife.com/Enach</tem:Seq2>
        //         <tem:Seq2Type>CTA</tem:Seq2Type>
        //         <tem:Seq3></tem:Seq3>
        //         <tem:Seq3Type></tem:Seq3Type>
        //         <tem:Seq4></tem:Seq4>
        //         <tem:Seq4Type></tem:Seq4Type>
        //         <tem:Seq5></tem:Seq5>
        //         <tem:Seq5Type></tem:Seq5Type>
        //         <tem:Seq6></tem:Seq6>
        //         <tem:Seq6Type></tem:Seq6Type>
        //      </tem:GupShupSendMediaNotification>
        //   </soapenv:Body>
        //</soapenv:Envelope>";

        //    using (var client = new HttpClient())
        //    {
        //        var content = new StringContent(envelope, Encoding.UTF8, "text/xml");
        //        content.Headers.Add("SOAPAction", "http://tempuri.org/IGupshupService/GupShupSendMediaNotification");
        //        //content.Headers.Add("Authorization", "Basic UmVsaWFuY2VXQUJBOjhVMU5x");
        //        try
        //        {
        //            //var response = await client.PostAsync("https://kapi.omni-channel.in/fe/api/v1/iPMessage/One2Many", content);
        //             var response = await client.PostAsync("https://hrconnectsit.reliancenipponlife.com/Gupshup/GupshupService.svc", content);
        //            string result = await response.Content.ReadAsStringAsync();
        //            System.Diagnostics.Debug.WriteLine("WhatsApp Msg Response: " + result);
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"WhatsApp API Error: {ex.Message}");
        //        }
        //    }
        //}














        public ActionResult calendarUser()
        {
            SetData();
            // Session["SAPCODE"] = SAPCODE;
            return View();
        }


        public JsonResult DisplayEvent()
        {

            List<CustomModel.JSONEvent> data = new List<CustomModel.JSONEvent>();
            string sapcode = Session["SAPCODE"]?.ToString();

            var query = (from p in db.ENGAGE_SHARECOUNT
                         where p.SHC_SAPCODE == sapcode
                         where p.SHC_DATE != null
                         group p by p.SHC_DATE into g

                         select new
                         {
                             share_date = g.Key,
                             count = g.Count()
                         }).ToList();

            var queryLeads = (from p in db.Leads
                              where p.leads_sapcode == sapcode
                              where p.leads_date != null
                              group p by p.leads_date into g

                              select new
                              {
                                  leads_date = g.Key,
                                  count = g.Count()
                              }).ToList();

            int i = 1;
            foreach (var item in query)
            {
                data.Add(
                new CustomModel.JSONEvent
                {
                    event_id = i.ToString(),
                    title = item.count.ToString(),
                    start = Convert.ToDateTime(item.share_date).ToString("yyyy-MM-dd"),
                    end = Convert.ToDateTime(item.share_date).ToString("yyyy-MM-dd"),
                    color = "#FF0000",
                    url = "#"
                }
            );
                i++;
            }

            foreach (var item in queryLeads)
            {
                data.Add(
                new CustomModel.JSONEvent
                {
                    event_id = i.ToString(),
                    title = item.count.ToString(),
                    start = Convert.ToDateTime(item.leads_date).ToString("yyyy-MM-dd"),
                    end = Convert.ToDateTime(item.leads_date).ToString("yyyy-MM-dd"),
                    color = "#39BDC5",
                    url = "#"
                }
            );
                i++;
            }

            //SetData();
            // Session["SAPCODE"] = SAPCODE;
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetPieChartShares(string fromdate, string todate)
        {
            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);

            string sapcode = Session["SAPCODE"] == null ? "" : Session["SAPCODE"].ToString();

            var query = (from p in db.ENGAGE_SHARECOUNT
                         where p.SHC_SAPCODE == sapcode
                         where p.SHC_DATE >= dt1 && p.SHC_DATE <= dt2
                         group p by p.SHC_PLATEFORM into g

                         select new CustomModel.ModelPieChart
                         {
                             name = g.Key,
                             y = g.Count(),
                             short_name = g.Key,
                             colorIndex = "1"

                         }).ToList();

            int i = 0;
            foreach (var item in query)
            {
                i++;
                query[i - 1].colorIndex = (i).ToString();
            }

            CustomModel.jsonPieChart jp = new CustomModel.jsonPieChart();

            jp.name = "share";
            jp.data = query;

            return Json(jp, JsonRequestBehavior.AllowGet);


        }

        public JsonResult GetPieChartLeads(string fromdate, string todate)
        {
            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);

            string sapcode = Session["SAPCODE"]?.ToString();

            var query = (from p in db.Leads
                         where p.leads_sapcode == sapcode
                         where p.leads_date >= dt1 && p.leads_date <= dt2
                         group p by p.leads_plateform into g

                         select new CustomModel.ModelPieChart
                         {
                             name = g.Key,
                             y = g.Count(),
                             short_name = g.Key,
                             colorIndex = "1"

                         }).ToList();

            int i = 0;
            foreach (var item in query)
            {
                i++;
                query[i - 1].colorIndex = (i).ToString();
            }

            CustomModel.jsonPieChart jp = new CustomModel.jsonPieChart();

            jp.name = "Leads";
            jp.data = query;

            return Json(jp, JsonRequestBehavior.AllowGet);


        }

        public ActionResult Privacypolicy()
        {

            return View();


        }

        [HttpGet]
        public ActionResult Landingpage(string PARAMS)
        {
            Lead model = new Lead();

            if (!string.IsNullOrEmpty(PARAMS))
            {
                string decodedParams = Encoding.UTF8.GetString(Convert.FromBase64String(PARAMS));
                var values = HttpUtility.ParseQueryString(decodedParams);

                ViewBag.SAPCODE = values["SAPCODE"];
                ViewBag.CREATIVEID = values["CREATIVEID"];
                ViewBag.PLATEFORM = values["PLATEFORM"];

                // Get CampaignMasterId from parameters
                decimal campaignMasterId;
                if (decimal.TryParse(values["CampaignMasterId"], out campaignMasterId))
                {
                    // Fetch campaign details from database
                    var campaign = db.campaign_master
                                   .Where(c => c.campaign_master_id == campaignMasterId)
                                   .FirstOrDefault();

                    if (campaign != null)
                    {
                        // OG basic data
                        ViewBag.CampaignTitle = campaign.campaign_master_creative_caption;
                        ViewBag.CampaignDescription = campaign.campaign_master_description;
                        ViewBag.CampaignTags = campaign.campaign_master_tags;
                        ViewBag.LandingPageUrl = campaign.campaign_master_landing_page;

                        // -------- FIX STARTS HERE --------

                        // ===== Correct base URL (ngrok / live / local) =====
                        string scheme = Request.Headers["X-Forwarded-Proto"] ?? Request.Url.Scheme;
                        string host = Request.Headers["X-Forwarded-Host"] ?? Request.Url.Authority;

                        // IMPORTANT: Application path ( /Digimyin )
                        string appPath = Url.Content("~/").TrimEnd('/');

                        string baseUrl = scheme + "://" + host + appPath;

                        // ===== OG Image =====
                        if (!string.IsNullOrEmpty(campaign.campaign_master_images))
                        {
                            string[] images = campaign.campaign_master_images.Split(',');

                            if (images.Length > 0 && !string.IsNullOrWhiteSpace(images[0]))
                            {
                                string imageName = images[0].Trim();

                                ViewBag.CampaignImageUrl =
                                    baseUrl + "/Content/images/campaign/" + imageName;
                            }
                        }


                        // -------- FIX ENDS HERE --------

                        // Store campaign object if needed
                        ViewBag.Campaign = campaign;
                    }
                }
            }

            return View(model);
        }




        //public ActionResult Landingpage(string SAPCODE, string creativeid)
        //{

        //    return View();


        //}
        //    [HttpPost]
        //public async Task<ActionResult> Landingpage(
        //Lead data,
        //string SAPCODE,
        //string CREATIVEID,
        //string PLATEFORM)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        data.leads_creativeid = Convert.ToDecimal(CREATIVEID);

        //        bool isDuplicate = db.Leads.Any(l =>
        //            l.leads_mobile == data.leads_mobile &&
        //            l.leads_creativeid == data.leads_creativeid);

        //        if (!isDuplicate)
        //        {
        //            // 🔹 LeadSquared API call
        //            string requestId = await TestAPI();

        //            data.leads_date = DateTime.UtcNow.AddMinutes(330);
        //            data.leads_sapcode = SAPCODE;
        //            data.leads_plateform = PLATEFORM;

        //            // 🔥 RequestId save here
        //            data.api_leads_id = requestId;

        //            db.Leads.Add(data);
        //            db.SaveChanges();

        //            TempData["alert"] = "Show";

        //            return RedirectToAction("Landingpage", new
        //            {
        //                SAPCODE = SAPCODE,
        //                CREATIVEID = CREATIVEID,
        //                PLATEFORM = PLATEFORM
        //            });
        //        }
        //        else
        //        {
        //            TempData["alert"] = "Duplicate entry found";
        //        }
        //    }

        //    return View();
        //}

        private bool ValidateLead(
    Lead data,
    decimal creativeId,
    ModelStateDictionary modelState,
    out string errorMessage)
        {
            errorMessage = "";

            // 🔹 Duplicate check
            bool isDuplicate = db.Leads.Any(l =>
                l.leads_mobile == data.leads_mobile &&
                l.leads_creativeid == creativeId);

            if (isDuplicate)
            {
                errorMessage = "Lead with the same mobile number and creative ID already exists.";
                modelState.AddModelError("leads_mobile", errorMessage);
                return false;
            }

            // 🔹 Mobile validation
            if (string.IsNullOrEmpty(data.leads_mobile?.ToString()))
            {
                errorMessage = "Enter Your Mobile Number";
                modelState.AddModelError("leads_mobile", errorMessage);
                return false;
            }

            string mobileRegex = @"^([0-9]{10})$";
            if (!Regex.IsMatch(data.leads_mobile.ToString(), mobileRegex))
            {
                errorMessage = "Enter a valid Mobile Number";
                modelState.AddModelError("leads_mobile", errorMessage);
                return false;
            }

            return true;
        }

        //================================= correct code just abhi running=======================


        [HttpPost]
        public async Task<ActionResult> Landingpage(Lead data, string SAPCODE, string CREATIVEID, string PLATEFORM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["SwalType"] = "error";
                    TempData["SwalMessage"] = "Please fill all required details correctly.";
                    return View(data);
                }

                if (!decimal.TryParse(CREATIVEID, out decimal creativeId))
                {
                    TempData["SwalType"] = "error";
                    TempData["SwalMessage"] = "Invalid campaign. Please try again.";
                    return View(data);
                }

                data.leads_creativeid = creativeId;

                if (!ValidateLead(data, creativeId, ModelState, out string validationMsg))
                {
                    TempData["SwalType"] = "warning";
                    TempData["SwalMessage"] = validationMsg;
                    return View(data);
                }

                // 🔹 Get Hierarchy Data
                var empData = db.NEW_TEMP_HIERARCHY
                    .FirstOrDefault(x =>
                        x.X_BM_EMP_CD == SAPCODE ||
                        x.X_ZM_EMP_CD == SAPCODE ||
                        x.X_RM_EMP_EMP_CD == SAPCODE ||
                        x.X_SM_EMP_CD == SAPCODE);

                if (empData == null)
                {
                    TempData["SwalType"] = "error";
                    TempData["SwalMessage"] = "Invalid SAP Code.";
                    return View(data);
                }

                string sapName = "";
                string channelCode = empData.X_CHANNEL?.Trim().ToUpper();

                if (empData.X_BM_EMP_CD == SAPCODE)
                    sapName = empData.X_BM_NM;
                else if (empData.X_ZM_EMP_CD == SAPCODE)
                    sapName = empData.X_ZM_NM;
                else if (empData.X_RM_EMP_EMP_CD == SAPCODE)
                    sapName = empData.X_RM_NM;
                else if (empData.X_SM_EMP_CD == SAPCODE)
                    sapName = empData.X_SM_NM;

                if (string.IsNullOrEmpty(channelCode))
                {
                    TempData["SwalType"] = "error";
                    TempData["SwalMessage"] = "Channel not found.";
                    return View(data);
                }

                // 🔹 Fetch Category Name
                var categoryName = (from cm in db.campaign_master
                                    join cc in db.campaign_category
                                    on cm.campaign_category_id equals cc.campaign_category_id
                                    where cm.channel_code == channelCode
                                    select cc.campaign_category_name)
                                    .FirstOrDefault();

                if (string.IsNullOrEmpty(categoryName))
                {
                    TempData["SwalType"] = "error";
                    TempData["SwalMessage"] = "Category not mapped for this channel.";
                    return View(data);
                }

                var lsqChannels = new List<string> { "AG", "DB", "DL", "DP", "PC", "PM" };
                var seChannels = new List<string> { "CM", "CN", "GR", "NV", "ST", "CD" };

                string apiRequestId = null;
                string rawJson = "";

                // 🔥 Channel Wise API Call
                if (lsqChannels.Contains(channelCode))
                {
                    var result = await TestAPI(
                        data.leads_name,
                        data.leads_mobile.ToString(),
                        SAPCODE,
                        sapName);

                    apiRequestId = result.Response?.RequestId;
                    rawJson = result.RawJson;
                }
                else if (seChannels.Contains(channelCode))
                {
                    var result = await TestAPI_SE(
                        data.leads_name,
                        data.leads_mobile.ToString(),
                        SAPCODE,
                        sapName,
                        categoryName);

                    apiRequestId = result.Response?.RequestId;
                    rawJson = null;
                }
                else
                {
                    TempData["SwalType"] = "error";
                    TempData["SwalMessage"] = "Channel not mapped properly.";
                    return View(data);
                }

                // 🔹 Save Data
                data.leads_date = DateTime.UtcNow.AddMinutes(330);
                data.leads_sapcode = SAPCODE;
                data.leads_plateform = PLATEFORM;

                data.api_leads_id = apiRequestId;          // null allowed
                data.api_response_json = rawJson;         // empty allowed

                data.LeadType = categoryName;             // 🔥 Campaign Category Name Save Here

                db.Leads.Add(data);
                db.SaveChanges();

                TempData["SwalType"] = "success";
                TempData["SwalMessage"] = "Thank you! Your details have been submitted successfully.";

                return RedirectToAction("Landingpage", new
                {
                    SAPCODE,
                    CREATIVEID,
                    PLATEFORM
                });
            }
            catch (Exception ex)
            {
                TempData["SwalType"] = "error";
                TempData["SwalMessage"] = "Something went wrong. Please try again.";
                return View(data);
            }
        }









        //=================================kamlesh====
        //[HttpPost]
        //public async Task<ActionResult> Landingpage(Lead data, string SAPCODE, string CREATIVEID, string PLATEFORM)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        TempData["SwalType"] = "error";
        //        TempData["SwalMessage"] = "Please fill all required details correctly.";
        //        return View(data);
        //    }

        //    if (!decimal.TryParse(CREATIVEID, out decimal creativeId))
        //    {
        //        TempData["SwalType"] = "error";
        //        TempData["SwalMessage"] = "Invalid campaign. Please try again.";
        //        return View(data);
        //    }

        //    data.leads_creativeid = creativeId;

        //    if (!ValidateLead(data, creativeId, ModelState, out string validationMsg))
        //    {
        //        TempData["SwalType"] = "warning";
        //        TempData["SwalMessage"] = validationMsg;
        //        return View(data);
        //    }

        //    // 🔹 Get Employee Data
        //    var empData = db.NEW_TEMP_HIERARCHY
        //        .FirstOrDefault(x =>
        //            x.X_BM_EMP_CD == SAPCODE ||
        //            x.X_ZM_EMP_CD == SAPCODE ||
        //            x.X_RM_EMP_EMP_CD == SAPCODE ||
        //            x.X_SM_EMP_CD == SAPCODE);
        //    string channelCode = "";
        //    string sapName = "";

        //    if (empData != null)
        //    {
        //        // 🔹 Set SAP Name
        //        if (empData.X_BM_EMP_CD == SAPCODE)
        //            sapName = empData.X_BM_NM;

        //        else if (empData.X_ZM_EMP_CD == SAPCODE)
        //            sapName = empData.X_ZM_NM;

        //        else if (empData.X_RM_EMP_EMP_CD == SAPCODE)
        //            sapName = empData.X_RM_NM;

        //        else if (empData.X_SM_EMP_CD == SAPCODE)
        //            sapName = empData.X_SM_NM;

        //        // 🔹 FIXED CHANNEL CODE LINE
        //        channelCode = (empData.X_CHANNEL ?? "").Trim().ToUpper();
        //    }


        //    //string sapName = "";
        //    //string channelCode = "";

        //    //if (empData != null)
        //    //{
        //    //    if (empData.X_BM_EMP_CD == SAPCODE)
        //    //    {
        //    //        sapName = empData.X_BM_NM;
        //    //        channelCode = empData.CHANNEL_CODE;
        //    //    }
        //    //    else if (empData.X_ZM_EMP_CD == SAPCODE)
        //    //    {
        //    //        sapName = empData.X_ZM_NM;
        //    //        channelCode = empData.CHANNEL_CODE;
        //    //    }
        //    //    else if (empData.X_RM_EMP_EMP_CD == SAPCODE)
        //    //    {
        //    //        sapName = empData.X_RM_NM;
        //    //        channelCode = empData.CHANNEL_CODE;
        //    //    }
        //    //    else if (empData.X_SM_EMP_CD == SAPCODE)
        //    //    {
        //    //        sapName = empData.X_SM_NM;
        //    //        channelCode = empData.CHANNEL_CODE;
        //    //    }
        //    //}

        //    ViewBag.LeadName = data.leads_name;
        //    ViewBag.LeadMobile = data.leads_mobile;
        //    ViewBag.LeadSapCode = SAPCODE;
        //    ViewBag.SapName = sapName;

        //    string rawApiResponse = "";

        //    // ✅ SPECIAL CHANNEL CONDITION
        //    var specialChannels = new List<string> { "CM", "CN", "GR", "NV", "ST", "CD" ,"AG" };

        //    if (specialChannels.Contains(channelCode))
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Add("x-client-id", "22a0b4e5-940f-40a4-984d-6af1ac8e8c9e");
        //            client.DefaultRequestHeaders.Add("origin", "https://sa3dev.reliancenipponlife.com");

        //            var requestBody = new
        //            {
        //                userId = SAPCODE,
        //                name = data.leads_name,
        //                phoneNo = data.leads_mobile.ToString(),
        //                apiSource = "digimyin",
        //                loginType = "Individual",
        //                leadType = data.LeadType,
        //            };

        //            var content = new StringContent(
        //                Newtonsoft.Json.JsonConvert.SerializeObject(requestBody),
        //                Encoding.UTF8,
        //                "application/json");

        //            var response = await client.PostAsync(
        //                "https://sa3dev.reliancenipponlife.com/v1.0/nlms/push-leads",
        //                content);

        //            rawApiResponse = await response.Content.ReadAsStringAsync();
        //        }

        //        // ✅ Save API response in leads_email column
        //        data.leads_email = rawApiResponse;
        //    }
        //    else
        //    {
        //        // 🔹 Existing API
        //        var apiResult = await TestAPI(
        //            data.leads_name,
        //            data.leads_mobile.ToString(),
        //            SAPCODE,
        //            sapName);

        //        if (apiResult.Response == null)
        //        {
        //            TempData["SwalType"] = "error";
        //            TempData["SwalMessage"] = "Unable to process your request right now. Please try again later.";
        //            return View(data);
        //        }

        //        data.api_leads_id = apiResult.Response.RequestId;
        //        data.api_response_json = apiResult.RawJson;
        //    }

        //    data.leads_date = DateTime.UtcNow.AddMinutes(330);
        //    data.leads_sapcode = SAPCODE;
        //    data.leads_plateform = PLATEFORM;

        //    db.Leads.Add(data);
        //    db.SaveChanges();

        //    TempData["SwalType"] = "success";
        //    TempData["SwalMessage"] = "Thank you! Your details have been submitted successfully.";

        //    return RedirectToAction("Landingpage", new
        //    {
        //        SAPCODE,
        //        CREATIVEID,
        //        PLATEFORM
        //    });
        //}



        //=================================================


        public ActionResult LandingPageList()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.lstLandingpage = db.landingpages.Where(x => x.landingpage_delflag == null).ToList();
                return View();
            }
        }

        [HttpPost]
        public ActionResult LandingPageList(landingpage data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.lstLandingpage = db.landingpages.Where(x => x.landingpage_delflag == null).ToList();
                db.landingpages.Add(data);
                if (db.SaveChanges() > 0)
                {
                    return RedirectToAction("LandingPageList");
                }
                else
                {
                    return View();
                }
            }
        }


        public ActionResult EditLandingPage(decimal LandingPageId)
        {

            landingpage model = new landingpage();

            if (LandingPageId > 0)
            {

                landingpage cat = db.landingpages.SingleOrDefault(x => x.landingpage_id == LandingPageId && x.landingpage_delflag == null);
                model.landingpage_id = cat.landingpage_id;

                model.landingpage_url = cat.landingpage_url;

            }
            return PartialView("_PartialEditLandingPage", model);
        }

        [HttpPost]
        public ActionResult EditLandingPage(landingpage data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                try
                {
                    if (data.landingpage_id > 0)
                    {
                        //update
                        landingpage cat = db.landingpages.SingleOrDefault(x => x.landingpage_id == data.landingpage_id && x.landingpage_delflag == null);

                        cat.landingpage_url = data.landingpage_url;
                        db.SaveChanges();


                    }
                    //return View("CampaignCategoryList");
                    return RedirectToAction("LandingPageList");
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }












        [HttpPost]
        public ActionResult DeleteLandingPageList(decimal landingpagesId)
        {
            try
            {
                var landingPage = db.landingpages
                    .SingleOrDefault(x => x.landingpage_id == landingpagesId && x.landingpage_delflag == null);

                if (landingPage != null)
                {
                    db.landingpages.Remove(landingPage); // Permanently delete
                    db.SaveChanges();

                    return Json(new { success = true, message = "Landing page deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Landing page not found or already deleted." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }





        [HttpPost]
        public ActionResult ShareCount(string sapcode, string plateform, int creative_id)
        {
            if (sapcode != null && plateform != null && creative_id > 0)
            {
                ENGAGE_SHARECOUNT sc = new ENGAGE_SHARECOUNT();

                //sc = db.ENGAGE_SHARECOUNT.Where(x => x.SHC_SAPCODE == sapcode && x.SHC_PLATEFORM == plateform).First();
                //if (sc != null)
                //{
                //    decimal count = 0;
                //    count = Convert.ToDecimal(sc.SHC_SHARECOUNT) + 1;
                //    sc.SHC_SHARECOUNT = count;
                //}
                //else
                //{
                sc.SHC_PLATEFORM = plateform;
                sc.SHC_SAPCODE = sapcode;
                sc.SHC_SHARECOUNT = 1;
                sc.CREATIVE_ID = creative_id;
                sc.SHC_DATE = System.DateTime.UtcNow.AddMinutes(330);
                db.ENGAGE_SHARECOUNT.Add(sc);
                //}




                if (db.SaveChanges() > 0)
                {
                    return Json("Success" + sc.SHC_ID, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Failed", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Failed", JsonRequestBehavior.AllowGet);
            }
        }
        //public ActionResult UserLeaderBoard()
        //{
        //    try
        //    {
        //        string sapCode = Session["SAPCODE"]?.ToString();

        //        if (string.IsNullOrEmpty(sapCode))
        //        {
        //            ViewBag.Error = "User session not found";
        //            return View();
        //        }

        //        // 🔹 Step 1: Fetch Top 10 from ENGAGE_SHARECOUNT
        //        var qry = db.Database.SqlQuery<ENGAGE_SHARECOUNT>(@"
        //    SELECT TOP 10 *
        //    FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS SHC_ID,
        //            CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS SHC_SHARECOUNT,
        //            SHC_SAPCODE,
        //            '' AS SHC_PLATEFORM,
        //            CAST('2022-01-01' AS DATE) AS SHC_DATE,
        //            CREATIVE_ID
        //        FROM ENGAGE_SHARECOUNT
        //        GROUP BY SHC_SAPCODE, CREATIVE_ID
        //    ) A
        //").ToList();

        //        // 🔹 Step 2: Current user's rank
        //        var qry2 = db.Database.SqlQuery<ENGAGE_SHARECOUNT>(@"
        //    SELECT * 
        //    FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(SHC_SAPCODE) DESC) AS NUMERIC(18,0)) AS SHC_ID,
        //            CAST(COUNT(SHC_SAPCODE) AS NUMERIC(18,0)) AS SHC_SHARECOUNT,
        //            SHC_SAPCODE,
        //            '' AS SHC_PLATEFORM,
        //            CAST('2022-01-01' AS DATE) AS SHC_DATE,
        //            CREATIVE_ID
        //        FROM ENGAGE_SHARECOUNT
        //        GROUP BY SHC_SAPCODE, CREATIVE_ID
        //    ) A 
        //    WHERE A.SHC_SAPCODE = @SAPCODE",
        //            new SqlParameter("@SAPCODE", sapCode)
        //        ).FirstOrDefault();

        //        // 🔹 Step 3: Load hierarchy table and map names
        //        var allHierarchy = db.NEW_TEMP_HIERARCHY.ToList();

        //        var shareWithNames = qry.Select(x => new
        //        {
        //            x.SHC_ID,
        //            x.SHC_SHARECOUNT,
        //            x.SHC_SAPCODE,
        //            Name = allHierarchy.FirstOrDefault(h =>
        //                (!string.IsNullOrEmpty(h.X_ZM_EMP_CD) && h.X_ZM_EMP_CD == x.SHC_SAPCODE) ||
        //                (!string.IsNullOrEmpty(h.X_RM_EMP_EMP_CD) && h.X_RM_EMP_EMP_CD == x.SHC_SAPCODE) ||
        //                (!string.IsNullOrEmpty(h.X_BM_EMP_CD) && h.X_BM_EMP_CD == x.SHC_SAPCODE) ||
        //                (!string.IsNullOrEmpty(h.X_SM_EMP_CD) && h.X_SM_EMP_CD == x.SHC_SAPCODE) ||
        //                (!string.IsNullOrEmpty(h.AGENT_CODE) && h.AGENT_CODE == x.SHC_SAPCODE)
        //            )?.X_SM_NM ?? "N/A"
        //        }).ToList();

        //        var myRankWithName = qry2 != null ? new
        //        {
        //            qry2.SHC_ID,
        //            qry2.SHC_SHARECOUNT,
        //            qry2.SHC_SAPCODE,
        //            Name = allHierarchy.FirstOrDefault(h =>
        //                (!string.IsNullOrEmpty(h.X_ZM_EMP_CD) && h.X_ZM_EMP_CD == qry2.SHC_SAPCODE) ||
        //                (!string.IsNullOrEmpty(h.X_RM_EMP_EMP_CD) && h.X_RM_EMP_EMP_CD == qry2.SHC_SAPCODE) ||
        //                (!string.IsNullOrEmpty(h.X_BM_EMP_CD) && h.X_BM_EMP_CD == qry2.SHC_SAPCODE) ||
        //                (!string.IsNullOrEmpty(h.X_SM_EMP_CD) && h.X_SM_EMP_CD == qry2.SHC_SAPCODE) ||
        //                (!string.IsNullOrEmpty(h.AGENT_CODE) && h.AGENT_CODE == qry2.SHC_SAPCODE)
        //            )?.X_SM_NM ?? "N/A"
        //        } : null;

        //        ViewBag.ShareLeaderBoard = shareWithNames;
        //        ViewBag.ShareLeaderBoardMyRank = myRankWithName;

        //        // 🔹 Step 4: Leads Leaderboard - FIXED QUERY
        //        var qry3 = db.Database.SqlQuery<Lead>(@"
        //    SELECT TOP 10 *
        //    FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS leads_id,
        //            leads_SAPCODE AS leads_sapcode,
        //            '' AS LEADS_NAME,
        //            CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE,
        //            CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID,
        //            CAST(GETDATE() AS DATETIME) AS LEADS_DATE,
        //            '' AS LEADS_PLATEFORM,
        //            CAST('' AS VARCHAR(100)) AS leads_email,
        //            CAST(0 AS NUMERIC(18,0)) AS api_leads_id
        //        FROM leads
        //        WHERE leads_SAPCODE IS NOT NULL
        //        GROUP BY leads_SAPCODE
        //    ) A
        //").ToList();

        //        var qry4 = db.Database.SqlQuery<Lead>(@"
        //    SELECT * 
        //    FROM (
        //        SELECT 
        //            CAST(ROW_NUMBER() OVER(ORDER BY COUNT(leads_id) DESC) AS NUMERIC(18,0)) AS leads_id,
        //            leads_SAPCODE AS leads_sapcode,
        //            '' AS LEADS_NAME,
        //            CAST(0 AS NUMERIC(18,0)) AS LEADS_MOBILE,
        //            CAST(0 AS NUMERIC(18,0)) AS LEADS_CREATIVEID,
        //            CAST(GETDATE() AS DATETIME) AS LEADS_DATE,
        //            '' AS LEADS_PLATEFORM,
        //            CAST('' AS VARCHAR(100)) AS leads_email,
        //            CAST(0 AS NUMERIC(18,0)) AS api_leads_id
        //        FROM leads
        //        WHERE leads_SAPCODE IS NOT NULL
        //        GROUP BY leads_SAPCODE
        //    ) A 
        //    WHERE A.leads_sapcode = @SAPCODE",
        //            new SqlParameter("@SAPCODE", sapCode)
        //        ).FirstOrDefault();

        //        ViewBag.LeadLeaderBoard = qry3;
        //        ViewBag.LeadLeaderBoardMyRank = qry4;

        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        // Logger.Error(ex, "Error in UserLeaderBoard action");

        //        ViewBag.Error = "An error occurred while loading the leaderboard";
        //        return View();
        //    }
        //}





        //==========================================


        public ActionResult UserLeaderBoard()
        {
            string Ref = Session["REF_KEY"]?.ToString();
            string sapCode = string.Empty;

            if (!string.IsNullOrEmpty(Ref))
            {
                var valueBytes = Convert.FromBase64String(Ref);
                string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

                string[] separate_params = str_REFKEY.Split('&');
                foreach (var item in separate_params)
                {
                    if (item.Contains("SAPCODE"))
                        sapCode = item.Split('=')[1];
                }
            }

            Session["SAPCODE"] = sapCode;

            db.Database.CommandTimeout = 180;

            /* ================= LOAD HIERARCHY ONCE ================= */

            var hierarchy = db.NEW_TEMP_HIERARCHY
                .Select(x => new {
                    x.AGENT_CODE,
                    x.AGENT_NAME,
                    x.X_BM_EMP_CD,
                    x.X_BM_NM,
                    x.X_ZM_EMP_CD,
                    x.X_ZM_NM,
                    x.X_RM_EMP_EMP_CD,
                    x.X_RM_NM,
                    x.X_SM_EMP_CD,
                    x.X_SM_NM
                })
                .ToList();


            /* ================= SHARE LEADERBOARD ================= */

            var topShares = db.Database.SqlQuery<ENGAGE_SHARECOUNT>(@"
        SELECT TOP 10
            CONVERT(DECIMAL(18,0), ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC)) AS SHC_ID,
            CONVERT(DECIMAL(18,0), COUNT(*)) AS SHC_SHARECOUNT,
            SHC_SAPCODE,
            '' AS SHC_PLATEFORM,
            CONVERT(DECIMAL(18,0),0) AS CREATIVE_ID,
            GETDATE() AS SHC_DATE
        FROM ENGAGE_SHARECOUNT
        GROUP BY SHC_SAPCODE
        ORDER BY COUNT(*) DESC
    ").ToList();

            foreach (var item in topShares)
            {
                var code = item.SHC_SAPCODE?.Trim();

                var emp = hierarchy.FirstOrDefault(h =>
                    (h.AGENT_CODE != null && h.AGENT_CODE.Trim() == code) ||
                    (h.X_BM_EMP_CD != null && h.X_BM_EMP_CD.Trim() == code) ||
                    (h.X_ZM_EMP_CD != null && h.X_ZM_EMP_CD.Trim() == code) ||
                    (h.X_RM_EMP_EMP_CD != null && h.X_RM_EMP_EMP_CD.Trim() == code) ||
                    (h.X_SM_EMP_CD != null && h.X_SM_EMP_CD.Trim() == code)
                );

                if (emp != null)
                {
                    if (emp.AGENT_CODE?.Trim() == code)
                        item.SHC_PLATEFORM = emp.AGENT_NAME;
                    else if (emp.X_BM_EMP_CD?.Trim() == code)
                        item.SHC_PLATEFORM = emp.X_BM_NM;
                    else if (emp.X_ZM_EMP_CD?.Trim() == code)
                        item.SHC_PLATEFORM = emp.X_ZM_NM;
                    else if (emp.X_RM_EMP_EMP_CD?.Trim() == code)
                        item.SHC_PLATEFORM = emp.X_RM_NM;
                    else if (emp.X_SM_EMP_CD?.Trim() == code)
                        item.SHC_PLATEFORM = emp.X_SM_NM;
                }
                else
                {
                    item.SHC_PLATEFORM = code;
                }
            }

            ViewBag.ShareLeaderBoard = topShares;
            ViewBag.ShareLeaderBoardMyRank =
                topShares.FirstOrDefault(x => x.SHC_SAPCODE == sapCode);


            /* ================= LEADS LEADERBOARD ================= */

            var topLeads = db.Database.SqlQuery<Lead>(@"
        SELECT TOP 10
            CONVERT(DECIMAL(18,0), COUNT(*)) AS leads_id,
            leads_SAPCODE AS leads_sapcode,
            '' AS leads_name,
            CONVERT(DECIMAL(18,0),0) AS LEADS_MOBILE,
            CONVERT(DECIMAL(18,0),0) AS LEADS_CREATIVEID,
            GETDATE() AS LEADS_DATE,
            '' AS LEADS_PLATEFORM,
            '' AS leads_email,
            '' AS api_leads_id,
            '' AS api_response_json,
            '' AS LeadType
        FROM leads
        GROUP BY leads_SAPCODE
        ORDER BY COUNT(*) DESC
    ").ToList();

            foreach (var item in topLeads)
            {
                var code = item.leads_sapcode?.Trim();

                var emp = hierarchy.FirstOrDefault(h =>
                    (h.AGENT_CODE != null && h.AGENT_CODE.Trim() == code) ||
                    (h.X_BM_EMP_CD != null && h.X_BM_EMP_CD.Trim() == code) ||
                    (h.X_ZM_EMP_CD != null && h.X_ZM_EMP_CD.Trim() == code) ||
                    (h.X_RM_EMP_EMP_CD != null && h.X_RM_EMP_EMP_CD.Trim() == code) ||
                    (h.X_SM_EMP_CD != null && h.X_SM_EMP_CD.Trim() == code)
                );

                if (emp != null)
                {
                    if (emp.AGENT_CODE?.Trim() == code)
                        item.leads_name = emp.AGENT_NAME;
                    else if (emp.X_BM_EMP_CD?.Trim() == code)
                        item.leads_name = emp.X_BM_NM;
                    else if (emp.X_ZM_EMP_CD?.Trim() == code)
                        item.leads_name = emp.X_ZM_NM;
                    else if (emp.X_RM_EMP_EMP_CD?.Trim() == code)
                        item.leads_name = emp.X_RM_NM;
                    else if (emp.X_SM_EMP_CD?.Trim() == code)
                        item.leads_name = emp.X_SM_NM;
                }
                else
                {
                    item.leads_name = "NA";   // 👈 yaha change kiya
                }
            }

            ViewBag.LeadLeaderBoard = topLeads;
            ViewBag.LeadLeaderBoardMyRank =
                topLeads.FirstOrDefault(x => x.leads_sapcode == sapCode);

            return View();
        }


        //=========================================
        /****START - CHANNEL ****/
        //public ActionResult ChannelList()
        //{
        //    if (Session["userid"] == null)
        //    {
        //        return RedirectToAction("Index");
        //   

        //    // Channels list Alphabetical by SubChannel Name
        //    ViewBag.lstChannel = db.channels
        //                           .Where(x => x.channel_delflag == null)
        //                           .OrderBy(x => x.subchannel_name)   // ✅ अब subchannel_name से order
        //                           .ToList();

        //    // NEW_TEMP_HIERARCHY drop-down (no order requirement from you)
        //    ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
        //                               .Select(x => new SelectListItem
        //                               {
        //                                   Value = x.X_CHANNEL,
        //                                   Text = x.X_CHANNEL
        //                               })
        //                               .Distinct()
        //                               .ToList();

        //    return View();
        //}

        //[HttpPost]
        //public ActionResult ChannelList(channel data)
        //{
        //    if (Session["userid"] == null)
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    // ✅ Check duplicate channel name (case-insensitive + trim)
        //    bool isExist = db.channels
        //                     .Any(x => x.channel_name.ToLower().Trim() == data.channel_name.ToLower().Trim()
        //                               && x.channel_delflag == null);

        //    if (isExist)
        //    {
        //        // ✅ Duplicate मिला → fixed alert message
        //        TempData["DuplicateMsg"] = "Channel name already save in table";

        //        // Reload dropdowns again
        //        ViewBag.lstChannel = db.channels
        //            .Where(x => x.channel_delflag == null)
        //            .OrderBy(x => x.subchannel_name)   // ✅ subchannel_name से order
        //            .ToList();

        //        ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
        //            .Select(x => new SelectListItem
        //            {
        //                Value = x.X_CHANNEL,
        //                Text = x.X_CHANNEL
        //            })
        //            .Distinct()
        //            .ToList();

        //        return View(data);
        //    }

        //    // Agar duplicate nahi hai → Save karo
        //    db.channels.Add(data);
        //    if (db.SaveChanges() > 0)
        //    {
        //        return RedirectToAction("ChannelList");
        //    }

        //    // Save fail case → reload view
        //    ViewBag.lstChannel = db.channels
        //        .Where(x => x.channel_delflag == null)
        //        .OrderBy(x => x.subchannel_name)   // ✅ subchannel_name से order
        //        .ToList();

        //    ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
        //        .Select(x => new SelectListItem
        //        {
        //            Value = x.X_CHANNEL,
        //            Text = x.X_CHANNEL
        //        })
        //        .Distinct()
        //        .ToList();

        //    return View(data);
        //}

        //============================correct code ==============

        public ActionResult ChannelList()


        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Channels list ko alphabetically order by subchannel_name
            ViewBag.lstChannel = db.channels
                                   .Where(x => x.channel_delflag == null)
                                   .OrderBy(x => x.subchannel_name)  // <-- alphabetical order
                                   .ToList();

            // NEW_TEMP_HIERARCHY drop-down binding
            ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
                                       .Select(x => new SelectListItem
                                       {
                                           Value = x.X_CHANNEL,
                                           Text = x.X_CHANNEL
                                       })
                                       .Distinct()
                                       .OrderBy(x => x.Text)  // dropdown bhi alphabet me
                                       .ToList();

            //return View();
            return View(new channel());

        }


        [HttpPost]
        public ActionResult ChannelList(channel data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Add new channel
            db.channels.Add(data);
            if (db.SaveChanges() > 0)
            {
                return RedirectToAction("ChannelList");
            }

            // If save fails, reload the view with dropdown data

            // Channels Alphabetical A → Z
            ViewBag.lstChannel = db.channels
                .Where(x => x.channel_delflag == null)
                .OrderBy(x => x.channel_name)
                .ToList();

            // NEW_TEMP_HIERARCHY Alphabetical A → Z
            ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
                .Select(x => new SelectListItem
                {
                    Value = x.X_CHANNEL,
                    Text = x.X_CHANNEL
                })
                .Distinct()
                .OrderBy(x => x.Text)
                .ToList();

            return View(data);
        }

        [HttpGet]
        public JsonResult GetChannelDesc(string channel)
        {
            var desc = db.NEW_TEMP_HIERARCHY
                .Where(x => x.X_CHANNEL == channel)
                .Select(x => x.X_CHANNEL_DESC)
                .FirstOrDefault();

            return Json(desc, JsonRequestBehavior.AllowGet);
        }






        //============================================================================
        //public ActionResult ChannelList()
        //{
        //    if (Session["userid"] == null)
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    // Channels list ko alphabetically order by subchannel_name
        //    ViewBag.lstChannel = db.channels
        //                           .Where(x => x.channel_delflag == null)
        //                           .OrderBy(x => x.subchannel_name)  // <-- alphabetical order
        //                           .ToList();

        //    // NEW_TEMP_HIERARCHY drop-down binding
        //    ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
        //                               .Select(x => new SelectListItem
        //                               {
        //                                   Value = x.X_CHANNEL,
        //                                   Text = x.X_CHANNEL
        //                               })
        //                               .Distinct()
        //                               //.OrderBy(x => x.Text)  // dropdown bhi alphabet me
        //                               .ToList();

        //    return View();
        //}


        //[HttpPost]
        //public ActionResult ChannelList(channel data)
        //{
        //    if (Session["userid"] == null)
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    // Add new channel
        //    db.channels.Add(data);
        //    if (db.SaveChanges() > 0)
        //    {
        //        return RedirectToAction("ChannelList");
        //    }

        //    // If save fails, reload the view with dropdown data

        //    // Channels Alphabetical A → Z
        //    ViewBag.lstChannel = db.channels
        //        .Where(x => x.channel_delflag == null)
        //        .OrderBy(x => x.channel_name)
        //        .ToList();

        //    // NEW_TEMP_HIERARCHY Alphabetical A → Z
        //    ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
        //        .Select(x => new SelectListItem
        //        {
        //            Value = x.X_CHANNEL,
        //            Text = x.X_CHANNEL
        //        })
        //        .Distinct()
        //        //.OrderBy(x => x.Text)
        //        .ToList();

        //    return View(data);
        //}

        //***********************************************


        [HttpPost]
        public ActionResult DeleteChannel(decimal ChannelId)
        {
            try
            {
                var channel = db.channels
                    .SingleOrDefault(x => x.channel_id == ChannelId && x.channel_delflag == null);

                if (channel != null)
                {
                    db.channels.Remove(channel); // Permanently delete
                    db.SaveChanges();
                    return Json(new { success = true, message = "Channel deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Channel not found or already deleted." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred: " + ex.Message });
            }
        }


        public ActionResult EditChannel(decimal ChannelId)
        {
            channel model = new channel();

            if (ChannelId > 0)
            {
                var cat = db.channels.SingleOrDefault(x => x.channel_id == ChannelId && x.channel_delflag == null);
                if (cat != null)
                {
                    model.channel_id = cat.channel_id;
                    model.subchannel_name = cat.subchannel_name; // Ensure this property exists in the model
                    model.channel_name = cat.channel_name;
                }
            }

            // Load dropdown data for edit view using X_CHANNEL instead of SUBCHANNEL
            ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
                .Select(x => new SelectListItem
                {
                    Value = x.X_CHANNEL,
                    Text = x.X_CHANNEL
                })
                .Distinct()
                 .OrderBy(x => x.Text)
                .ToList();

            return PartialView("_PartialChannelEdit", model);
        }

        [HttpPost]
        public ActionResult EditChannel(channel data)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                if (data.channel_id > 0)
                {
                    var cat = db.channels.SingleOrDefault(x => x.channel_id == data.channel_id && x.channel_delflag == null);
                    if (cat != null)
                    {
                        cat.subchannel_name = data.subchannel_name; // Ensure this property exists
                        cat.channel_name = data.channel_name;
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("ChannelList");
            }
            catch (Exception ex)
            {
                // Reload dropdown data in case of error
                ViewBag.NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY
                    .Select(x => new SelectListItem
                    {
                        Value = x.SUBCHANNEL,
                        Text = x.SUBCHANNEL
                    })
                    .Distinct()
                     .OrderBy(x => x.Text)
                    .ToList();
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return PartialView("_PartialChannelEdit", data);
            }
        }

        //===================


        public ActionResult UserTimeLine()
        {
            SetData();

            var qry = db.ENGAGE_SHARECOUNT.SqlQuery("select CAST(count(SHC_ID) AS NUMERIC(18,0)) AS SHC_SHARECOUNT, '0' AS 'SHC_SAPCODE', SHC_DATE as 'SHC_DATE', '' AS 'SHC_PLATEFORM', CAST('0' AS NUMERIC(18, 0)) AS 'SHC_ID'  from ENGAGE_SHARECOUNT where SHC_SAPCODE = @SAPCODE group by SHC_DATE  UNION ALL   select '0' AS SHC_SHARECOUNT,  CAST(count(leads_id) AS VARCHAR(50)) AS SHC_SHARECOUNT,   leads_date as 'SHC_DATE',   '' AS 'SHC_PLATEFORM',  CAST('0' AS NUMERIC(18, 0)) AS 'SHC_ID' from leads where leads_sapcode = @SAPCODE group by leads_date", new SqlParameter("@SAPCODE", Session["SAPCODE"]))
              .ToList<ENGAGE_SHARECOUNT>();

            //var qry2 = db.ENGAGE_SHARECOUNT
            //  .SqlQuery("select count(SHC_id) , SHC_date  from ENGAGE_SHARECOUNT where SHC_SAPCODE = @SAPCODE group by SHC_DATE", new SqlParameter("@SAPCODE", Session["SAPCODE"]))
            //  .ToList<ENGAGE_SHARECOUNT>();
            ViewBag.timeline = qry;



            return View();
        }


        public ActionResult DynamicCalendar()
        {
            return View();
        }


        [ChildActionOnly]
        public ActionResult GetCampaign(decimal cam_cat_id)
        {
            ViewBag.cam_id = cam_cat_id;
            string userChannel = (Session["X_CHANNEL"]?.ToString() ?? "").Trim().ToUpper();
            DateTime dt = System.DateTime.UtcNow.AddMinutes(330);

            var qry = db.campaign_master
                         .Where(x => ((x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt)
                                       || x.campaign_master_start_date > dt)
                                     && x.channel_code.Trim().ToUpper() == userChannel
                                     && x.campaign_category_id == cam_cat_id)
                         .GroupBy(x => x.campaign_id)  // <--- group by campaign_id to remove duplicates
                         .Select(g => g.FirstOrDefault())
                         .OrderByDescending(x => x.campaign_master_id)
                         .ToList();

            ViewBag.lstpcampaign = qry;
            return PartialView("_Campaign");
        }





        //[ChildActionOnly]
        //public ActionResult GetCampaign(decimal cam_cat_id)
        //{
        //    ViewBag.cam_id = cam_cat_id;
        //    // ViewBag.lstpcampaign = db.campaigns.Where(x => x.campaign_delflag == null && x.campaign_category_id == cam_cat_id).ToList();
        //    string userChannel = (Session["X_CHANNEL"]?.ToString() ?? "").Trim().ToUpper();
        //    DateTime dt = System.DateTime.UtcNow.AddMinutes(330);
        //    var qry = db.campaign_master
        //                 .Where(x => ((x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt)
        //                               || x.campaign_master_start_date > dt)
        //                             && x.channel_code.Trim().ToUpper() == userChannel
        //                             && x.campaign_category_id == cam_cat_id
        //                             ) // normalize here
        //                 .OrderByDescending(x => x.campaign_master_id)
        //                 .ToList();
        //    ViewBag.lstpcampaign = qry;
        //    return PartialView("_Campaign");
        //}






        [ChildActionOnly]
        public ActionResult GetCategory(decimal cat_id)
        {
            ViewBag.cat_id = cat_id;
            ViewBag.lstpCategory = db.campaign_category.Where(x => x.campaign_category_delflag == null && x.campaign_category_id == cat_id).ToList();
            ///ViewBag.lstpcampaign = db.campaigns.Where(x => x.campaign_delflag == null && x.campaign_category_id == cat_id).ToList();
            return PartialView("_Category");
        }





        [ChildActionOnly]
        public ActionResult GetCreative(decimal cam_id)
        {
            ViewBag.cam_id = cam_id;
            DateTime dt = System.DateTime.UtcNow.AddMinutes(330);
            string userChannel = (Session["X_CHANNEL"]?.ToString() ?? "").Trim().ToUpper();

            var qry1 = db.campaign_master
                .Where(x =>
                (x.campaign_master_start_date > dt || (x.campaign_master_start_date <= dt && x.campaign_master_end_date >= dt)) &&
                x.campaign_id == cam_id &&
                x.channel_code == userChannel
                ).OrderByDescending(x => x.campaign_master_id).ToList();
            ViewBag.lstpAllCampaigns = qry1;
            return PartialView("_Creative");
        }


        [ChildActionOnly]
        public ActionResult GetUpCampaign(decimal cam_cat_id)
        {
            ViewBag.lstpcampaign = db.campaigns.Where(x => x.campaign_delflag == null && x.campaign_category_id == cam_cat_id).ToList();
            return PartialView("_UpCampaign");
        }
        [ChildActionOnly]
        public ActionResult GetUpCategory(decimal cat_id)
        {
            ViewBag.cat_id = cat_id;
            ViewBag.lstpCategory = db.campaign_category.Where(x => x.campaign_category_delflag == null && x.campaign_category_id == cat_id).ToList();
            ///ViewBag.lstpcampaign = db.campaigns.Where(x => x.campaign_delflag == null && x.campaign_category_id == cat_id).ToList();
            return PartialView("_UpCategory");
        }
        [ChildActionOnly]
        public ActionResult GetUpCreative(decimal cam_id)
        {
            ViewBag.cam_id = cam_id;
            DateTime dt = System.DateTime.UtcNow.AddMinutes(330);
            var qry1 = db.campaign_master.Where(x => x.campaign_master_start_date > dt && x.campaign_id == cam_id).OrderByDescending(x => x.campaign_master_id).ToList();
            ViewBag.lstpAllCampaigns = qry1;
            return PartialView("_UpCreative");
        }

        public ActionResult DateWiseShare()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {


                DateTime dt1 = System.DateTime.UtcNow.AddMinutes(330);
                DateTime dt2 = System.DateTime.UtcNow.AddMinutes(330);

                ViewBag.dt1 = dt1;
                ViewBag.dt2 = dt2;

                ViewBag.lstChannel = (from a in db.NEW_TEMP_HIERARCHY
                                      where a.X_SM_STATUS == "if"
                                      select new
                                      {
                                          channelname = a.X_CHANNEL
                                      }).ToList().Distinct();

                ViewBag.lstZone = (from a in db.NEW_TEMP_HIERARCHY
                                   where a.X_SM_STATUS == "if"
                                   where a.X_ZONE != null
                                   select new
                                   {
                                       zonename = a.X_ZONE
                                   }).ToList().Distinct();




                var sharecount = (from p in db.ENGAGE_SHARECOUNT
                                  where p.SHC_DATE >= dt1 && p.SHC_DATE <= dt2
                                  where p.SHC_DATE != null
                                  group p by new
                                  {

                                      p.SHC_SAPCODE
                                  }
                             into g
                                  select new CustomModel.ViewGroupByShare
                                  {

                                      count = g.Count(),
                                      sapcode = g.Key.SHC_SAPCODE
                                  }).ToList();

                return View(sharecount);
            }
        }


        [HttpPost]
        public ActionResult DateWiseShare(string fromdate, string todate, string channel, string zone)
        {

            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);


            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;
            ViewBag.channel = channel;
            ViewBag.zone = zone;

            ViewBag.lstChannel = (from a in db.NEW_TEMP_HIERARCHY
                                  where a.X_SM_STATUS == "if"
                                  select new
                                  {
                                      channelname = a.X_CHANNEL
                                  }).ToList().Distinct();

            ViewBag.lstZone = (from a in db.NEW_TEMP_HIERARCHY
                               where a.X_SM_STATUS == "if"
                               where a.X_ZONE != null
                               select new
                               {
                                   zonename = a.X_ZONE
                               }).ToList().Distinct();


            var sharecount = (from p in db.ENGAGE_SHARECOUNT
                              where p.SHC_DATE >= dt1 && p.SHC_DATE <= dt2
                              where p.SHC_DATE != null
                              group p by new
                              {
                                  p.SHC_SAPCODE
                              }
                            into g
                              select new CustomModel.ViewGroupByShare
                              {
                                  count = g.Count(),
                                  sapcode = g.Key.SHC_SAPCODE
                              }).ToList();
            return View(sharecount);
        }

        [ChildActionOnly]
        public ActionResult GetLeadRecord(int srno, string fd, string td, string sapcode, string sharecount, string channel, string zone)
        {
            ViewBag.id = srno;
            ViewBag.sharecount = sharecount;
            ViewBag.sapcode = sapcode;
            ViewBag.rqstchannel = channel;
            ViewBag.rqstzone = zone;


            DateTime dt1 = Convert.ToDateTime(fd);
            DateTime dt2 = Convert.ToDateTime(td);



            if (db.NEW_TEMP_HIERARCHY.Where(x => x.X_ZM_EMP_CD == sapcode).FirstOrDefault() != null)
            {
                var NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY.Where(x => x.X_ZM_EMP_CD == sapcode).FirstOrDefault();
                ViewBag.zone = NEW_TEMP_HIERARCHY.X_ZONE;
                ViewBag.channel = NEW_TEMP_HIERARCHY.X_CHANNEL;
            }
            else if (db.NEW_TEMP_HIERARCHY.Where(x => x.X_RM_EMP_EMP_CD == sapcode).FirstOrDefault() != null)
            {
                var NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY.Where(x => x.X_RM_EMP_EMP_CD == sapcode).FirstOrDefault();
                ViewBag.zone = NEW_TEMP_HIERARCHY.X_ZONE;
                ViewBag.channel = NEW_TEMP_HIERARCHY.X_CHANNEL;
            }
            else if (db.NEW_TEMP_HIERARCHY.Where(x => x.X_BM_EMP_CD == sapcode).FirstOrDefault() != null)
            {
                var NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY.Where(x => x.X_BM_EMP_CD == sapcode).FirstOrDefault();
                ViewBag.zone = NEW_TEMP_HIERARCHY.X_ZONE;
                ViewBag.channel = NEW_TEMP_HIERARCHY.X_CHANNEL;
            }
            else if (db.NEW_TEMP_HIERARCHY.Where(x => x.X_SM_EMP_CD == sapcode).FirstOrDefault() != null)
            {
                var NEW_TEMP_HIERARCHY = db.NEW_TEMP_HIERARCHY.Where(x => x.X_SM_EMP_CD == sapcode).FirstOrDefault();
                ViewBag.zone = NEW_TEMP_HIERARCHY.X_ZONE;
                ViewBag.channel = NEW_TEMP_HIERARCHY.X_CHANNEL;
            }
            else
            {
                ViewBag.zone = "-";
                ViewBag.channel = "-";
            }




            ViewBag.lstLeads = (from a in db.Leads
                                where a.leads_sapcode == sapcode
                                where a.leads_date >= dt1 && a.leads_date <= dt2
                                select a).Count();

            return PartialView("_LeadRecords");

        }







        ///----------------------

        public ActionResult LeadCampaign()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Check if subchannel is assigned for Sub-Admin(Read-Only)
            string userRole = Session["role"]?.ToString()?.ToLower();
            if (userRole == "subadmin_readonly" && string.IsNullOrEmpty(Session["subchannel"]?.ToString()))
            {
                TempData["warning"] = "No subchannel assigned for this Sub-Admin!";
                return RedirectToAction("Index");
            }

            DateTime dt1 = DateTime.UtcNow.AddMinutes(330); // IST offset
            DateTime dt2 = DateTime.UtcNow.AddMinutes(330);

            // Set ViewBag.X_CHANNEL based on user role
            ViewBag.X_CHANNEL = userRole == "admin" || userRole == "subadmin_creator"
                ? "" // Admins and Sub-Admin(Creator) see all data
                : Session["subchannel"]?.ToString() ?? ""; // Sub-Admin(Read-Only) filtered by subchannel

            // Fetch dropdowns
            ViewBag.Channels = db.NEW_TEMP_HIERARCHY
                .Select(t => t.X_CHANNEL)
                .Distinct()
                .Where(c => !string.IsNullOrEmpty(c))
                .OrderBy(c => c)
                .ToList();

            //ViewBag.CampaignCategories = db.campaign_category
            //    .Select(c => c.campaign_category_name)
            //    .Distinct()
            //    .OrderBy(c => c)
            //    .ToList();

            ViewBag.CampaignCategories = db.campaign_category
    .Where(c => c.campaign_category_delflag == null && c.Campaign_Category_Status == "0")
    .Select(c => c.campaign_category_name)
    .OrderBy(name => name)
        .Distinct()
    .ToList();






            //ViewBag.SubCampaigns = db.subcampaigns
            //    .Select(s => s.subcampaign_name)
            //    .Distinct()
            //    .OrderBy(s => s)
            //    .ToList();

            ViewBag.SubCampaigns = db.subcampaigns
    .Where(s => s.subcampaign_delflag == null)   // ✔ null check (string column)
    .Select(s => s.subcampaign_name)
    .Distinct()
    .OrderBy(name => name)
    .ToList();


            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;
            ViewBag.SubCampaignName = "";
            ViewBag.CampaignCategoryName = "";

            var lstLeads = GetLeads(dt1, dt2, ViewBag.X_CHANNEL, null, null);
            ViewBag.lstLeads = lstLeads;

            return View();
        }

        [HttpPost]
        public ActionResult LeadCampaign(string fromdate, string todate, string X_CHANNEL, string SubCampaignName, string CampaignCategoryName)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            // Check if subchannel is assigned for Sub-Admin(Read-Only)
            string userRole = Session["role"]?.ToString()?.ToLower();
            if (userRole == "subadmin_readonly" && string.IsNullOrEmpty(Session["subchannel"]?.ToString()))
            {
                TempData["warning"] = "No subchannel assigned for this Sub-Admin!";
                return RedirectToAction("Index");
            }

            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate).AddDays(1).AddTicks(-1);

            // Set ViewBag.X_CHANNEL based on user role
            ViewBag.X_CHANNEL = userRole == "admin" ? (X_CHANNEL ?? "") // Admins can filter by channel
                : userRole == "subadmin_creator" ? "" // Sub-Admin(Creator) ignores subchannel
                : Session["subchannel"]?.ToString() ?? ""; // Sub-Admin(Read-Only) filtered by subchannel

            // Fetch dropdowns
            ViewBag.Channels = db.NEW_TEMP_HIERARCHY
                .Select(t => t.X_CHANNEL)
                .Distinct()
                .Where(c => !string.IsNullOrEmpty(c))
                .OrderBy(c => c)
                .ToList();

            //ViewBag.CampaignCategories = db.campaign_category
            //    .Select(c => c.campaign_category_name)
            //    .Distinct()
            //    .OrderBy(c => c)
            //    .ToList();
            ViewBag.CampaignCategories = db.campaign_category
  .Where(c => c.campaign_category_delflag == null && c.Campaign_Category_Status == "0")
  .Select(c => c.campaign_category_name)
  .OrderBy(name => name)
  .Distinct()
  .ToList();

            //ViewBag.SubCampaigns = db.subcampaigns
            //    .Select(s => s.subcampaign_name)
            //    .Distinct()
            //    .OrderBy(s => s)
            //    .ToList();
            ViewBag.SubCampaigns = db.subcampaigns
    .Where(s => s.subcampaign_delflag == null)   // ✔ null check (string column)
    .Select(s => s.subcampaign_name)
    .Distinct()
    .OrderBy(name => name)
    .ToList();


            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;
            ViewBag.SubCampaignName = SubCampaignName ?? "";
            ViewBag.CampaignCategoryName = CampaignCategoryName ?? "";

            var lstLeads = GetLeads(dt1, dt2, ViewBag.X_CHANNEL, SubCampaignName, CampaignCategoryName);
            ViewBag.lstLeads = lstLeads;

            return View();
        }



        private List<CustomModel.ViewModelLeads> GetLeads(DateTime dt1, DateTime dt2, string xChannel, string subCampaignName, string campaignCategoryName)
        {
            var query = (from a in db.Leads
                         where a.leads_date >= dt1 && a.leads_date <= dt2
                         join b in db.campaign_master on a.leads_creativeid equals b.campaign_master_id
                         join c in (
                                from temp in db.NEW_TEMP_HIERARCHY
                                where temp.X_BM_EMP_CD != null
                                select new { SHC_SAPCODE = temp.X_BM_EMP_CD, temp.X_ZONE, temp.X_REGION, temp.X_CHANNEL, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.X_BM_NM, ROLE = "BM" })
                                .Union(
                                from temp in db.NEW_TEMP_HIERARCHY
                                where temp.X_SM_EMP_CD != null
                                select new { SHC_SAPCODE = temp.X_SM_EMP_CD, temp.X_ZONE, temp.X_REGION, X_CHANNEL = temp.X_CHANNEL, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.X_SM_NM, ROLE = "SM" })
                                .Union(
                                from temp in db.NEW_TEMP_HIERARCHY
                                where temp.AGENT_CODE != null
                                select new { SHC_SAPCODE = temp.AGENT_CODE, temp.X_ZONE, temp.X_REGION, X_CHANNEL = temp.SUBCHANNEL, temp.X_SALES_UNIT_NM, temp.X_SALES_UNIT_CD, SAP_NAME = temp.AGENT_NAME, ROLE = "AGENT" })
                         on a.leads_sapcode equals c.SHC_SAPCODE
                         join cc in db.campaign_category on b.campaign_category_id equals cc.campaign_category_id
                         join ca in db.campaigns on b.campaign_id equals ca.campaign_id
                         join sc in db.subcampaigns on b.subcampaign_id equals sc.subcampaign_id
                         select new CustomModel.ViewModelLeads
                         {
                             cm = b,
                             l = a,
                             X_CHANNEL = c.X_CHANNEL,
                             CampaignCategoryName = cc.campaign_category_name,
                             CampaignName = ca.campaign_name,
                             SubCampaignName = sc.subcampaign_name,
                             SapcodeName = c.SAP_NAME,
                             CampaignMasterId = b.campaign_master_id,
                             X_ZONE = c.X_ZONE,
                             X_REGION = c.X_REGION,
                             X_SALES_UNIT_NM = c.X_SALES_UNIT_NM,
                             X_SALES_UNIT_CD = c.X_SALES_UNIT_CD
                         });

            if (!string.IsNullOrEmpty(xChannel))
            {
                query = query.Where(x => x.X_CHANNEL == xChannel);
            }

            if (!string.IsNullOrEmpty(subCampaignName))
            {
                query = query.Where(x => x.SubCampaignName == subCampaignName);
            }

            if (!string.IsNullOrEmpty(campaignCategoryName))
            {
                query = query.Where(x => x.CampaignCategoryName == campaignCategoryName);
            }

            var lstLeads = query
                .OrderByDescending(x => x.l.leads_date)
                .ToList();

            foreach (var lead in lstLeads)
            {
                lead.CreativeId = "ADS" + (100000 + lead.CampaignMasterId);
            }

            return lstLeads;
        }







        public ActionResult CreativeWise()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            DateTime dt1 = DateTime.UtcNow.AddMinutes(330); // IST
            DateTime dt2 = DateTime.UtcNow.AddMinutes(330);

            // Fetch dropdowns
            ViewBag.Channels = db.NEW_TEMP_HIERARCHY.Select(x => x.X_CHANNEL).Distinct().ToList();
            //ViewBag.CampaignCategories = db.campaign_category.Select(x => x.campaign_category_name).ToList();

            ViewBag.CampaignCategories = db.campaign_category
    .Where(x => x.campaign_category_delflag == null
             && x.Campaign_Category_Status == "0")
    .Select(x => x.campaign_category_name)
    .OrderBy(x => x)
    .ToList();

            //ViewBag.SubCampaigns = db.subcampaigns.Select(x => x.subcampaign_name).ToList();
            ViewBag.SubCampaigns = db.subcampaigns
    .Where(x => x.subcampaign_delflag == null)
    .Select(x => x.subcampaign_name)
    .OrderBy(x => x)
    .ToList();


            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;
            ViewBag.X_CHANNEL = "";
            ViewBag.SubCampaignName = "";
            ViewBag.CampaignCategoryName = "";

            string role = Session["role"]?.ToString();
            string sessionSubChannel = Session["subchannel"]?.ToString();

            var lstLeads = new List<CustomModel.ViewModelLeads>();

            if (role == "Sub-Admin(Read-Only)" && !string.IsNullOrEmpty(sessionSubChannel))
            {
                // ✅ Force filtering only by subchannel
                lstLeads = GetCreativeWiseLeads(dt1, dt2, null, null, null, sessionSubChannel);
            }
            else
            {
                lstLeads = GetCreativeWiseLeads(dt1, dt2, null, null, null, null);
            }

            ViewBag.lstLeads = lstLeads;
            return View();
        }


        [HttpPost]
        public ActionResult CreativeWise(string fromdate, string todate, string X_CHANNEL, string SubCampaignName, string CampaignCategoryName)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index");
            }

            DateTime dt1, dt2;
            if (!DateTime.TryParse(fromdate, out dt1))
                dt1 = DateTime.UtcNow.AddMinutes(330);
            if (!DateTime.TryParse(todate, out dt2))
                dt2 = DateTime.UtcNow.AddMinutes(330);
            dt2 = dt2.AddDays(1).AddTicks(-1);

            if (dt1 > dt2) dt1 = dt2.Date;

            // Dropdowns
            ViewBag.Channels = db.NEW_TEMP_HIERARCHY.Select(x => x.X_CHANNEL).Distinct().ToList();
            //ViewBag.CampaignCategories = db.campaign_category.Select(x => x.campaign_category_name).ToList();
            //ViewBag.SubCampaigns = db.subcampaigns.Select(x => x.subcampaign_name).ToList();
            ViewBag.CampaignCategories = db.campaign_category
             .Where(x => x.campaign_category_delflag == null
                      && x.Campaign_Category_Status == "0")
             .Select(x => x.campaign_category_name)
             .OrderBy(x => x)
             .ToList();


            ViewBag.SubCampaigns = db.subcampaigns
        .Where(x => x.subcampaign_delflag == null)
        .Select(x => x.subcampaign_name)
        .OrderBy(x => x)
        .ToList();


            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;

            string role = Session["role"]?.ToString();
            string sessionSubChannel = Session["subchannel"]?.ToString();

            var lstLeads = new List<CustomModel.ViewModelLeads>();

            if (role == "Sub-Admin(Read-Only)" && !string.IsNullOrEmpty(sessionSubChannel))
            {
                // ✅ Only subchannel filter
                lstLeads = GetCreativeWiseLeads(dt1, dt2, null, SubCampaignName, CampaignCategoryName, sessionSubChannel);
            }
            else
            {
                lstLeads = GetCreativeWiseLeads(dt1, dt2, X_CHANNEL, SubCampaignName, CampaignCategoryName, null);
            }

            ViewBag.lstLeads = lstLeads;
            ViewBag.Message = lstLeads.Any() ? null : "No data found for the selected filters.";

            return View();
        }

        private List<CustomModel.ViewModelLeads> GetCreativeWiseLeads(
    DateTime dt1,
    DateTime dt2,
    string xChannel,
    string subCampaignName,
    string campaignCategoryName,
    string subChannel // ✅ new param
)
        {
            var queryA = (from engageShareCount in db.ENGAGE_SHARECOUNT
                          where engageShareCount.CREATIVE_ID != 0 &&
                                engageShareCount.SHC_DATE >= dt1 &&
                                engageShareCount.SHC_DATE <= dt2
                          group engageShareCount by new { engageShareCount.CREATIVE_ID, engageShareCount.SHC_SAPCODE, engageShareCount.SHC_PLATEFORM, engageShareCount.SHC_DATE } into grouped
                          select new
                          {
                              SHC_SAPCODE = grouped.Key.SHC_SAPCODE,
                              SHC_PLATEFORM = grouped.Key.SHC_PLATEFORM,
                              SHC_DATE = grouped.Key.SHC_DATE,
                              Creative_Id = grouped.Key.CREATIVE_ID,
                              ShareCount = grouped.Count()
                          });

            var queryB = (from tempHierarchy in db.NEW_TEMP_HIERARCHY
                          where tempHierarchy.X_BM_EMP_CD != null
                          select new
                          {
                              SHC_SAPCODE = tempHierarchy.X_BM_EMP_CD,
                              X_CHANNEL = tempHierarchy.X_CHANNEL,
                              SUBCHANNEL = tempHierarchy.SUBCHANNEL,   // ✅ keep subchannel
                              X_ZONE = tempHierarchy.X_ZONE,
                              X_REGION = tempHierarchy.X_REGION,
                              X_SALES_UNIT_NM = tempHierarchy.X_SALES_UNIT_NM,
                              X_SALES_UNIT_CD = tempHierarchy.X_SALES_UNIT_CD,
                              SAP_NAME = tempHierarchy.X_BM_NM,
                              ROLE = "BM"
                          })
                         .Union(from tempHierarchy in db.NEW_TEMP_HIERARCHY
                                where tempHierarchy.X_SM_EMP_CD != null
                                select new
                                {
                                    SHC_SAPCODE = tempHierarchy.X_SM_EMP_CD,
                                    X_CHANNEL = tempHierarchy.X_CHANNEL,
                                    SUBCHANNEL = tempHierarchy.SUBCHANNEL,
                                    X_ZONE = tempHierarchy.X_ZONE,
                                    X_REGION = tempHierarchy.X_REGION,
                                    X_SALES_UNIT_NM = tempHierarchy.X_SALES_UNIT_NM,
                                    X_SALES_UNIT_CD = tempHierarchy.X_SALES_UNIT_CD,
                                    SAP_NAME = tempHierarchy.X_SM_NM,
                                    ROLE = "SM"
                                })
                         .Union(from tempHierarchy in db.NEW_TEMP_HIERARCHY
                                where tempHierarchy.AGENT_CODE != null
                                select new
                                {
                                    SHC_SAPCODE = tempHierarchy.AGENT_CODE,
                                    X_CHANNEL = tempHierarchy.SUBCHANNEL,
                                    SUBCHANNEL = tempHierarchy.SUBCHANNEL,
                                    X_ZONE = tempHierarchy.X_ZONE,
                                    X_REGION = tempHierarchy.X_REGION,
                                    X_SALES_UNIT_NM = tempHierarchy.X_SALES_UNIT_NM,
                                    X_SALES_UNIT_CD = tempHierarchy.X_SALES_UNIT_CD,
                                    SAP_NAME = tempHierarchy.AGENT_NAME,
                                    ROLE = "AGENT"
                                });

            var query = (from a in queryA
                         join b in queryB on a.SHC_SAPCODE equals b.SHC_SAPCODE
                         join campaignMaster in db.campaign_master on a.Creative_Id equals campaignMaster.campaign_master_id
                         join campaign in db.campaigns on campaignMaster.campaign_id equals campaign.campaign_id
                         join subcampaign in db.subcampaigns on campaignMaster.subcampaign_id equals subcampaign.subcampaign_id
                         join campaignCategory in db.campaign_category on campaignMaster.campaign_category_id equals campaignCategory.campaign_category_id
                         select new CustomModel.ViewModelLeads
                         {
                             SHC_SAPCODE = a.SHC_SAPCODE,
                             SAP_NAME = b.SAP_NAME,
                             ROLE = b.ROLE,
                             X_CHANNEL = b.X_CHANNEL,
                             ShareCount = a.ShareCount,
                             SHC_PLATEFORM = a.SHC_PLATEFORM,
                             SHC_DATE = a.SHC_DATE,
                             CampaignName = campaign.campaign_name,
                             SubCampaignName = subcampaign.subcampaign_name,
                             Creative_Id = a.Creative_Id ?? 0,
                             CreativeId = a.Creative_Id.HasValue ? "ADS" + (100000 + a.Creative_Id.Value).ToString() : "ADS0",
                             CampaignMasterCreativeCaption = campaignMaster.campaign_master_creative_caption,
                             CampaignCategoryName = campaignCategory.campaign_category_name,
                             X_ZONE = b.X_ZONE,
                             X_REGION = b.X_REGION,
                             X_SALES_UNIT_NM = b.X_SALES_UNIT_NM,
                             X_SALES_UNIT_CD = b.X_SALES_UNIT_CD
                         });

            if (!string.IsNullOrEmpty(xChannel))
                query = query.Where(x => x.X_CHANNEL.Contains(xChannel));

            if (!string.IsNullOrEmpty(subChannel))
                query = query.Where(x => x.X_CHANNEL == subChannel || x.SubCampaignName == subChannel);

            if (!string.IsNullOrEmpty(subCampaignName))
                query = query.Where(x => x.SubCampaignName.Contains(subCampaignName));

            if (!string.IsNullOrEmpty(campaignCategoryName))
                query = query.Where(x => x.CampaignCategoryName.Contains(campaignCategoryName));

            return query.OrderByDescending(x => x.SHC_DATE).ToList();
        }



        public ActionResult AdminSharecount()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            DateTime dt1 = DateTime.UtcNow.AddMinutes(330).Date;
            DateTime dt2 = DateTime.UtcNow.AddMinutes(330).Date;

            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;

            string role = Session["role"]?.ToString();
            string subadminType = Session["subadmin_type"]?.ToString();
            string sessionSubChannel = Session["subchannel"]?.ToString();

            // Channel dropdown
            var channelList = db.NEW_TEMP_HIERARCHY
                .Where(t => t.X_CHANNEL != null)
                .Select(t => t.X_CHANNEL)
                .Distinct()
                .ToList();

            if (subadminType == "ReadOnly" && !string.IsNullOrEmpty(sessionSubChannel))
            {
                channelList = channelList.Where(x => x == sessionSubChannel).ToList();  // ✅ restrict dropdown
            }

            ViewBag.ChannelList = channelList;

            // Role dropdown (BM/SM/AGENT)
            ViewBag.RoleList = db.NEW_TEMP_HIERARCHY
                .Where(t => t.X_BM_EMP_CD != null || t.X_SM_EMP_CD != null || t.AGENT_CODE != null)
                .Select(t => new
                {
                    Role = t.X_BM_EMP_CD != null ? "BM" : t.X_SM_EMP_CD != null ? "SM" : "AGENT"
                })
                .Distinct()
                .Select(t => t.Role)
                .ToList();

            // Data
            var rawData = GetSharecountData(dt1, dt2, role, subadminType, sessionSubChannel, null, null, null);

            ViewBag.lstLeads = rawData.OrderByDescending(x => x.SHC_DATE).ToList();

            return View();
        }
        [HttpPost]
        public ActionResult AdminSharecount(string fromdate, string todate, string X_CHANNEL, string SAP_NAME, string ROLE)
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);

            ViewBag.dt1 = dt1;
            ViewBag.dt2 = dt2;
            ViewBag.X_CHANNEL = X_CHANNEL;
            ViewBag.SAP_NAME = SAP_NAME;
            ViewBag.ROLE = ROLE;

            string role = Session["role"]?.ToString();
            string subadminType = Session["subadmin_type"]?.ToString();
            string sessionSubChannel = Session["subchannel"]?.ToString();

            // Channel dropdown
            var channelList = db.NEW_TEMP_HIERARCHY
                .Where(t => t.X_CHANNEL != null)
                .Select(t => t.X_CHANNEL)
                .Distinct()
                .ToList();

            if (subadminType == "ReadOnly" && !string.IsNullOrEmpty(sessionSubChannel))
            {
                channelList = channelList.Where(x => x == sessionSubChannel).ToList();  // ✅ restrict dropdown
            }

            ViewBag.ChannelList = channelList;

            // Role dropdown
            ViewBag.RoleList = db.NEW_TEMP_HIERARCHY
                .Where(t => t.X_BM_EMP_CD != null || t.X_SM_EMP_CD != null || t.AGENT_CODE != null)
                .Select(t => new
                {
                    Role = t.X_BM_EMP_CD != null ? "BM" : t.X_SM_EMP_CD != null ? "SM" : "AGENT"
                })
                .Distinct()
                .Select(t => t.Role)
                .ToList();

            // Data
            var rawData = GetSharecountData(dt1, dt2, role, subadminType, sessionSubChannel, X_CHANNEL, SAP_NAME, ROLE);

            ViewBag.lstLeads = rawData.OrderByDescending(x => x.SHC_DATE).ToList();

            return View();
        }
        private List<SharecountViewModel> GetSharecountData(
    DateTime dt1, DateTime dt2,
    string role, string subadminType, string sessionSubChannel,
    string X_CHANNEL, string SAP_NAME, string ROLE)
        {
            var rawData = (from a in
                              (from e in db.ENGAGE_SHARECOUNT
                               where e.SHC_DATE >= dt1 && e.SHC_DATE <= dt2
                               group e by new { e.SHC_SAPCODE, e.SHC_DATE } into grouped
                               select new
                               {
                                   SHC_SAPCODE = grouped.Key.SHC_SAPCODE,
                                   SHC_DATE = grouped.Key.SHC_DATE,
                                   facebook_count = grouped.Where(x => x.SHC_PLATEFORM == "FACEBOOK").Sum(x => x.SHC_SHARECOUNT) ?? 0,
                                   whatsapp_count = grouped.Where(x => x.SHC_PLATEFORM == "WHATSAPP").Sum(x => x.SHC_SHARECOUNT) ?? 0,
                                   twitter_count = grouped.Where(x => x.SHC_PLATEFORM == "TWITTER").Sum(x => x.SHC_SHARECOUNT) ?? 0,
                                   instagram_count = grouped.Where(x => x.SHC_PLATEFORM == "INSTAGRAM").Sum(x => x.SHC_SHARECOUNT) ?? 0,
                                   linkedin_count = grouped.Where(x => x.SHC_PLATEFORM == "LINKEDIN").Sum(x => x.SHC_SHARECOUNT) ?? 0
                               })
                           join b in
                               (from tempHierarchy in db.NEW_TEMP_HIERARCHY
                                where tempHierarchy.X_BM_EMP_CD != null
                                select new
                                {
                                    SHC_SAPCODE = tempHierarchy.X_BM_EMP_CD,
                                    X_CHANNEL = tempHierarchy.X_CHANNEL,
                                    SAP_NAME = tempHierarchy.X_BM_NM,
                                    ROLE = "BM"
                                })
                               .Union
                               (from tempHierarchy in db.NEW_TEMP_HIERARCHY
                                where tempHierarchy.X_SM_EMP_CD != null
                                select new
                                {
                                    SHC_SAPCODE = tempHierarchy.X_SM_EMP_CD,
                                    X_CHANNEL = tempHierarchy.X_CHANNEL,
                                    SAP_NAME = tempHierarchy.X_SM_NM,
                                    ROLE = "SM"
                                })
                               .Union
                               (from tempHierarchy in db.NEW_TEMP_HIERARCHY
                                where tempHierarchy.AGENT_CODE != null
                                select new
                                {
                                    SHC_SAPCODE = tempHierarchy.AGENT_CODE,
                                    X_CHANNEL = tempHierarchy.SUBCHANNEL,
                                    SAP_NAME = tempHierarchy.AGENT_NAME,
                                    ROLE = "AGENT"
                                })
                               on a.SHC_SAPCODE equals b.SHC_SAPCODE
                           select new SharecountViewModel
                           {
                               SHC_SAPCODE = b.SHC_SAPCODE,
                               X_CHANNEL = b.X_CHANNEL,
                               SAP_NAME = b.SAP_NAME,
                               ROLE = b.ROLE,
                               SHC_DATE = a.SHC_DATE ?? DateTime.MinValue,
                               facebook_count = (int)a.facebook_count,
                               whatsapp_count = (int)a.whatsapp_count,
                               twitter_count = (int)a.twitter_count,
                               instagram_count = (int)a.instagram_count,
                               linkedin_count = (int)a.linkedin_count
                           }).ToList();

            // ✅ Restrict for Sub-Admin(Read-Only)
            if (subadminType == "ReadOnly" && !string.IsNullOrEmpty(sessionSubChannel))
                rawData = rawData.Where(x => x.X_CHANNEL == sessionSubChannel).ToList();

            // ✅ Admin/SubAdminCreator → allow filter normally
            if (!string.IsNullOrEmpty(X_CHANNEL))
                rawData = rawData.Where(x => x.X_CHANNEL != null && x.X_CHANNEL.ToLower().Contains(X_CHANNEL.ToLower())).ToList();

            if (!string.IsNullOrEmpty(SAP_NAME))
                rawData = rawData.Where(x => x.SAP_NAME != null && x.SAP_NAME.ToLower().Contains(SAP_NAME.ToLower())).ToList();

            if (!string.IsNullOrEmpty(ROLE))
                rawData = rawData.Where(x => x.ROLE != null && x.ROLE.ToLower().Contains(ROLE.ToLower())).ToList();

            return rawData;
        }





    }
}