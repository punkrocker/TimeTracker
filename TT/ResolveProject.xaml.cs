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
using TT.Util;
using TimeTracker.Util;
using System.Data;
using Model;
using Model.WebResult;
using Tstring.DataBase;

namespace TT
{
    /// <summary>
    /// ResolveProject.xaml 的交互逻辑
    /// </summary>
    public partial class ResolveProject : Window
    {
        private ProjectTimeInfo prj;
        public int TimeLeft
        {
            get
            {
                return Convert.ToInt16(txtTimeLeft.Text);
            }
            set
            {
                txtTimeLeft.Text = value.ToString();
            }
        }
        public ResolveProject(ProjectTimeInfo _prj)
        {
            InitializeComponent();
            prj = _prj;
            InitUI();
        }
        Dictionary<string, string> TeamList = new Dictionary<string, string>();

        private void InitUI()
        {
            this.Title = TimeRecorder.Username + "/" + prj.projectname;

            lblTimeLeft.Content = ConfigFile.Languege.ReadValue("lblTimeLeft");
            this.TimeLeft = prj.tasktime;
            lblCustomer.Content = ConfigFile.Languege.ReadValue("lblCustomer");

            List<KeyValuePair<string, string>> paramlist = new List<KeyValuePair<string, string>>();
            paramlist.Add(new KeyValuePair<string, string>("customerid", prj.customerid));
            var customerTeamsResult = WebCall.GetMethod<CustomerTeamsResult>(WebCall.GetCustomerTeams, paramlist);

            if (customerTeamsResult.Code==SystemConst.MsgSuccess)
            {
                foreach (var customerTeamsDto in customerTeamsResult.Data)
                {
                    txtCustomer.Text = customerTeamsDto.CustomerName;

                    CustomerTeam ct = new CustomerTeam
                    {
                        teamid = customerTeamsDto.TeamId,
                        teamname = customerTeamsDto.TeamName
                    };
                    cbxTeam.Items.Add(ct);
                    TeamList.Add(ct.teamid.ToString(), ct.teamname);
                    cbxTeam.SelectedIndex = 0;
                }
            }

            lblTeam.Content = ConfigFile.Languege.ReadValue("lblTeam");
            lblTaskReceived.Content = ConfigFile.Languege.ReadValue("lblTaskReceived");
            lblTaskFinished.Content = ConfigFile.Languege.ReadValue("lblTaskFinished");
            lblTaskTime.Content = ConfigFile.Languege.ReadValue("lblTaskTime");
            lblComment.Content = ConfigFile.Languege.ReadValue("lblComment");

            btnAdd.Content = ConfigFile.Languege.ReadValue("btnAdd");
            btnDelete.Content = ConfigFile.Languege.ReadValue("btnDelete");
            btnClearAll.Content = ConfigFile.Languege.ReadValue("btnClearAll");
            btnClose.Content = ConfigFile.Languege.ReadValue("btnClose");

            InitGrid();
        }

        private void InitGrid()
        {
            List<ProjectTimeDetail> memberData = new List<ProjectTimeDetail>();
            SqliteOper sp = new SqliteOper();
            string sql = "Select T1.TeamID,T2.TeamName,T1.UserID,T1.ProjectID,T3.ProjectName,T3.ProjectCode,T3.CustomerID,T1.PlanTask,T1.RealTask,T1.TaskTime,T1.[Desc] From T_PM_Task T1 Left Join T_HR_Team T2 on T1.TeamID = T2.TeamID Left Join T_PM_Project T3 on T1.ProjectID = T3.ProjectID Where T1.ProjectID = '" + this.prj.projectid + "' And UserID = '" + TimeRecorder.UserID + "' And T1.[Status] <> '" + AppConst.Uploaded + "'";
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

                    TimeLeft -= item.tasktime;
                    if (item.teamid != "0")
                    {
                        CustomerTeam ct = new CustomerTeam();
                        ct.teamid = Convert.ToInt16(item.teamid);
                        ct.teamname = item.teamname;
                        CustomerTeam temp = null;
                        foreach (CustomerTeam ctitem in cbxTeam.Items)
                            if (ctitem.teamid == ct.teamid)
                                temp = ctitem;
                        if (temp != null)
                            cbxTeam.Items.Remove(temp);
                    }
                    if (cbxTeam.Items.Count != 0)
                        cbxTeam.SelectedIndex = 0;
                }
            gridDetail.DataContext = memberData;
        }

