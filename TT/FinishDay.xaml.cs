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
using TT.Model;
using Tstring.DataBase;
using System.Data;
using Model;
using Model.Dto;
using Model.WebResult;
using TT.Util;
using TimeTracker.Util;

namespace TT
{
    /// <summary>
    /// FinishDay.xaml 的交互逻辑
    /// </summary>
    public partial class FinishDay : Window
    {
        public FinishDay()
        {
            InitializeComponent();
            this.Title = ConfigFile.Languege.ReadValue("tlSubmit");
            btnUpload.ToolTip = ConfigFile.Languege.ReadValue("tgSubmitAll");
            btnClose.ToolTip = ConfigFile.Languege.ReadValue("tgExit");
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            List<ProjectInfo> memberData = new List<ProjectInfo>();
            SqliteOper sp = new SqliteOper();
            DataSet ds = sp.ExecuteDataSet("SELECT * FROM T_PM_Project T1 Left Join T_PM_UserTime T2 On T1.[ProjectID] = T2.ProjectID Where T2.UserID = '" + TimeRecorder.UserID + "' And (T2.Status = '" + AppConst.Executing + "' Or T2.Status = '" + AppConst.Resolved + "')");
            if (ds != null && ds.Tables.Count != 0)
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ProjectTimeInfo item = new ProjectTimeInfo(dr);
                    memberData.Add(item);
                }
            gridProject.DataContext = memberData;
        }

        private void itemsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (gridProject.SelectedIndex == -1)
                return;
            var selectItem = this.gridProject.SelectedItem;
            ProjectTimeInfo item = (ProjectTimeInfo)selectItem;

            this.Hide();
            ResolveProject cp = new ResolveProject(item);
            cp.ShowDialog();
            this.ShowDialog();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            if (gridProject.Items.Count == 0)
            {
                Application.Current.Shutdown();
            }
            else
            {
                MessageBox.Show(ConfigFile.Languege.ReadValue("NotFinish"), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void gridProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridProject.SelectedIndex != -1)
                RefreshGrid();
        }

