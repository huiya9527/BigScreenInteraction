﻿using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Kinect;
using System.Windows.Controls;


namespace BigScreenInteraction
{
    class KinectControl
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        KinectSensor sensor;
        /// <summary>
        /// Reader for body frames
        /// </summary>
        BodyFrameReader bodyFrameReader;
        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;
        /// <summary>
        /// Screen width and height for determining the exact mouse sensitivity
        /// </summary>
        int screenWidth, screenHeight;

        public bool control_mouse = true;


        Point lastCurPos = new Point(0, 0);
        GestureRecognizerStart grs;
        PostureRecognizerStart prs;


        public KinectControl(GestureRecognizerStart _grs, PostureRecognizerStart _prs)
        {
            // get Active Kinect Sensor
            sensor = KinectSensor.GetDefault();
            // open the reader for the body frames
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            // get screen with and height
            screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            grs = _grs;
            prs = _prs;
           
            // open the sensor
            sensor.Open();
        }

        /// <summary>
        /// Read body frames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived)
            {
                return;
            }

            foreach (Body body in this.bodies)
            {

                // get first tracked body only, notice there's a break below.
                if (body.IsTracked)
                {
                    //鼠标控制
                    if (control_mouse)
                    {
                        MouseControl.Mouse_Driver(body);
                    }
                    //动作识别
                    grs.recoginze(body);
                    //姿势识别
                    prs.recoginze(body);
                    
                }
            }
        }
    }
}
