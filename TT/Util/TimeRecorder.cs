using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Timers;
using Tstring.DataBase;
using TimeTracker.Util;
using TT.Model;

namespace TT.Util
{
    public class TimeRecorder
    {
        private static TimeRecorder Recorder;
        public static TimeRecorder getRecorder()
        {
            if (Recorder == null)
                Recorder = new TimeRecorder();

            return Recorder;
        }

        //public static ProjectInfo QueueProject;
        //public int RecordingProjectID = -1;
        public static string Username = "";
        public static int UserID = -1;

        private ProjectInfo recordingPrj = null;
        public ProjectInfo RecordingProject
        {
            get
            {
                return recordingPrj;
            }
            set
            {
                recordingPrj = value;
                if (value == null)
                    MenuWindow.getMain().ChangeProject(">>NO PROJECT<<");
                else
                {
                    MenuWindow.getMain().ChangeProject(value.projectname);
                    SqliteOper sp = new SqliteOper();
                    string sql = "SELECT TaskTime FROM T_PM_UserTime Where ProjectID = '" + RecordingProject.projectid + "' And UserID = '" + TimeRecorder.UserID + "' And Status <> '" + AppConst.Uploaded + "'";
                    TaskTime = Convert.ToInt32(sp.ExecuteScalar(sql).ToString());
                }
            }
        }

        private bool isRecording = false;
        public bool IsRecording
        {
            get
            {
                return isRecording;
            }
            set
            {
                isRecording = value;
                MenuWindow.getMain().ChangeRecordingStatu(isRecording);
            }
        }

        private int taskTime = 0;
        public int TaskTime
        {
            get
            {
                //if (taskTime == 0)
                //    return 1;
                //else
                    return taskTime;
            }
            set
            {
                taskTime = value;

                int hour = 0, min = 0, second = 0;
                hour = taskTime / 3600;
                min = min = taskTime / 60 % 60;
                second = taskTime % 60;
                MenuWindow.getMain().ChangeLabel(hour.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0') + ":" + second.ToString().PadLeft(2, '0'));
            }
        }

        private TimeRecorder()
        {
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;    // 1秒 = 1000毫秒
        }

        public void StartRecord()
        {
            if (!IsRecording)
            {
                //if (TimeRecorder.QueueProject == null)
                if (RecordingProject == null)
                {
                    MessageBox.Show(ConfigFile.Languege.ReadValue("NoSelectedFile"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    SqliteOper sp = new SqliteOper();
                    int timeleft = 0;
                    //RecordingProject = TimeRecorder.QueueProject;
                    try
                    {
                        timeleft = Convert.ToInt16(sp.ExecuteDataSet("SELECT TaskTime FROM T_PM_UserTime Where ProjectID = '" + RecordingProject.projectid + "' And UserID = '" + TimeRecorder.UserID + "'").Tables[0].Rows[0]["tasktime"]);
                        sp.ExecuteNonQuery("update T_PM_UserTime set Status = '" + AppConst.Executing + "' Where UserID = '" + TimeRecorder.UserID + "' And ProjectID = '" + RecordingProject.projectid + "';update T_PM_Project set Status = '" + AppConst.Executing + "' Where ProjectID = '" + RecordingProject.projectid + "';");
                    }
                    catch
                    {
                        MessageBox.Show(ConfigFile.Languege.ReadValue("HasRecording"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {

                    }
                    this.TaskTime = timeleft;
                    IsRecording = true;
                    aTimer.Start();
                }
            }
            else
                MessageBox.Show(ConfigFile.Languege.ReadValue("HasRecording"), "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void StopRecord()
        {
            if (IsRecording)
            {
                aTimer.Stop();
                IsRecording = false;
            }
            else
                MessageBox.Show(ConfigFile.Languege.ReadValue("NoRecording"), "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ClearTime()
        {
            aTimer.Stop();
            SqliteOper sp = new SqliteOper();
            sp.ExecuteNonQuery("update T_PM_UserTime set TaskTime = 0,Status = '1' Where UserID = '" + TimeRecorder.UserID + "' And ProjectID = '" + RecordingProject.projectid + "';update T_PM_Project set Status = '" + AppConst.Downloaded + "' WHere ProjectID = '" + RecordingProject.projectid + "'");
            TaskTime = 0;
            IsRecording = false;
        }

        System.Timers.Timer aTimer = new System.Timers.Timer();

        /// <summary>
        /// Timer的Elapsed事件处理程序
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //写入数据库
            SqliteOper sp = new SqliteOper();
            sp.ExecuteNonQuery("update T_PM_UserTime set TaskTime = TaskTime + 1,Status = '2' Where UserID = '" + TimeRecorder.UserID + "' And ProjectID = '" + RecordingProject.projectid + "' And (Status = '1' Or Status = '2')");
            //改taskTime
            TaskTime++;
        }
    }
}
