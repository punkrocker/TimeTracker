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
    
    public partial class T_PM_Project
    {
        public int ProjectID { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int CustomerID { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public Nullable<System.DateTime> RecordTime { get; set; }
    }
}
