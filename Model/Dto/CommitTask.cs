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
        public int PlanTask { get; set; }
        public int RealTask { get; set; }
        public int TaskTime { get; set; }
        public DateTime SubmitTime { get; set; }
    }
}