        private void RefreshGrid()
        {
            ProjectTimeInfo prj = (gridProject.SelectedItem as ProjectTimeInfo);
            Dictionary<string, string> TeamList = new Dictionary<string, string>();

            List<KeyValuePair<string, string>> paramlist = new List<KeyValuePair<string, string>>();
            paramlist.Add(new KeyValuePair<string, string>("customerid", prj.customerid));
            var customerTeamsResult = WebCall.GetMethod<CustomerTeamsResult>(WebCall.GetCustomerTeams, paramlist);

            if (customerTeamsResult.Code==SystemConst.MsgSuccess)
            {
                foreach (var customerTeam in customerTeamsResult.Data)
                {
                    CustomerTeam ct = new CustomerTeam()
                    {
                        teamid = customerTeam.TeamId,
                        teamname = customerTeam.TeamName
                    };
                    TeamList.Add(ct.teamid.ToString(), ct.teamname);
                }
            }

            List<ProjectTimeDetail> memberData = new List<ProjectTimeDetail>();
            SqliteOper sp = new SqliteOper();
            string sql = "Select T1.TeamID,T2.TeamName,T1.UserID,T1.ProjectID,T3.ProjectName,T3.ProjectCode,T3.CustomerID,T1.PlanTask,T1.RealTask,T1.TaskTime,T1.[Desc] From T_PM_Task T1 Left Join T_HR_Team T2 on T1.TeamID = T2.TeamID Left Join T_PM_Project T3 on T1.ProjectID = T3.ProjectID Where T1.ProjectID = '" + prj.projectid + "' And UserID = '" + TimeRecorder.UserID + "' And T1.[Status] <> '" + AppConst.Uploaded + "'";
            DataSet ds = sp.ExecuteDataSet(sql);
            if (ds != null && ds.Tables.Count != 0)
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ProjectTimeDetail item = new ProjectTimeDetail();
                    item.teamid = dr["teamid"].ToString();
                    item.teamname = "";
                    string tm = "";
                    if (TeamList.TryGetValue(item.teamid, out tm))
                        item.teamname = tm;
                    item.userid = TimeRecorder.UserID.ToString();
                    item.projectid = dr["projectid"].ToString();
                    item.projectcode = dr["projectcode"].ToString();
                    item.projectname = dr["projectname"].ToString();
                    item.customerid = dr["customerid"].ToString();
                    item.plan = Convert.ToInt16(dr["PlanTask"].ToString());
                    item.real = Convert.ToInt16(dr["RealTask"].ToString());
                    item.tasktime = Convert.ToInt16(dr["TaskTime"].ToString());
                    item.comment = dr["Desc"].ToString();
                    memberData.Add(item);
                }
            gridDetail.DataContext = memberData;
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            SqliteOper sq = new SqliteOper();
            string prjcount = sq.ExecuteDataSet("Select Count(1) as ACount From T_PM_UserTime Where Status = '" + AppConst.Executing + "'").Tables[0].Rows[0]["ACount"].ToString();
            if (!prjcount.Equals("0"))
            {
                MessageBox.Show(ConfigFile.Languege.ReadValue("NotResolved"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            DataSet result = sq.ExecuteDataSet("Select ProjectID,UserID,TaskDate,SubmitSeq,TeamID,PlanTask,RealTask,TaskTime,[Desc],SubmitTime From T_PM_Task Where UserID = '" + TimeRecorder.UserID + "' And [Status] = '" + AppConst.Comfirmed + "'");
            if (result != null && result.Tables.Count != 0)
            {
                List<CommitTask> commitTasks = new List<CommitTask>();
                foreach (DataRow dr in result.Tables[0].Rows)
                {
                    CommitTask commitTask = new CommitTask
                    {
                        ProjectId = Convert.ToInt32(dr["ProjectID"]),
                        UserId = Convert.ToInt32(dr["UserID"]),
                        TaskDate = Convert.ToDateTime(dr["TaskDate"]),
                        TeamId = Convert.ToInt32(dr["TeamID"]),
                        PlanTask = Convert.ToUInt32(dr["PlanTask"]),
                        RealTask = Convert.ToUInt32(dr["RealTask"]),
                        TaskTime = Convert.ToUInt64(dr["TaskTime"]),
                        Desc = dr["Desc"].ToString(),
                        SubmitTime = Convert.ToDateTime(dr["SubmitTime"])
                    };
                    commitTasks.Add(commitTask);
                }

                WebResult webResult = AppUtils.JsonDeserialize<WebResult>(WebCall.PostMethod<List<CommitTask>>(WebCall.CommitProjects, commitTasks));
                if (webResult.Code.Equals(SystemConst.MsgSuccess))
                {
                    SqliteOper sp = new SqliteOper();
                    sp.ExecuteNonQuery("Update T_PM_Task set Status = '" + AppConst.Uploaded + "' Where  UserID = '" + TimeRecorder.UserID + "' And Status = '" + AppConst.Comfirmed + "';Update T_PM_UserTime Set Status = '" + AppConst.Finished + "' Where UserID = '" + TimeRecorder.UserID + "' And Status = '" + AppConst.Resolved + "';Update T_PM_Project Set Status = '" + AppConst.Finished + "' Where Status = '" + AppConst.Executing + "'");
                    MessageBox.Show(ConfigFile.Languege.ReadValue("UploadSuccess"), "", MessageBoxButton.OK, MessageBoxImage.None);
                    sp.ExecuteNonQuery("Delete From T_PM_Task Where Status = '" + AppConst.Uploaded + "';Delete From T_PM_UserTime Where Status = '" + AppConst.Finished + "'");
                    MenuWindow.getMain().RefreshGrid();
                    TimeRecorder.getRecorder().RecordingProject = null;
                    this.Hide();
                }
                else
                    MessageBox.Show(webResult.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
