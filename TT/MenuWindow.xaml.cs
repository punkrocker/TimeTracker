using System;
using System.Windows;
using System.Windows.Input;
using TT.Util;
using System.Windows.Controls;
using TT.Model;
using System.Collections.Generic;
using Tstring.DataBase;
using System.Data;
using Model;
using Model.WebResult;
using TimeTracker.Util;

namespace TT
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MenuWindow : Window
    {
        #region singleton
        public static MenuWindow wm;
        public static MenuWindow getMain()
        {
            if (wm == null)
                wm = new MenuWindow();

            return wm;
        }
        #endregion


        private MenuWindow()
        {
            InitializeComponent();
            this.Top = 0;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            BindProjects();
        }

        public void ChangeLabel(string _value)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                txtTime.Text = _value;
            }));
        }

        public void ChangeProject(string _prjName)
        {
            txtProject.Text = _prjName;
        }

        #region Control Method
        private static void StartRecord()
        {
            try
            {
                TimeRecorder.getRecorder().StartRecord();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void PauseRecord()
        {
            try
            {
                TimeRecorder.getRecorder().StopRecord();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void StopRecord()
        {
            try
            {
                if (TimeRecorder.getRecorder().RecordingProject != null)
                {
                    if (MessageBox.Show(ConfigFile.Languege.ReadValue("ClearTaskTime"), "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        TimeRecorder.getRecorder().ClearTime();
                }
                else
                    MessageBox.Show(ConfigFile.Languege.ReadValue("NoSelectedFile"), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void FinishDay()
        {
            try
            {
                if (TimeRecorder.getRecorder().IsRecording)
                    TimeRecorder.getRecorder().StopRecord();

                FinishDay fd = new FinishDay();
                fd.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #region event
        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            StartRecord();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            PauseRecord();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopRecord();
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            FinishDay();
        }

        private void btnProjectList_Click(object sender, RoutedEventArgs e)
        {
            //ProjectList.getProjectList().Show();
            if (gridProject.Visibility == Visibility.Visible)
                gridProject.Visibility = Visibility.Hidden;
            else
                gridProject.Visibility = Visibility.Visible;
        }

        private void gridProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        #endregion



        private void BindProjects()
        {
            try
            {
                List<KeyValuePair<string, string>> paramlist = new List<KeyValuePair<string, string>>();
                paramlist.Add(new KeyValuePair<string, string>("userid", TimeRecorder.UserID.ToString()));
                var userProjectResult = WebCall.GetMethod<UserProjectResult>(WebCall.GetProjects, paramlist);

                SqliteOper sp = new SqliteOper();
                sp.ExecuteNonQuery("Delete From T_PM_Project");
                if (userProjectResult.Code == SystemConst.MsgSuccess)
                    foreach (var userProject in userProjectResult.Data)
                    {
                        SqliteOper spi = new SqliteOper();
                        spi.ExecuteNonQuery(
                            "Insert Into T_PM_Project (ProjectID,ProjectCode,ProjectName,CustomerID,[Status]) Values ('" +
                            userProject.ProjectID + "','" + userProject.ProjectCode + "','" +
                            userProject.ProjectCode + "','" + userProject.CustomerID + "','1');");
                        string sql = "Select count(1) as count From T_PM_USERTIME Where ProjectID = '" +
                                     userProject.ProjectID + "' And UserID = '" + TimeRecorder.UserID + "'";
                        if (spi.ExecuteDataSet(sql).Tables[0].Rows[0]["count"].ToString().Equals("0"))
                        {
                            spi.ExecuteNonQuery("Insert Into T_PM_USERTIME (ProjectID,UserID,TaskTime) Values ('" +
                                                userProject.ProjectID + "','" + TimeRecorder.UserID +
                                                "','0')");
                        }
                    }
                else
                {
                    MessageBox.Show(userProjectResult.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public void RefreshGrid()
        {
            List<ProjectInfo> memberData = new List<ProjectInfo>();
            SqliteOper sp = new SqliteOper();
            DataSet ds = sp.ExecuteDataSet("SELECT * FROM T_PM_Project T1 Left Join T_PM_UserTime T2 On T1.[ProjectID] = T2.ProjectID Where T2.UserID = '" + TimeRecorder.UserID + "' And (T1.Status = '" + AppConst.Downloaded + "' OR T1.Status = '" + AppConst.Executing + "')");
            if (ds != null && ds.Tables.Count != 0)
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ProjectInfo item = new ProjectInfo(dr);
                    memberData.Add(item);
                }
            gridProject.DataContext = memberData;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            SqliteOper sp = new SqliteOper();
            string count = sp.ExecuteDataSet("Select Count(1) as ACount From T_PM_Task Where Status <> '" + AppConst.Uploaded + "'").Tables[0].Rows[0]["ACount"].ToString();
            string prjcount = sp.ExecuteDataSet("Select Count(1) as ACount From T_PM_Project Where Status = '" + AppConst.Executing + "'").Tables[0].Rows[0]["ACount"].ToString();
            if (count.Equals("0") && prjcount.Equals("0"))
                Application.Current.Shutdown();
            else
                MessageBox.Show(ConfigFile.Languege.ReadValue("NotUploaded"), "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void itemsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (TimeRecorder.getRecorder().RecordingProject == null)
                MenuWindow.getMain().ChangeProject((gridProject.SelectedItem as ProjectInfo).projectname);
                TimeRecorder.getRecorder().RecordingProject = gridProject.SelectedItem as ProjectInfo;
            //TimeRecorder.QueueProject = gridProject.SelectedItem as ProjectInfo;
            gridProject.Visibility = Visibility.Hidden;
        }

        public void ChangeRecordingStatu(bool _isRecording)
        {
            btnProjectList.IsEnabled = !_isRecording;
            if (_isRecording)
            {
                gridProject.Visibility = Visibility.Hidden;
                this.lblTitle.Content = ConfigFile.Languege.ReadValue("mainTitle");
            }
            else
            {
                gridProject.Visibility = Visibility.Visible;
                this.lblTitle.Content = "UnionServ TimeTracker";
            }
        }

    }
}
