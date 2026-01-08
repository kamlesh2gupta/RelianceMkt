using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RelianceMkt.Models;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Net;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using ExcelDataReader;
using System.Reflection.Emit;

namespace RelianceMkt.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public rglinixm_relEntities db = new rglinixm_relEntities();
        public ActionResult SendCampaign()
        {
            ViewBag.lstcampaign = (from a in db.campaign_master
                            where a.camapaign_master_status == "Start"
                            select a).ToList();
           
            return View();
        }

        [HttpPost]
        public ActionResult ImportExcel(HttpPostedFileBase file, int Campaign_Master_Id)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    // Open the Excel file using ExcelDataReader
                    using (var stream = new MemoryStream())
                    {
                        file.InputStream.CopyTo(stream);
                        stream.Position = 0;
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            // Read the first sheet of the Excel file
                            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });
                            var dataTable = dataSet.Tables[0];

                            // Iterate through the rows of the Excel file and add them to the database
                            //using (var context = new rglinixm_relEntities())
                            {
                                DateTime dateTime = DateTime.Now;
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    var data = new Digimyin_Send_Campaign()
                                    {
                                        Name = row["Name"].ToString(),
                                        Type_Of_Data = row["Type_Of_Data"].ToString(),
                                        L1_Code = row["L1_Code"].ToString(),
                                        L1_Name = row["L1_Name"].ToString(),
                                        Channel = row["Channel"].ToString(),
                                        Created = dateTime,
                                        Status = 0,
                                        Updated = dateTime,
                                        //Price = decimal.Parse(row["Price"].ToString()),
                                        Campaign_Master_Id = Campaign_Master_Id,
                                        ///Subcategory = subcategory
                                    };
                                    db.Digimyin_Send_Campaign.Add(data);
                                    ////db.SaveChanges();
                                }
                                db.SaveChanges();
                            }
                        }
                    }

                    ViewBag.Message = "Imported successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error occurred: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "Please select an Excel file to import";
            }
            ViewBag.lstcampaign = (from a in db.campaign_master
                                   where a.camapaign_master_status == "Start"
                                   select a).ToList();
            return View("SendCampaign");
        }
        public ActionResult ChangeStatus(string key)
        {
            //?key=ID=1&STATUS=2
            int ID = 0;
            int STATUS =0;
            var valueBytes = Convert.FromBase64String(key);
            string str_REFKEY = System.Text.Encoding.UTF8.GetString(valueBytes);

            string[] separate_params = str_REFKEY.Split('&');

            foreach (var item in separate_params)
            {
                if (item.Contains("ID"))
                {

                    ID = Convert.ToInt16((item.Split('='))[1]);
                }
                if (item.Contains("STATUS"))
                {

                    STATUS = Convert.ToInt16((item.Split('='))[1]);
                }
            }
            var data = db.Digimyin_Send_Campaign.Where(x => x.ID == ID).FirstOrDefault();
            if(data != null)
            {
                DateTime dateTime = DateTime.Now;
                data.Status = STATUS;
                data.Updated = dateTime;
                db.SaveChanges();
                ViewBag.Message = "Status Changed successfully";
            }
            return View();
        }

    }    

    }