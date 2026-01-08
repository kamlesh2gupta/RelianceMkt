using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RelianceMkt.Models;
using System.Data;

namespace RelianceMkt.Models
{
    public class CustomModel
    {
        public class ModelNewCampaign
        {
            public campaign_master cm { get; set; }
            public HttpPostedFileBase[] images { get; set; }

            // ✅ Add this for multiple channel selection
            public List<int> SelectedChannels { get; set; }
        }
        public class MyDataModel
        {
            public string Lead_Id { get; set; }
        }
        public class ViewModelLeads
        {
            public campaign_master cm { get; set; }
            public Lead l { get; set; }
            public string X_CHANNEL { get; set; }
            public string SAP_NAME { get; set; }
            public string X_ZONE { get; set; }
            public string X_REGION { get; set; }
            public string X_SALES_UNIT_NM { get; set; }
            public string X_SALES_UNIT_CD { get; set; }

            public string CampaignCategoryName { get; set; }
            public string CampaignName { get; set; }
            public string SubCampaignName { get; set; }
            public string SapcodeName { get; set; }
            public string CreativeId { get; set; }
            public decimal Creative_Id { get; set; }
            public string SHC_SAPCODE { get; set; }
            public string SHC_PLATEFORM { get; set; }
           // public string Campaign_Image_URL { get; set; }
            
            public decimal CampaignMasterId { get; set; }
            public int ShareCount { get; internal set; }
            public string ROLE { get; internal set; }
            public DateTime? SHC_DATE { get; internal set; }
            public string CampaignMasterCreativeCaption { get; internal set; }

        }

        public class ViewModelCampaignCategory
        {
            public campaign cam { get; set; }
            public campaign_category cc { get; set; }
            public subcampaign sc { get; set; }
            public campaign_master cm { get; set; }
            public channel ch { get; set; }
        }


        public class ViewModelLeaderboard
        {
           public int rank { get; set; }
            public decimal count { get; set; }
            public string sapcode { get; set; }
        }


        public class ViewModelTimeline
        {
            public int Share { get; set; }
            public int Lead { get; set; }
            public string sapcode { get; set; }

            public DateTime dt { get; set; }
        }

        public class JSONEvent
        {
            public string event_id { get; set; }

            public string title { get; set; }

            public string start { get; set; }

            public string end { get; set; }

            public string color { get; set; }

            public string url { get; set; }
        }


        public class ModelPieChart
        {
            public string name { get; set; }
            public string short_name { get; set; }
            public int y { get; set; }
            public string colorIndex { get; set; }

        }

        public class jsonPieChart
        {
            public string name { get; set; }

            public List<ModelPieChart> data { get; set; }
        }

        public class ViewGroupByShare
        {
            //public Nullable<System.DateTime> share_date { get; set; }
            public int count { get; set; }
            public  string sapcode { get; set; }
        }




    }
}