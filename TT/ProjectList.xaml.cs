using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using TimeTracker.BLL;
using TT.Model;
using System.Data;
using TimeTracker.Util;
using TT.Util;
using Tstring.DataBase;

namespace TT
{
    /// <summary>
    /// ProjectList.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectList : Window
    {
        private static ProjectList pl;
        public static ProjectList getProjectList()
        {
            if (pl == null)
                pl = new ProjectList();

            return pl;
        }

        private ProjectList()
        {
            InitializeComponent();
            BindProjects();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void BindProjects()
        {
            
            TimeService.Service ts = new TimeService.Service();

            DataSet ds = ts.GetProjects(TimeRecorder.UserID.ToString());
            if (ds != null && ds.Tables.Count != 0)
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    SqliteOper sp = new SqliteOper();
                    sp.ExecuteNonQuery("REPLACE  Into T_PM_Project (ProjectID,ProjectCode,ProjectName,CustomerID,[Status]) Values ('" + dr["ProjectID"].ToString() + "','" + dr["ProjectCode"].ToString() + "','" + dr["ProjectName"].ToString() + "','" + dr["CustomerID"].ToString() + "','1');Insert Into T_PM_USERTIME (ProjectID,UserID,TaskTime) Values ('" + dr["ProjectID"].ToString() + "','" + dr["UserID"].ToString() + "','0')");
                }
            
        }


        private void Window_Activated(object sender, EventArgs e)
        {
            List<ProjectInfo> memberData = new List<ProjectInfo>();
            SqliteOper sp = new SqliteOper();
            DataSet ds = sp.ExecuteDataSet("SELECT * FROM T_PM_Project T1 Left Join T_PM_UserTime T2 On T1.[ProjectID] = T2.ProjectID Where T2.UserID = '" + TimeRecorder.UserID + "' And (T2.Status = '" + AppConst.Downloaded + "' OR T2.Status = '" + AppConst.Executing + "')");
            if (ds != null && ds.Tables.Count != 0)
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ProjectInfo item = new ProjectInfo(dr);
                    memberData.Add(item);
                }
            gridProject.DataContext = memberData;
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            TimeRecorder.getRecorder().StartRecord();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            TimeRecorder.getRecorder().StopRecord();
        }


        private void gridProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TimeRecorder.CurrentProjectID = Convert.ToInt16((gridProject.SelectedItem as ProjectInfo).projectid);
        }

        private void toolBar1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
