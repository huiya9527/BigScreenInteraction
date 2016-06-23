using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using Recognizer.Dollar;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace BigScreenInteraction
{
    class GestureRecognizerStart
    {
        bool primeHand = true;
        private Recognizer.Dollar.Recognizer _rec;
        private bool start = false;
        private ArrayList _points;
        private  Canvas _m_canvas;
        private static UniformGrid _GestureCollection;
        

        public GestureRecognizerStart(Window parent)
        {
            _m_canvas = FindVisualChild<Canvas>(parent, "m_canvas");
            
            _GestureCollection = FindVisualChild<UniformGrid>(parent, "GestureCollection");
            _rec = new Recognizer.Dollar.Recognizer();
            _points = new ArrayList(256);
            LoadGestureFiles();
        }

       

        private void LoadGestureFiles()
        {
            String path = @"GesturesRecord\";

            var files = Directory.GetFiles(path, "*.xml");

            foreach (var file in files)
            {
                Console.WriteLine(file);
                _rec.LoadGesture(file);
            }
        }

        private void mouse_down(float x, float y)
        {
            Console.WriteLine("mouse_left_down");
            _points.Clear();
            _m_canvas.Children.Clear();
            draw(x, y);
            _points.Add(new PointR(x, y, Environment.TickCount));
        }

        private void mouse_move(float x, float y)
        {
            Console.WriteLine("move");
            draw(x, y);
            _points.Add(new PointR(x, y, Environment.TickCount));
        }

        private void mouse_up()
        {
            Console.WriteLine("mouse_left_up");
            if (_points.Count >= 5) // require 5 points for a valid gesture
            {
                if (_rec.NumGestures > 0) // not recording, so testing
                {

                    NBestList result = _rec.Recognize(_points); // where all the action is!!
                    select_posture(result.Name);
                }
            }
        }

        private void select_posture(String name)
        {
            foreach (var a in _GestureCollection.Children)
            {
                if (name.StartsWith(((Grid)a).Name))
                {
                    ((Grid)a).Background = new SolidColorBrush(Colors.LightBlue);
                }
                else
                {
                    ((Grid)a).Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        private void draw(float x, float y)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Fill = new SolidColorBrush(Colors.Red);
            ellipse.Width = 4;
            ellipse.Height = 4;
            Canvas.SetLeft(ellipse, x * _m_canvas.Width);
            Canvas.SetTop(ellipse, y * _m_canvas.Height);

            ellipse.Visibility = Visibility.Visible;
            _m_canvas.Children.Add(ellipse);
        }

        public void recoginze(Body body)
        {
            CameraSpacePoint handLeft = body.Joints[JointType.HandLeft].Position;
            CameraSpacePoint handRight = body.Joints[JointType.HandRight].Position;
            HandState leftHandState = body.HandLeftState;
            HandState rightHandState = body.HandRightState;

            float x = 0;
            float y = 0;
            HandState selectHandState = HandState.Unknown;

            if (primeHand)
            {
                x = handRight.X + 0.1f;
                y = handRight.Y + 0.4f;
                selectHandState = rightHandState;
            }
            else
            {
                x = handLeft.X - 0.1f;
                y = handLeft.Y + 0.4f;
                selectHandState = leftHandState;
            }
            //reverse y
            y = 1 - y;

            if (selectHandState == HandState.Open)
            {
                if (start)
                {
                    mouse_up();
                    start = false;
                }
            }
            else if (selectHandState == HandState.Closed)
            {
                if (start)
                {
                    mouse_move(x, y);
                }
                else
                {
                    mouse_down(x, y);
                    start = true;
                }
            }
            else
            {
                if (start)
                {
                    mouse_move(x, y);
                }
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj, string name) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem && ((FrameworkElement)child).Name == name)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child, name);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
