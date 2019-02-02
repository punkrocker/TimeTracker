using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class CustomerProjectDetail
    {
        public int ProjectId { get; set; }
        public int TeamId { get; set; }
        public string ProjectName { get; set; }
        public string TeamName { get; set; }
        public Int32? TaskCount { get; set; }
        public Int32? TimeCount { get; set; }
    }
}
