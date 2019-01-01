using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TT.Model
{
    public class ProjectInfo
    {
        public string projectid
        {
            get;
            set;
        }
        public string projectcode
        {
            get;
            set;
        }
        public string projectname
        {
            get;
            set;
        }

        public string customerid
        {
            get;
            set;
        }



        public ProjectInfo(System.Data.DataRow dr)
        {
            projectid = dr["ProjectID"].ToString();
            projectcode = dr["ProjectCode"].ToString();
            projectname = dr["ProjectName"].ToString();
            customerid = dr["CustomerID"].ToString();

        }
    }

    public class ProjectTimeInfo : ProjectInfo
    {
        public int tasktime
        {
            get;
            set;
        }

        

        public ProjectTimeInfo(System.Data.DataRow dr)
            : base(dr)
        {
            projectid = dr["ProjectID"].ToString();
            projectcode = dr["ProjectCode"].ToString();
            projectname = dr["ProjectName"].ToString();
            customerid = dr["CustomerID"].ToString();
            tasktime = Convert.ToInt16(dr["TaskTime"]) / 60;
        }
    }

    public class ProjectTimeDetail
    {
        public string teamid
        {
            get;
            set;
        }

        public string teamname
        {
            get;
            set;
        }

        public string userid
        {
            get;
            set;
        }

        public string username
        {
            get;
            set;
        }

        public string projectid
        {
            get;
            set;
        }
        public string projectcode
        {
            get;
            set;
        }
        public string projectname
        {
            get;
            set;
        }

        public string customerid
        {
            get;
            set;
        }

        public int plan
        {
            get;
            set;
        }

        public int real
        {
            get;
            set;
        }

        public int tasktime
        {
            get;
            set;
        }

        public string comment
        {
            get;
            set;
        }

        public ProjectTimeDetail()
        {
        }
    }
}