using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model.Dto
{
    public class CommitTask
    {
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public DateTime TaskDate { get; set; }
        public int TeamId { get; set; }
        public UInt32 PlanTask { get; set; }
        public UInt32 RealTask { get; set; }
        public UInt64 TaskTime { get; set; }
        public DateTime SubmitTime { get; set; }
        public string Desc { get; set; }
    }
}
