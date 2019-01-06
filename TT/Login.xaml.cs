using System;
using System.Windows;
using TimeTracker.Util;
using System.Threading;
using System.Data;
using Model;
using Model.Dto;
using Model.WebResult;
using Tstring.DataBase;
using TimeTracker.BLL;
using TT.Util;

namespace TT
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            //SplashScreen s = new SplashScreen(@"Images/Splash.png");
            //s.Show(true);
            //s.Close(new TimeSpan(0, 0, 4));

            //Thread.Sleep(2000);

            InitializeComponent();
            this.Title = ConfigFile.Languege.ReadValue("lblLogin");
            lblUserName.Content = ConfigFile.Languege.ReadValue("lblUser");
            lblPassword.Content = ConfigFile.Languege.ReadValue("lblPwd");
            btnLogin.Content = ConfigFile.Languege.ReadValue("btnLogin");
            btnClose.Content = ConfigFile.Languege.ReadValue("btnExit");

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            //TimeService.Service ts = new TimeService.Service();
            //int uid=ts.CheckPassword(txtUserName.Text.Trim(), txtPassword.Password);
            //if (uid != -1)
            //{
            //    TimeRecorder.Username = txtUserName.Text.Trim();  //Save username.
            //    TimeRecorder.UserID = uid;
            //    this.Hide();
            //    MenuWindow.getMain().Show();
            //}
            //else
            LoginPara para = new LoginPara()
            {
                UserName = txtUserName.Text.Trim(),
                Password = txtPassword.Password
            };
            string str_result = WebCall.PostMethod<LoginPara>(WebCall.Login, para);
            WebResult result = AppUtils.JsonDeserialize<WebResult>(str_result);
            if (result.Code.Equals(SystemConst.MsgSuccess))
            {
                TimeRecorder.Username = txtUserName.Text.Trim();  //Save username.
                TimeRecorder.UserID = Convert.ToInt16(result.Message);
                this.Hide();
                MenuWindow.getMain().Show();
            }
            else
            {
                MessageBox.Show(ConfigFile.Languege.ReadValue("InvalidPassword"), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
