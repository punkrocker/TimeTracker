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
        public int? TaskNum { get; set; }
        public int? RealTime { get; set; }
        public int? ModifyTime { get; set; }
        public int? RealAvg { get; set; }
        public int? ModifyAvg { get; set; }
    }
}
