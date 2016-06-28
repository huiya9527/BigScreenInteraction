
using System;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.Kinect;
using System.Collections;
using System.Collections.Generic;
using CursorControlLibrary;

namespace BigScreenInteraction
{
    class MouseControl
    {
        private static float mouse_sensity = Properties.Settings.Default.MouseSensitivity;
        private static float cursor_smoothing = Properties.Settings.Default.CursorSmoothing;
        private static bool prime_hand = Properties.Settings.Default.PrimeHand;
        private static bool mouse_click_region = Properties.Settings.Default.MouseClickRegion;
        private static bool middle_button_and_wheel = Properties.Settings.Default.MiddleButtonAndWheel;


        private static HandsState beforeHands;
        private static HandsState nowHand;
        private static ArrayList HandsStateList = new ArrayList();

        public static HandCursorVisualizer cursor = new HandCursorVisualizer();

        private static float lbx = 0;
        private static float lby = 0;
        private static float lnx = 0;
        private static float lny = 0;
        private static int lx = 0;
        private static int ly = 0;

        private static float rbx = 0;
        private static float rby = 0;
        private static float rnx = 0;
        private static float rny = 0;
        private static int rx = 0;
        private static int ry = 0;

        private static bool start = false;

        private const int STATE_RECORD_NUM = 10;
        private static int wheeldy = 0;

        private static int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
        private static int screenHeight = (int)SystemParameters.PrimaryScreenHeight;

        private static void MouseLeftDown()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            Console.WriteLine("mouse left down");
        }
        private static void MouseLeftUp()
        {
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            Console.WriteLine("mouse left up");
        }

