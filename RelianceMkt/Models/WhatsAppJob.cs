using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RelianceMkt.Models
{
    public class WhatsAppJob
    {
        public string Mobile { get; set; }
        public string Name { get; set; }
        public string CampaignName { get; set; }
        public string ImageLink { get; set; }
        public int RetryCount { get; set; } = 0;
    }
}