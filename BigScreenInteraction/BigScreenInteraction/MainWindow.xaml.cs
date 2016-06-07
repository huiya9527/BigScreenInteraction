using System;
using System.Windows;
using System.Configuration;
using System.Diagnostics;

namespace BigScreenInteraction
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }

        private void click(object sender, RoutedEventArgs e)
        {
            MouseControlConfiguration mouseConfig = new MouseControlConfiguration();
            mouseConfig.Show();
        }


        //启动手势识别界面
        private void GestureButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonGird.Visibility = Visibility.Collapsed;
            //GestrureDisplayGrid.Visibility = Visibility.Visible;
            //GestureImage.Source = null;
        }

        //启动姿势识别界面 
        private void PostureButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonGird.Visibility = Visibility.Collapsed;
            //PostureDisplayGrid.Visibility = Visibility.Visible;
            //PostureImage.Source = null;
        }

        //离开按钮
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        //用于设置程序始终在最上层
        public void setFullScreen(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }
        //启动用户3D地图,这里用画图程序替代，但是不好操作！如何退出？
        private void Map_3D(object sender, RoutedEventArgs e)
        {
            ButtonGird.Visibility = Visibility.Collapsed;
            try
            {
                //_WBFramework._GRRecognizer.TUIO_MOUSE = true;
                //var drawing = ConfigurationManager.AppSettings["drawing"];
                var drawing = ConfigurationManager.AppSettings["drawing"];
                ProcessHandler.process1 = Process.Start(drawing);

            }
            catch { }
        }
        //启动用户2D地图，这里用记事本程序替代，可能也不太好操作！如何退出？
        private void Map_2D(object sender, RoutedEventArgs e)
        {
            ButtonGird.Visibility = Visibility.Collapsed;
            try
            {

                //_WBFramework._GRRecognizer.TUIO_MOUSE = true;
                var notepad = ConfigurationManager.AppSettings["notepad"];
                ProcessHandler.process2 = Process.Start(notepad);

            }
            catch { }
        }
    }

    public static class ProcessHandler
    {
        public static Process process1;
        public static Process process2;
    }
}