        private static void MouseRightDown()
        {
            mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);
            Console.WriteLine("mouse right down");
        }

        private static void MouseRightUp()
        {
            mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero);
            Console.WriteLine("mouse right up");
        }

        private static void MouseMiddleDown()
        {
            mouse_event(MouseEventFlag.MiddleDown, 0, 0, 0, UIntPtr.Zero);
            Console.WriteLine("mouse middle down");
        }

        private static void MouseMiddleUp()
        {
            mouse_event(MouseEventFlag.MiddleUp, 0, 0, 0, UIntPtr.Zero);
            Console.WriteLine("mouse middle up");
        }

        private static void MouseRoll(int dy)
        {
            mouse_event(MouseEventFlag.Wheel, 0, 0, dy, UIntPtr.Zero);
        }
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, int data, UIntPtr extraInfo);

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        private static void StateClear()
        {
            if (beforeHands.operation == Operation.left_down)
            {
                MouseLeftUp();
            }
            else if (beforeHands.operation == Operation.middle_down)
            {
                MouseMiddleUp();
            }
            else if (beforeHands.operation == Operation.right_down)
            {
                MouseRightUp();
            }
        }

        private static void move(float nx, float ny, float bx, float by, bool mouse, bool leftHand)
        {
            float dx = nx - bx;
            float dy = ny - by;
            // get current cursor position
            Point curPos = MouseControl.GetCursorPosition();
            // smoothing for using should be 0 - 0.95f. The way we smooth the cusor is: oldPos + (newPos - oldPos) * smoothValue
            float smoothing = 1 - cursor_smoothing;
            // set cursor position
            
            if (leftHand)
            {
                lx = (int)(lx + (dx * mouse_sensity * screenWidth) * smoothing);
                ly = (int)(ly - (dy * mouse_sensity * screenHeight) * smoothing);
                if (lx < 0) lx = 0;
                if (lx > screenWidth) lx = screenWidth;
                if (ly < 0) ly = 0;
                if (ly > screenHeight) ly = screenHeight;
                nowHand.input._dx = lx;
                nowHand.input._dy = ly;
                if (!mouse)
                {
                    MouseControl.SetCursorPos(lx, ly);
                }
            }
            else
            {
                rx = (int)(rx + (dx * mouse_sensity * screenWidth) * smoothing);
                ry = (int)(ry - (dy * mouse_sensity * screenHeight) * smoothing);
                if (rx < 0) rx = 0;
                if (rx > screenWidth) rx = screenWidth;
                if (ry < 0) ry = 0;
                if (ry > screenHeight) ry = screenHeight;
                nowHand.input._dx1 = rx;
                nowHand.input._dy1 = ry;
                if (mouse)
                {
                    MouseControl.SetCursorPos(rx, ry);
                }
            }
        }

        private static HandsState findState()
        {
            Dictionary<Operation, int> dic =
            new Dictionary<Operation, int>();

            for (int i = 0; i < HandsStateList.Count; i++)
            {
                HandsState temp = (HandsState)HandsStateList[i];
                if (dic.ContainsKey(temp.operation))
                {
                    dic[temp.operation] += 1;
                }
                else
                {
                    dic.Add(temp.operation, 1);
                }
            }

            int num = 0;
            Operation maxOperation = Operation.no_operation;
            foreach (KeyValuePair<Operation, int> kvp in dic)
            {
                if (kvp.Value >= num)
                {
                    num = kvp.Value;
                    maxOperation = kvp.Key;
                }
            }

            for (int i = HandsStateList.Count-1; i >= 0; i--)
            {
                HandsState temp = (HandsState)HandsStateList[i];
                if (temp.operation == maxOperation)
                {
                    return temp;
                }
            }
            //should not be here;
            return null;
        }

        private static void cal_ave(ref float nx, ref float ny, ref float bx, ref float by, bool isRight)
        {
            float sumX = 0;
            float sumY = 0;
            for (int i = 0; i < HandsStateList.Count; i++)
            {
                HandsState temp = (HandsState)HandsStateList[i];
                if (isRight)
                {
                    sumX += temp.wristRight.X;
                    sumY += temp.wristRight.Y;
                }
                else
                {
                    sumX += temp.wristLeft.X;
                    sumY += temp.wristLeft.Y;
                }
                  
            }
            if (!start)
            {
                start = true;
                nx = sumX / HandsStateList.Count;
                ny = sumY / HandsStateList.Count;
                bx = nx;
                by = ny;
            }
            else
            {
                bx = nx;
                by = ny;
                nx = sumX / HandsStateList.Count;
                ny = sumY / HandsStateList.Count;
            }

        }

        public static void Mouse_Driver(Body body)
        {
            cal_ave(ref lnx, ref lny, ref lbx, ref lby, false);
            cal_ave(ref rnx, ref rny, ref rbx, ref rby, true);

            if (nowHand != null)
            {
                beforeHands = nowHand;
                if (HandsStateList.Count >= STATE_RECORD_NUM)
                {
                    HandsStateList.RemoveAt(0);
                }
                HandsStateList.Add(new HandsState(body));
                nowHand = findState();
            }
            else
            {
                beforeHands = new HandsState();
                nowHand = new HandsState(body);
                HandsStateList.Add(nowHand);
            }
            
            //operation start
            if (nowHand.operation == Operation.no_operation)
            {
                StateClear();
                MouseControl.SetCursorPos(screenWidth / 2, screenHeight / 2);
                lx = rx  = screenWidth / 2;
                ly = ry = screenHeight / 2;

            }
            else if (nowHand.operation == Operation.left_down)
            {
                //left
                move(lnx,lny, lbx, lby, nowHand.isRight , true);
                //right
                move(rnx, rny, rbx, rby, nowHand.isRight , false);
                if (beforeHands.operation == Operation.left_down)
                {
                    return;
                }
                else if (beforeHands.operation == Operation.middle_down)
                {
                    MouseMiddleUp();
                }
                else if (beforeHands.operation == Operation.right_down)
                {
                    MouseRightUp();
                }
                MouseLeftDown();
            }
            else if (nowHand.operation == Operation.middle_down)
            {
                //left
                move(lnx, lny, lbx, lby, nowHand.isRight, true);
                //right
                move(rnx, rny, rbx, rby, nowHand.isRight, false);
                if (beforeHands.operation == Operation.left_down)
                {
                    MouseLeftUp();
                }
                else if (beforeHands.operation == Operation.middle_down)
                {
                    return;
                }
                else if (beforeHands.operation == Operation.right_down)
                {
                    MouseRightUp();
                }
                MouseMiddleDown();
            }
            else if (nowHand.operation == Operation.right_down)
            {
                //left
                move(lnx, lny, lbx, lby, nowHand.isRight, true);
                //right
                move(rnx, rny, rbx, rby, nowHand.isRight, false);
                if (beforeHands.operation == Operation.left_down)
                {
                    MouseLeftUp();
                }
                else if (beforeHands.operation == Operation.middle_down)
                {
                    MouseMiddleUp();
                }
                else if (beforeHands.operation == Operation.right_down)
                {
                    return;
                }
                MouseRightDown();
            }
            else if (nowHand.operation == Operation.move)
            {
                StateClear();
                //left
                move(lnx, lny, lbx, lby, nowHand.isRight, true);
                //right
                move(rnx, rny, rbx, rby, nowHand.isRight, false);
            }
            else if (nowHand.operation == Operation.wheel)
            {
                StateClear();
                //wheel
                if (beforeHands.operation != Operation.wheel)
                {
                    wheeldy = beforeHands.primeHandy;
                }
                int dy = nowHand.primeHandy - wheeldy;
                MouseRoll(dy);
            }
            cursor.UpdateHandCursor(nowHand.input);
        }

        class HandsState
        {
            //read file
            private const float TOUCH_REGION = 0.15f;
            private const float EMBED_REGION = 0.35f;

            //for move
            public CameraSpacePoint wristLeft;
            public CameraSpacePoint wristRight;
            //for compare
            public Operation operation;
            //for wheel
            public int primeHandy;
            //for cursor
            public HandInput input;
            public bool isRight;

            private HandPositionZ LeftHandPosition;
            private HandPositionZ RightHandPosition;
            private HandState LeftHandState;
            private HandState RightHandState;
            private HandPositionZ SelectHandPosition;
            private HandState SelectHandState;


            public HandsState(Body body)
            {
                CameraSpacePoint handLeft = body.Joints[JointType.HandLeft].Position;
                CameraSpacePoint handRight = body.Joints[JointType.HandRight].Position;
                wristLeft = body.Joints[JointType.WristLeft].Position;
                wristRight = body.Joints[JointType.WristRight].Position;
                CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;

                input = new HandInput();
                input.isLeftGrip = (body.HandLeftState == HandState.Closed);
                input.isRightGrip = (body.HandRightState == HandState.Closed);
                
                //select wrist
                if (prime_hand)
                {
                    primeHandy = (int)(wristRight.Y * 100);
                    isRight = true;
                }
                else
                {
                    primeHandy = (int)(wristLeft.Y * 100);
                    isRight = false;
                }
                //set left hand position
                float leftDepth = spineBase.Z - handLeft.Z;
                input._LPressExtent = (leftDepth - TOUCH_REGION) / (EMBED_REGION - TOUCH_REGION) ;
                if (leftDepth > EMBED_REGION)
                {
                    LeftHandPosition = HandPositionZ.EMBED;
                }
                else if (leftDepth > TOUCH_REGION)
                {
                    LeftHandPosition = HandPositionZ.TOUCH;
                }
                else
                {
                    LeftHandPosition = HandPositionZ.UNKNOW;
                }
                //set right hand position
                float rightDepth = spineBase.Z - handRight.Z;
                input._RPressExtent = (rightDepth -  TOUCH_REGION) / (EMBED_REGION - TOUCH_REGION);
                if (rightDepth > EMBED_REGION)
                {
                    RightHandPosition = HandPositionZ.EMBED;
                }
                else if (rightDepth > TOUCH_REGION)
                {
                    RightHandPosition = HandPositionZ.TOUCH;
                }
                else
                {
                    RightHandPosition = HandPositionZ.UNKNOW;
                }
                //set left hand state
                LeftHandState = body.HandLeftState;
                //set right hand state
                RightHandState = body.HandRightState;

                //no hand
                if (LeftHandPosition == HandPositionZ.UNKNOW && RightHandPosition == HandPositionZ.UNKNOW)
                {
                    operation = Operation.no_operation;
                    input._isWhich = 0;
                }
                //single hand
                else if (LeftHandPosition == HandPositionZ.UNKNOW || RightHandPosition == HandPositionZ.UNKNOW)
                {
                    //left hand operate
                    if (LeftHandPosition != HandPositionZ.UNKNOW)
                    {
                        SelectHandPosition = LeftHandPosition;
                        SelectHandState = LeftHandState;
                        input._isWhich = 1;
                        isRight = false;
                    }
                    //right hand operate
                    else
                    {
                        SelectHandPosition = RightHandPosition;
                        SelectHandState = RightHandState;
                        input._isWhich = 2;
                        isRight = true;
                    }
                    //single hand touch region
                    if (SelectHandPosition == HandPositionZ.TOUCH)
                    {
                        if (SelectHandState == HandState.Closed)
                        {
                            if (mouse_click_region)
                            {
                                operation = Operation.left_down;
                            }
                            else
                            {
                                operation = Operation.right_down;
                            }

                        }
                        else
                        {
                            operation = Operation.move;
                        }
                    }
                    //single hand embed region
                    else
                    {
                        if (SelectHandState == HandState.Closed)
                        {
                            if (mouse_click_region)
                            {
                                operation = Operation.right_down;
                            }
                            else
                            {
                                operation = Operation.left_down;
                            }
                        }
                        else
                        {
                            operation = Operation.move;
                        }
                    }
                }
                else
                {
                    //two hand closed will operate wheel
                    input._isWhich = 3;
                    if (LeftHandState == HandState.Closed && RightHandState == HandState.Closed)
                    {
                        if (middle_button_and_wheel)
                        {
                            operation = Operation.wheel;
                        }
                        else
                        {
                            operation = Operation.middle_down;
                        }

                    }
                    //one hand closed 
                    else if (LeftHandState == HandState.Closed || RightHandState == HandState.Closed)
                    {
                        if (middle_button_and_wheel)
                        {
                            operation = Operation.middle_down;
                        }
                        else
                        {
                            operation = Operation.wheel;
                        }
                    }
                    else
                    {
                        operation = Operation.move;
                    }
                }
            }

            public HandsState()
            {
                operation = Operation.no_operation;
            }
        }

        public enum HandPositionZ
        {
            UNKNOW = 0,
            TOUCH = 1,
            EMBED = 2,
        }

        public enum Operation
        {
            no_operation = 0,
            left_down = 1,
            middle_down = 2,
            right_down = 3,
            move = 4,
            wheel = 5,
        }

        [Flags]
        public enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
    }
}
