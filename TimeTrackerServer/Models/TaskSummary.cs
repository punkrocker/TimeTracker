using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeTrackerServer.Models
{
    public class TaskSummary
    {
        public int UserId { get; set; }
        public int? RealTask { get; set; }
        public int? ReportTime { get; set; }
        public int ProjectId { get; set; }
    }
}