//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class T_PM_Task
    {
        public int ProjectID { get; set; }
        public int UserID { get; set; }
        public System.DateTime TaskDate { get; set; }
        public int SubmitSeq { get; set; }
        public int TeamID { get; set; }
        public Nullable<int> PlanTask { get; set; }
        public Nullable<int> RealTask { get; set; }
        public Nullable<int> TaskTime { get; set; }
        public Nullable<int> ReportTime { get; set; }
        public string Desc { get; set; }
        public Nullable<System.DateTime> SubmitTime { get; set; }
    }
}
