using System;
using System.Windows;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LoadingControl.Control;
using System.Threading;
using System.Management;
using CursorControlLibrary;

namespace BigScreenInteraction
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        KinectControl kinectCtrl;
        GestureRecognizerStart grs;
        PostureRecognizerStart prs;
        private Timer timer;
        HandCursorVisualizer HCV;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grs = new GestureRecognizerStart(this);
            prs = new PostureRecognizerStart(this);
            prs.postureEventHandler += new PostureEventHandler(this.OnPostureEvent);
            kinectCtrl = new KinectControl(grs, prs);
            HCV = MouseControl.cursor;
            full_screen.Children.Add(HCV);
        }

        //启动手势识别界面
        private void GestureButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonGird.Visibility = Visibility.Collapsed;
            GestrureDisplayGrid.Visibility = Visibility.Visible;
            HCV.Visibility = Visibility.Collapsed;
            kinectCtrl.control_mouse = false;
        }
        //启动姿势识别界面 
        private void PostureButtonClick(object sender, RoutedEventArgs e)
        {
            ButtonGird.Visibility = Visibility.Collapsed;
            PostureDisplayGrid.Visibility = Visibility.Visible;
            HCV.Visibility = Visibility.Collapsed;
            kinectCtrl.control_mouse = false;
        }
        //离开按钮
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        //启动用户3D地图,这里用画图程序替代，但是不好操作！如何退出？
        private void Map_3D(object sender, RoutedEventArgs e)
        {
            ButtonGird.Visibility = Visibility.Collapsed;
            try
            {
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
                var notepad = ConfigurationManager.AppSettings["notepad"];
                ProcessHandler.process2 = Process.Start(notepad);
            }
            catch
            {
            }
        }

       

        private void OnPostureEvent(object sender, PostureEventArgs e)
        {
            
            //姿势1对应功能，显示项目选择界面
            if (e.EventName == "1")
            {
                if (timer == null)
                {
                    //这里显示动画，动画结束时kill进程或者返回！
                    Loaded_animation.Visibility = Visibility.Visible;
                    timer = new Timer(callBack, null, 2000, 1);
                }
            }
            else
            {
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                    Loaded_animation.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void callBack(object o)
        {
            kinectCtrl.control_mouse = true;
            this.ButtonGird.Dispatcher.Invoke(
                new Action(
                     delegate
                     {
                         this.ButtonGird.Visibility = Visibility.Visible;
                     }
                )
            );
            this.GestrureDisplayGrid.Dispatcher.Invoke(
                new Action(
                     delegate
                     {
                         this.GestrureDisplayGrid.Visibility = Visibility.Collapsed;
                     }
                )
            );
            this.PostureDisplayGrid.Dispatcher.Invoke(
                new Action(
                     delegate
                     {
                         this.PostureDisplayGrid.Visibility = Visibility.Collapsed;
                     }
                )
            );
            this.HCV.Dispatcher.Invoke(
                new Action(
                     delegate
                     {
                         this.HCV.Visibility = Visibility.Visible;
                     }
                )
            );

            if (ProcessHandler.process1 != null)
            {
                ProcessHandler.process1.Kill();
                ProcessHandler.process1 = null;
            }

            if (ProcessHandler.process2 != null)
            {
                ProcessHandler.process2.Kill();
                ProcessHandler.process2 = null;
            }
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }



        public static class ProcessHandler
        {
            public static Process process1;
            public static Process process2;
        }
    }
}