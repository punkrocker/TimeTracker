using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class ReportInfo
    {
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int? TaskNum { get; set; }
        public int? RealTime { get; set; }
        public int? ModifyTime { get; set; }
        public double RealAvg { get; set; }
        public double ModifyAvg { get; set; }
    }
}