        #region 检测数字输入
        private void textBox1_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!isNumberic(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void textBox1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!isNumberic(e.Text))
            {
                e.Handled = true;
            }
            else
                e.Handled = false;
        }

        //isDigit是否是数字
        public static bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    //if(c<'0' c="">'9')//最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
                    return false;
            }
            return true;
        }
        #endregion

        private void RefreshGrid()
        {
            List<ProjectTimeDetail> memberData = new List<ProjectTimeDetail>();
            SqliteOper sp = new SqliteOper();
            string sql = "Select T1.TeamID,T2.TeamName,T1.UserID,T1.ProjectID,T3.ProjectName,T3.ProjectCode,T3.CustomerID,T1.PlanTask,T1.RealTask,T1.TaskTime,T1.[Desc] From T_PM_Task T1 Left Join T_HR_Team T2 on T1.TeamID = T2.TeamID Left Join T_PM_Project T3 on T1.ProjectID = T3.ProjectID Where T1.ProjectID = '" + this.prj.projectid + "' And UserID = '" + TimeRecorder.UserID + "' And T1.[Status] <> '" + AppConst.Uploaded + "'";
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckInfo())
            {
                string teamid = "";
                if (cbxTeam.SelectedIndex != -1)
                    teamid = (cbxTeam.SelectedItem as CustomerTeam).teamid.ToString();
                else
                {
                    MessageBox.Show(ConfigFile.Languege.ReadValue("NoTeam"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                    //teamid = "0";
                }
                SqliteOper sp = new SqliteOper();
                if (!sp.ExecuteDataSet("Select Count(1) As ACount From T_PM_Task Where ProjectID = '" + prj.projectid + "' And UserID = '" + TimeRecorder.UserID + "' And TeamID = '" + teamid + "' And Status <> '" + AppConst.Uploaded + "'").Tables[0].Rows[0]["ACount"].ToString().Equals("0"))
                    return;
                try
                {
                    string sql = "Insert Into T_PM_Task (ProjectID,UserID,TaskDate,TeamID,PlanTask,RealTask,TaskTime,[Desc],SubmitTime,Status) Values ('" + prj.projectid + "','" + TimeRecorder.UserID + "',date(),'" + teamid + "','" + txtPlan.Text + "','" + txtActual.Text + "','" + txtTaskTime.Text + "','" + txtComment.Text + "',datetime('now','localtime'),'1')";
                    sp.ExecuteNonQuery(sql);
                    TimeLeft -= Convert.ToInt16(txtTaskTime.Text);
                    if (teamid != "0")
                        cbxTeam.Items.RemoveAt(cbxTeam.SelectedIndex);
                    if (cbxTeam.Items.Count != 0)
                        cbxTeam.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ConfigFile.Languege.ReadValue("SameTeam"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                RefreshGrid();
            }
        }

        private bool CheckInfo()
        {
            try
            {
                if (Convert.ToInt16(txtActual.Text) > Convert.ToInt16(txtPlan.Text))
                {
                    MessageBox.Show(ConfigFile.Languege.ReadValue("CountError"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else if ((Convert.ToInt16(txtActual.Text) < Convert.ToInt16(txtPlan.Text) && (txtComment.Text.Trim().Equals(""))))
                {
                    MessageBox.Show(ConfigFile.Languege.ReadValue("InputComment"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else if (Convert.ToInt16(txtTaskTime.Text) > TimeLeft)
                {
                    MessageBox.Show(ConfigFile.Languege.ReadValue("TimeOverflow"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                //foreach (ProjectTimeDetail detailInfo in gridDetail.Items)
                //{
                //    if ((detailInfo.teamid == (cbxTeam.SelectedItem as CustomerTeam).teamid.ToString()) || (cbxTeam.SelectedIndex == -1 && detailInfo.teamid == "0"))
                //    {
                //        MessageBox.Show(ConfigFile.Languege.ReadValue("SameTeam"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return false;
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (gridDetail.SelectedIndex == -1)
                return;
            var selectItem = this.gridDetail.SelectedItem;
            ProjectTimeDetail detailInfo = (ProjectTimeDetail)selectItem;

            if (MessageBox.Show(ConfigFile.Languege.ReadValue("ComfirmDelete"), "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SqliteOper sp = new SqliteOper();
                sp.ExecuteNonQuery("Delete From T_PM_Task Where ProjectID = '" + detailInfo.projectid + "' And UserID = '" + TimeRecorder.UserID + "' And TeamID = '" + detailInfo.teamid + "' And Status <> '" + AppConst.Uploaded + "'");
                TimeLeft += detailInfo.tasktime;

                CustomerTeam ct = new CustomerTeam();
                if (detailInfo.teamid != "0")
                    ct.teamid = Convert.ToInt16(detailInfo.teamid);
                else
                    ct.teamid = 0;
                ct.teamname = detailInfo.teamname;

                if (ct.teamid != 0)
                    cbxTeam.Items.Add(ct);

                RefreshGrid();
            }
        }

        private void SliderTimeLeft_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //SliderTimeLeft.Value = Convert.ToInt16(SliderTimeLeft.Value);
            //txtTaskTime.Text = SliderTimeLeft.Value.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TimeLeft != 0)
            {
                MessageBox.Show(ConfigFile.Languege.ReadValue("NotEmpty"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Cancel = true;
            }
            else
            {
                SqliteOper sp = new SqliteOper();
                sp.ExecuteNonQuery("Update T_PM_Task set Status = '" + AppConst.Comfirmed + "' Where ProjectID = '" + prj.projectid + "' And UserID = '" + TimeRecorder.UserID + "' And Status = '" + AppConst.New + "';Update T_PM_UserTime set Status = '" + AppConst.Resolved + "' Where ProjectID = '" + prj.projectid + "' And UserID = '" + TimeRecorder.UserID + "'");
                MenuWindow.getMain().RefreshGrid();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            int count = gridDetail.Items.Count;
            for (int i = 0; i < count; i++)
            {
                var selectItem = this.gridDetail.Items[0];
                ProjectTimeDetail detailInfo = (ProjectTimeDetail)selectItem;


                SqliteOper sp = new SqliteOper();
                sp.ExecuteNonQuery("Delete From T_PM_Task Where ProjectID = '" + detailInfo.projectid + "' And UserID = '" + TimeRecorder.UserID + "' And TeamID = '" + detailInfo.teamid + "' And Status <> '" + AppConst.Uploaded + "'");
                TimeLeft += detailInfo.tasktime;

                CustomerTeam ct = new CustomerTeam();
                if (detailInfo.teamid != "0")
                    ct.teamid = Convert.ToInt16(detailInfo.teamid);
                else
                    ct.teamid = 0;
                ct.teamname = detailInfo.teamname;

                if (ct.teamid != 0)
                    cbxTeam.Items.Add(ct);
                RefreshGrid();
            }
        }
    }
}
