using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeTrackerServer.Services
{
    public class ProjectService
    {
        //select t1.ProjectID,
        //t1.ProjectName,
        //t2.UserID,
        //t3.UseName,
        //t2.Charge,
        //isnull(t4.task, 0) as Task,
        //isnull(t4.time, 0) as Time
        //    from T_PM_Project t1
        //left join T_PM_Member t2 on t1.ProjectID = t2.ProjectID
        //    left join T_Sys_UserInfo t3 on t2.UserID = t3.UserID
        //    left join (select UserID, sum(RealTask) as task, sum(ReportTime) as time
        //    from T_PM_Task
        //    where TaskDate = CONVERT(varchar(10), GETDATE(), 120)
        //group by UserID) t4 on t3.UserID = t4.UserID
        //    where t1.ProjectID in (select ProjectID from T_PM_Member where UserID = 32 and Charge = 1);
    }
}