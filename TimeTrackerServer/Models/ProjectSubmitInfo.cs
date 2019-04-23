using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeTrackerServer.Models
{
    public class ProjectSubmitInfo
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int UserID { get; set; }
        public string UseName { get; set; }
        public int Charge { get; set; }
        public int? Task { get; set; }
        public int? Time { get; set; }
    }
}