using System;

namespace RelianceMkt.Controllers
{
    public class SharecountViewModel
    {
        public string SHC_SAPCODE { get; set; }
        public string X_CHANNEL { get; set; }
        public string SAP_NAME { get; set; }
        public string ROLE { get; set; }
        public DateTime SHC_DATE { get; set; }
        public int facebook_count { get; set; }
        public int whatsapp_count { get; set; }
        public int twitter_count { get; set; }
        public int instagram_count { get; set; }
        public int linkedin_count { get; set; }
    }
}