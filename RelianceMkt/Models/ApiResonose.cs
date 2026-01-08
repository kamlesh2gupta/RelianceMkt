using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RelianceMkt.Models
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AdditionalDetail
    {
        public int PostDeDupActionPerformed { get; set; }
        public string ActivityId { get; set; }
    }

    public class ApiResponose
    {
        public string Status { get; set; }
        public int PrimaryAction { get; set; }
        public int SecondaryAction { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string RequestId { get; set; }
        public string RelatedProspectId { get; set; }
        public bool IsUnique { get; set; }
        public object CreatedOpportunityId { get; set; }
        public string ConflictedOpportunityId { get; set; }
        public List<AdditionalDetail> AdditionalDetails { get; set; }
        public bool OverwriteFields { get; set; }
        public bool UpdateEmptyFields { get; set; }
        public bool DoNotPostDuplicateActivity { get; set; }
        public bool DoNotChangeOwner { get; set; }
        public string Lead_Id { get; set; }
    }

    public class CentralBlast
    {
        public int Sr_Id { get; set; }
        public string Name { get; set; }
        public string mobileNumber { get; set; }
        public string Type_Of_Data { get; set; }
        public int L1_Code { get; set; }
        public string L1_Name { get; set; }
        public string Channel { get; set; }
        public string CampaignName { get; set; }
    }

}