using System;
using System.Windows;
using System.Windows.Input;
using TT.Util;

namespace TT
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow wm;
        public static MainWindow getMain()
        {
            if (wm == null)
                wm = new MainWindow();
            return wm;
        }

        private MainWindow()
        {
            InitializeComponent();
            ProjectList.getProjectList().Hide();
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProjectList.getProjectList().Show();
        }

        public void ChangeLabel(string _value)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                txtTime.Text = _value;
            }));
        }

        private void btnRecord_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TimeRecorder.getRecorder().StartRecord();
        }

        private void btnStop_Click(object sender, MouseButtonEventArgs e)
        {
            TimeRecorder.getRecorder().StopRecord();
        }
    }
}
