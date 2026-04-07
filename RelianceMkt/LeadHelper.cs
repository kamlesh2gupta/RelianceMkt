using RelianceMkt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;

public class LeadHelper
{
    public static async Task FindAndProcessLead(string mobileNumber, string campaignName)
    {
        try
        {
            using (var db = new rglinixm_relEntities())
            {
                // ✅ Normalize mobile (last 10 digit)
                string normalizedMobile = mobileNumber?.Length > 10
                    ? mobileNumber.Substring(mobileNumber.Length - 10)
                    : mobileNumber;

                // ✅ CENTRAL_BLAST lookup
                var blastRecord = db.CENTRAL_BLAST
                    .FirstOrDefault(x =>
                        x.CampaignName == campaignName &&
                        x.Contact_Number.EndsWith(normalizedMobile));

                if (blastRecord == null)
                    return;

                string channelCode = blastRecord.Channel?.Trim().ToUpper();
                string sapCode = blastRecord.L1_Code?.ToString();
                string userName = blastRecord.Name?.Trim();
                string userMobile = normalizedMobile;

                if (string.IsNullOrEmpty(channelCode) || string.IsNullOrEmpty(sapCode))
                    return;

                // ✅ Hierarchy lookup
                var empData = db.NEW_TEMP_HIERARCHY
                    .FirstOrDefault(x =>
                        x.X_BM_EMP_CD == sapCode ||
                        x.X_ZM_EMP_CD == sapCode ||
                        x.X_RM_EMP_EMP_CD == sapCode ||
                        x.X_SM_EMP_CD == sapCode);

                if (empData == null)
                    return;

                // ✅ SAP Name resolve
                string sapName = blastRecord.L1_Name?.Trim();

                if (empData.X_BM_EMP_CD == sapCode)
                    sapName = empData.X_BM_NM;
                else if (empData.X_ZM_EMP_CD == sapCode)
                    sapName = empData.X_ZM_NM;
                else if (empData.X_RM_EMP_EMP_CD == sapCode)
                    sapName = empData.X_RM_NM;
                else if (empData.X_SM_EMP_CD == sapCode)
                    sapName = empData.X_SM_NM;

                // ✅ Category
                var categoryName = (from cm in db.campaign_master
                                    join cc in db.campaign_category
                                    on cm.campaign_category_id equals cc.campaign_category_id
                                    where cm.channel_code == channelCode
                                    select cc.campaign_category_name)
                                    .FirstOrDefault();

                // ✅ Channel mapping
                var lsqChannels = new List<string> { "GR", "DL", "DP", "PC", "PM", "EN" };
                var seChannels = new List<string> { "AG", "CN", "GR", "NV", "ST", "CD", "EN" };

                var api = new ApiHelper();

                string apiRequestId = null;
                string rawJson = "";

                if (lsqChannels.Contains(channelCode))
                {
                    var result =  await api.TestAPI(userName, userMobile, sapCode, sapName);
                    apiRequestId = result.Response?.RequestId;
                    rawJson = result.RawJson;

                }
                else if (seChannels.Contains(channelCode))
                {
                    var result = await api.TestAPI_SE(userName, userMobile, sapCode, sapName, categoryName);
                    apiRequestId = result.Response?.RequestId;
                    rawJson = result.RawJson;
                }
                else
                {
                    // Channel not mapped — return
                    return;
                }
                var lead = new Lead
                {
                    leads_name = userName,
                    leads_mobile = Convert.ToInt64(userMobile),
                    leads_sapcode = sapCode,
                    leads_date = DateTime.UtcNow.AddMinutes(330),
                    leads_plateform = "WhatsApp",              // ✅ Platform WhatsApp
                    api_leads_id = apiRequestId,            // ✅ API Request ID
                    api_response_json = rawJson,                 // ✅ Full API Response JSON
                    LeadType = categoryName,            // ✅ Category Name
                    leads_creativeid = 0                        // ✅ Default — WhatsApp se creative nahi aata
                };

                db.Leads.Add(lead);
                db.SaveChanges();


            }
        }
        catch (Exception ex)
        {
            // optional logging
        }
    }
}