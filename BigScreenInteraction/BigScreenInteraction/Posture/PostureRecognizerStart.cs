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
    class PostureRecognizerStart
    {
        float[][] templates;

        //the posture number 
        private const int postureNumber = 6;

        private const double propertion1 = 0.179 / 0.143;
        private const double propertion2 = 0.164 / 0.179;

        public event PostureEventHandler postureEventHandler;
        protected void AppendEvent(PostureEventHandler peh, PostureEventArgs args)
        {
            if (peh != null)
            {
                peh(this, args);
            }
        }


        private static UniformGrid _PostureCollection;

        public PostureRecognizerStart(Window parent)
        {
            _PostureCollection = FindVisualChild<UniformGrid>(parent, "PostureCollection");
            ReadDefinePosture("PosturesRecord\\record.txt");
        }

        public void recoginze(Body body)
        {
            float[] code = CalculateHashNumber(body);
            int num = SelectPoster(code);
            select_posture("pic"+num.ToString());
            AppendEvent(postureEventHandler, new PostureEventArgs(num.ToString()));
        }

        private void select_posture(String name)
        {
            foreach (var a in _PostureCollection.Children)
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

        private float[] CalculateHashNumber(Body body)
        {
            float[] code = new float[8];

            CameraSpacePoint wristLeft = body.Joints[JointType.WristLeft].Position;
            CameraSpacePoint wristRight = body.Joints[JointType.HandRight].Position;

            CameraSpacePoint elbowLeft = body.Joints[JointType.ElbowLeft].Position;
            CameraSpacePoint elbowRight = body.Joints[JointType.ElbowRight].Position;

            //CameraSpacePoint shoulderCenter = body.Joints[JointType.SpineShoulder].Position;
            CameraSpacePoint shoulderLeft = body.Joints[JointType.ShoulderLeft].Position;
            CameraSpacePoint shoulderRight = body.Joints[JointType.ShoulderRight].Position;

            //坐标平移
            wristLeft.X -= shoulderLeft.X;
            wristLeft.Y -= shoulderLeft.Y;
            wristRight.X -= shoulderRight.X;
            wristRight.Y -= shoulderRight.Y;
            elbowLeft.X -= shoulderLeft.X;
            elbowLeft.Y -= shoulderLeft.Y;
            elbowRight.X -= shoulderRight.X;
            elbowRight.Y -= shoulderRight.Y;

            //比例变化
            double dis1, dis2, std_dis2, prop, moveX, moveY;
            double std_length = getDistance(0, 0, shoulderLeft.X, shoulderLeft.Y);

            dis1 = getDistance(0, 0, elbowLeft.X, elbowLeft.Y);
            dis2 = getDistance(elbowLeft.X, elbowLeft.Y, wristLeft.X, wristLeft.Y);
            std_dis2 = dis1 * propertion2;
            prop = std_dis2 / dis2;
            moveX = wristLeft.X - elbowLeft.X;
            moveY = wristLeft.Y - elbowLeft.Y;
            moveX *= prop;
            moveY *= prop;
            wristLeft.X = (float)(moveX + elbowLeft.X);
            wristLeft.Y = (float)(moveY + elbowLeft.Y);

            code[0] = (float)(wristLeft.X / dis1);
            code[1] = (float)(wristLeft.Y / dis1);
            code[4] = (float)(elbowLeft.X / dis1);
            code[5] = (float)(elbowLeft.Y / dis1);

            dis1 = getDistance(0, 0, elbowRight.X, elbowRight.Y);
            dis2 = getDistance(elbowRight.X, elbowRight.Y, wristRight.X, wristRight.Y);
            std_dis2 = dis1 * propertion2;
            prop = std_dis2 / dis2;
            moveX = wristRight.X - elbowRight.X;
            moveY = wristRight.Y - elbowRight.Y;
            moveX *= prop;
            moveY *= prop;
            wristRight.X = (float)(moveX + elbowRight.X);
            wristRight.Y = (float)(moveY + elbowRight.Y);

            code[2] = (float)(wristRight.X / dis1);
            code[3] = (float)(wristRight.Y / dis1);
            code[6] = (float)(elbowRight.X / dis1);
            code[7] = (float)(elbowRight.Y / dis1);

            if (true)
            {
                String temp = "";
                for (int i = 0; i < 8; i++)
                {
                    temp += code[i].ToString();
                    temp += " ";
                }
            }
            return code;
        }
        private int SelectPoster(float[] code)
        {
            int result = 0;
            double minDiff = Double.MaxValue;
            for (int i = 0; i < templates.Length; i++)
            {
                double diff = compare(code, templates[i]);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    result = i;
                }
            }
            return result;
        }

        private double compare(float[] s1, float[] s2)
        {
            double diff = 0;
            for (int i = 0; i < s1.Length / 2; i++)
            {
                diff += getDistance(s1[i * 2], s1[i * 2 + 1], s2[i * 2], s2[i * 2 + 1]);
            }

            return diff;
        }

        private double getDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
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

        private void ReadDefinePosture(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream, Encoding.Default);
            templates = new float[6][];
            for (int i = 0; i < postureNumber; i++)
            {
                templates[i] = new float[8];
                String str = streamReader.ReadLine();
                String[] strs = str.Split(' ');
                for (int j = 0; j < 8; j++)
                {
                    templates[i][j] = Convert.ToSingle(strs[j]);
                }
            }
        }
    }
}
