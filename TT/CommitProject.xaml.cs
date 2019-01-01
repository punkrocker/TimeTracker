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
using Tstring.DataBase;

namespace TT
{
    /// <summary>
    /// CommitProject.xaml 的交互逻辑
    /// </summary>
    public partial class CommitProject : Window
    {
        private ProjectTimeInfo prj;
        public CommitProject(ProjectTimeInfo _prj)
        {
            InitializeComponent();
            prj = _prj;
            InitUI();
        }

        private void InitUI()
        {
            lblUser.Content = ConfigFile.Languege.ReadValue("lblCurrentUser");
            lblPrj.Content = ConfigFile.Languege.ReadValue("lblCurrentProject");
            lblTask.Content = ConfigFile.Languege.ReadValue("lblTask");
            btnCommit.Content = ConfigFile.Languege.ReadValue("btnCommit");
            btnClose.Content = ConfigFile.Languege.ReadValue("btnClose");

            tbUser.Text = TimeRecorder.Username;
            tbPrj.Text = prj.projectname;
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Convert.ToInt16(txtActual.Text) > Convert.ToInt16(txtPlan.Text))
                {
                    MessageBox.Show(ConfigFile.Languege.ReadValue("CountError"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            TimeService.Service ts = new TimeService.Service();
            if (ts.CommitProject(prj.projectid, TimeRecorder.UserID.ToString(), prj.tasktime.ToString(), txtActual.Text + "/" + txtPlan.Text + "." + tbComment.Text))
            {
                SqliteOper sp = new SqliteOper();
                sp.ExecuteNonQuery("update T_PM_UserTime set SubmitTime = datetime('now'),[Status] = '3' Where UserID = '" + TimeRecorder.UserID.ToString() + "' And ProjectID = '" + prj.projectid + "'");
                this.Close();
            }
            else
                MessageBox.Show(ConfigFile.Languege.ReadValue("SaveError"), "", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
